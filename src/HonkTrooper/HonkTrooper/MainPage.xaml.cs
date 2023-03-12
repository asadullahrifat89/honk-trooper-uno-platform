using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;
using Windows.Graphics.Display;

namespace HonkTrooper
{
    public sealed partial class MainPage : Page
    {
        #region Fields

        private readonly Scene _scene_game;
        private readonly Scene _scene_main_menu;
        private readonly Controller _game_controller;

        private readonly HealthBar _player_health_bar;
        private readonly HealthBar _boss_health_bar;
        private readonly HealthBar _powerUp_health_bar;

        private readonly ScoreBar _game_score_bar;
        private readonly StackPanel _health_bars;

        private readonly Random _random;
        private Player _player;

        #endregion

        #region Ctor

        public MainPage()
        {
            this.InitializeComponent();

            _scene_game = this.GameScene;
            _scene_main_menu = this.MainMenuScene;
            _player_health_bar = this.PlayerHealthBar;
            _boss_health_bar = this.BossHealthBar;
            _powerUp_health_bar = this.PowerUpHealthBar;

            _game_controller = this.GameController;
            _game_score_bar = this.GameScoreBar;
            _health_bars = this.HealthBars;

            ToggleHudVisibility(Visibility.Collapsed);

            _random = new Random();

            Loaded += MainPage_Loaded;
            Unloaded += MainPage_Unloaded;
        }

        #endregion

        #region Methods

        #region Game

        private void PauseGame()
        {
            ToggleHudVisibility(Visibility.Collapsed);

            _scene_game.Pause();
            _scene_main_menu.Play();

            GenerateTitleScreenInScene("Game Paused");
        }

        private void ResumeGame(TitleScreen se)
        {
            ToggleHudVisibility(Visibility.Visible);

            _scene_game.Play();
            _scene_main_menu.Pause();

            RecycleTitleScreen(se);

            _game_controller.AttackButton.Focus(FocusState.Programmatic);
        }

        private void NewGame(TitleScreen se)
        {
            _game_controller.Reset();

            GeneratePlayerInScene();

            _powerUp_health_bar.Reset();
            _boss_health_bar.Reset();

            _game_score_bar.Reset();

            // if there is a boss already in the picture then remove it
            if (_scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating) is Boss boss)
            {
                boss.Health = 0;
                boss.IsAttacking = false;
                boss.SetPosition(left: -500, top: -500);

                boss.IsAnimating = false;

                Console.WriteLine("Boss relocated");
            }

            foreach (var powerUpPickup in _scene_game.Children.OfType<PowerUpPickup>())
            {
                powerUpPickup.SetPosition(
                     left: -500,
                     top: -500);

                powerUpPickup.IsAnimating = false;
            }

            foreach (var healthPickup in _scene_game.Children.OfType<HealthPickup>())
            {
                healthPickup.SetPosition(
                     left: -500,
                     top: -500);

                healthPickup.IsAnimating = false;
            }

            _scene_game.SceneState = SceneState.GAME_RUNNING;

            if (!_scene_game.IsAnimating)
                _scene_game.Play();

            _scene_main_menu.Pause();

            RecycleTitleScreen(se);
            ToggleHudVisibility(Visibility.Visible);

            ScreenExtensions.EnterFullScreen(true);

            _game_controller.AttackButton.Focus(FocusState.Programmatic);
        }

        private void GameOver()
        {
            // if player is dead game keeps playing in the background but scene state goes to game over
            if (_player.IsDead)
            {
                _scene_main_menu.Play();
                _scene_game.SceneState = SceneState.GAME_STOPPED;

                ToggleHudVisibility(Visibility.Collapsed);
                GenerateTitleScreenInScene("Game Over");
            }
        }

        #endregion

        #region TitleScreen

        private bool SpawnTitleScreenInScene()
        {
            TitleScreen TitleScreen = null;

            TitleScreen = new(
                animateAction: AnimateTitleScreen,
                recycleAction: (se) => { return true; },
                downScaling: _scene_game.DownScaling,
                playAction: () =>
                {
                    if (_scene_game.SceneState == SceneState.GAME_STOPPED)
                    {
                        if (ScreenExtensions.RequiredDisplayOrientation == ScreenExtensions.GetDisplayOrienation())
                            NewGame(TitleScreen);
                        else
                            ScreenExtensions.SetDisplayOrientation(ScreenExtensions.RequiredDisplayOrientation);
                    }
                    else
                    {
                        if (!_scene_game.IsAnimating)
                        {
                            if (ScreenExtensions.RequiredDisplayOrientation == ScreenExtensions.GetDisplayOrienation())
                                ResumeGame(TitleScreen);
                            else
                                ScreenExtensions.SetDisplayOrientation(ScreenExtensions.RequiredDisplayOrientation);
                        }
                    }

                    return true;
                });

            TitleScreen.SetPosition(
                left: -500,
                top: -500);

            _scene_main_menu.AddToScene(TitleScreen);

            return true;
        }

        private bool GenerateTitleScreenInScene(string title)
        {
            if (_scene_main_menu.Children.OfType<TitleScreen>().FirstOrDefault(x => x.IsAnimating == false) is TitleScreen titleScreen)
            {
                titleScreen.SetTitle(title);
                titleScreen.IsAnimating = true;
                titleScreen.Reposition();

                // Console.WriteLine("Game title generated.");

                return true;
            }

            return false;
        }

        private bool AnimateTitleScreen(Construct se)
        {
            TitleScreen screen1 = se as TitleScreen;
            screen1.Hover();
            return true;
        }

        private void RecycleTitleScreen(TitleScreen TitleScreen)
        {
            TitleScreen.SetPosition(left: -500, top: -500);
            TitleScreen.IsAnimating = false;
        }

        #endregion

        #region InterimScreen

        private bool SpawnInterimScreenInScene()
        {
            InterimScreen interimScreen = null;

            interimScreen = new(
                animateAction: AnimateInterimScreen,
                recycleAction: RecycleInterimScreen,
                downScaling: _scene_game.DownScaling);

            interimScreen.SetPosition(
                left: -500,
                top: -500);

            _scene_game.AddToScene(interimScreen);

            return true;
        }

        private bool GenerateInterimScreenInScene(string title)
        {
            if (_scene_game.Children.OfType<InterimScreen>().FirstOrDefault(x => x.IsAnimating == false) is InterimScreen interimScreen)
            {
                interimScreen.SetTitle(title);
                interimScreen.IsAnimating = true;
                interimScreen.Reposition();
                interimScreen.Reset();

                // Console.WriteLine("Game title generated.");

                return true;
            }

            return false;
        }

        private bool AnimateInterimScreen(Construct interimScreen)
        {
            InterimScreen screen1 = interimScreen as InterimScreen;
            screen1.Hover();
            screen1.DepleteOnScreenDelay();
            return true;
        }

        private bool RecycleInterimScreen(Construct interimScreen)
        {
            if (interimScreen is InterimScreen interimScreen1 && interimScreen1.IsDepleted)
            {
                interimScreen.SetPosition(left: -500, top: -500);
                interimScreen.IsAnimating = false;

                return true;
            }

            return false;
        }

        #endregion

        #region Player

        private bool SpawnPlayerInScene()
        {
            _player = new(
                animateAction: AnimatePlayer,
                recycleAction: (_player) => { return true; },
                downScaling: _scene_game.DownScaling);

            _player.SetPosition(
                  left: -500,
                  top: -500);

            SpawnDropShadowInScene(_player);

            _scene_game.AddToScene(_player);

            return true;
        }

        private bool GeneratePlayerInScene()
        {
            _player.IsAnimating = true;
            _player.Reset();
            _player.Reposition();

            SyncDropShadow(_player);

            SetPlayerHealthBar();

            return true;
        }

        private void SetPlayerHealthBar()
        {
            _player_health_bar.SetMaxiumHealth(_player.Health);
            _player_health_bar.SetValue(_player.Health);

            _player_health_bar.SetIcon(Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.HEALTH_PICKUP).Uri);
            _player_health_bar.SetBarForegroundColor(color: Colors.Purple);
        }

        private bool AnimatePlayer(Construct player)
        {
            _player.Pop();
            _player.Hover();
            _player.DepleteAttackStance();

            var speed = (_scene_game.Speed + player.SpeedOffset) * _scene_game.DownScaling;

            if (_scene_game.SceneState == SceneState.GAME_RUNNING)
            {
                if (_game_controller.IsMoveUp)
                {
                    if (_player.GetBottom() > 0 && _player.GetRight() > 0)
                        _player.MoveUp(speed);
                }
                else if (_game_controller.IsMoveDown)
                {
                    if (_player.GetTop() < _scene_game.Height && _player.GetLeft() < _scene_game.Width)
                        _player.MoveDown(speed);
                }
                else if (_game_controller.IsMoveLeft)
                {
                    if (_player.GetRight() > 0 && _player.GetTop() < _scene_game.Height)
                        _player.MoveLeft(speed);
                }
                else if (_game_controller.IsMoveRight)
                {
                    if (_player.GetLeft() < _scene_game.Width && _player.GetBottom() > 0)
                        _player.MoveRight(speed);
                }
                else
                {
                    if (_player.GetBottom() > 0 && _player.GetRight() > 0
                        && _player.GetTop() < _scene_game.Height && _player.GetLeft() < _scene_game.Width
                        && _player.GetRight() > 0 && _player.GetTop() < _scene_game.Height
                        && _player.GetLeft() < _scene_game.Width && _player.GetBottom() > 0)
                        _player.StopMovement();
                }

                if (_game_controller.IsAttacking)
                {
                    if (_scene_game.Children.OfType<Boss>().Any(x => x.IsAnimating && x.IsAttacking))
                    {
                        if (_powerUp_health_bar.HasHealth)
                            GeneratePlayerBombSeekingInScene();
                        else
                            GeneratePlayerBombInScene();
                    }
                    else
                    {
                        GeneratePlayerBombGroundInScene();
                    }

                    _game_controller.IsAttacking = false;
                }
            }

            return true;
        }

        private void LoosePlayerHealth()
        {
            _player.SetPopping();
            _player.LooseHealth();

            _player_health_bar.SetValue(_player.Health);

            GameOver();
        }

        #endregion

        #region PlayerBomb

        private bool SpawnPlayerBombsInScene()
        {
            for (int i = 0; i < 3; i++)
            {
                PlayerBomb bomb = new(
                    animateAction: AnimatePlayerBomb,
                    recycleAction: RecyclePlayerBomb,
                    downScaling: _scene_game.DownScaling);

                bomb.SetPosition(
                    left: -500,
                    top: -500,
                    z: 7);

                _scene_game.AddToScene(bomb);

                SpawnDropShadowInScene(source: bomb);
            }

            return true;
        }

        private bool GeneratePlayerBombInScene()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is Boss boss &&
                _scene_game.Children.OfType<PlayerBomb>().FirstOrDefault(x => x.IsAnimating == false) is PlayerBomb playerBomb)
            {
                _player.SetAttackStance();

                playerBomb.Reset();
                playerBomb.IsAnimating = true;
                playerBomb.SetPopping();

                playerBomb.Reposition(
                    Player: _player,
                    downScaling: _scene_game.DownScaling);

                SyncDropShadow(playerBomb);

                BossBombSeeking bossBombSeeking = _scene_game.Children.OfType<BossBombSeeking>().FirstOrDefault(x => x.IsAnimating);

                // Console.WriteLine("Player Bomb dropped.");

                #region Target Based Movement

                // player is on the bottom right side of the boss
                if ((_player.GetTop() > boss.GetTop() && _player.GetLeft() > boss.GetLeft()) ||
                    (bossBombSeeking is not null && _player.GetTop() > bossBombSeeking.GetTop() && _player.GetLeft() > bossBombSeeking.GetLeft()))
                {
                    playerBomb.AwaitMoveUp = true;
                    playerBomb.SetRotation(210);
                }
                // player is on the bottom left side of the boss
                else if ((_player.GetTop() > boss.GetTop() && _player.GetLeft() < boss.GetLeft()) ||
                    (bossBombSeeking is not null && _player.GetTop() > bossBombSeeking.GetTop() && _player.GetLeft() < bossBombSeeking.GetLeft()))
                {
                    playerBomb.AwaitMoveRight = true;
                    playerBomb.SetRotation(-33);
                }
                // if player is on the top left side of the boss
                else if ((_player.GetTop() < boss.GetTop() && _player.GetLeft() < boss.GetLeft()) ||
                    (bossBombSeeking is not null && _player.GetTop() < bossBombSeeking.GetTop() && _player.GetLeft() < bossBombSeeking.GetLeft()))
                {
                    playerBomb.AwaitMoveDown = true;
                    playerBomb.SetRotation(123);
                }
                // if player is on the top right side of the boss
                else if ((_player.GetTop() < boss.GetTop() && _player.GetLeft() > boss.GetLeft()) ||
                    (bossBombSeeking is not null && _player.GetTop() < bossBombSeeking.GetTop() && _player.GetLeft() > bossBombSeeking.GetLeft()))
                {
                    playerBomb.AwaitMoveLeft = true;
                    playerBomb.SetRotation(123);
                }
                else
                {
                    playerBomb.AwaitMoveUp = true;
                    playerBomb.SetRotation(123);
                }

                #endregion

                return true;
            }

            return false;
        }

        private bool AnimatePlayerBomb(Construct bomb)
        {
            PlayerBomb playerBomb = bomb as PlayerBomb;

            var speed = (_scene_game.Speed + bomb.SpeedOffset) * _scene_game.DownScaling;

            if (playerBomb.AwaitMoveLeft)
            {
                playerBomb.MoveLeft(speed);
            }
            else if (playerBomb.AwaitMoveRight)
            {
                playerBomb.MoveRight(speed);
            }
            else if (playerBomb.AwaitMoveUp)
            {
                playerBomb.MoveUp(speed);
            }
            else if (playerBomb.AwaitMoveDown)
            {
                playerBomb.MoveDown(speed);
            }

            if (playerBomb.IsBlasting)
            {
                bomb.Expand();
                bomb.Fade(0.02);

                DropShadow dropShadow = _scene_game.Children.OfType<DropShadow>().First(x => x.Id == bomb.Id);
                dropShadow.Opacity = bomb.Opacity;
            }
            else
            {
                bomb.Pop();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    // if player bomb touches boss, boss looses health
                    if (_scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is Boss boss)
                    {
                        if (playerBomb.GetCloseHitBox().IntersectsWith(boss.GetCloseHitBox()))
                        {
                            playerBomb.SetBlast();

                            LooseBossHealth(boss);

                            // Console.WriteLine($"Boss Health: {boss.Health}");
                        }
                    }

                    // if player bomb touches boss's seeking bomb, it blasts
                    if (_scene_game.Children.OfType<BossBombSeeking>().FirstOrDefault(x => x.IsAnimating) is BossBombSeeking bossBombSeeking)
                    {
                        if (playerBomb.GetCloseHitBox().IntersectsWith(bossBombSeeking.GetCloseHitBox()))
                        {
                            playerBomb.SetBlast();
                            bossBombSeeking.SetBlast();
                        }
                    }
                }
            }

            return true;
        }

        private bool RecyclePlayerBomb(Construct bomb)
        {
            var hitbox = bomb.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (bomb.IsFadingComplete ||
                hitbox.Left > _scene_game.Width || hitbox.Top > _scene_game.Height ||
                hitbox.Right < 0 || hitbox.Bottom < 0)
            {
                bomb.IsAnimating = false;

                bomb.SetPosition(
                    left: -500,
                    top: -500);

                return true;
            }

            return false;
        }

        #endregion

        #region PlayerBombGround

        private bool SpawnPlayerBombGroundsInScene()
        {
            for (int i = 0; i < 3; i++)
            {
                PlayerBombGround bomb = new(
                    animateAction: AnimatePlayerBombGround,
                    recycleAction: RecyclePlayerBombGround,
                    downScaling: _scene_game.DownScaling);

                bomb.SetPosition(
                    left: -500,
                    top: -500,
                    z: 7);

                _scene_game.AddToScene(bomb);

                SpawnDropShadowInScene(source: bomb);
            }

            return true;
        }

        private bool GeneratePlayerBombGroundInScene()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<PlayerBombGround>().FirstOrDefault(x => x.IsAnimating == false) is PlayerBombGround bomb)
            {
                _player.SetAttackStance();

                bomb.Reset();
                bomb.IsAnimating = true;
                bomb.IsGravitating = true;
                bomb.SetPopping();

                bomb.SetRotation(_random.Next(-30, 30));

                bomb.Reposition(
                    player: _player,
                    downScaling: _scene_game.DownScaling);

                SyncDropShadow(bomb);

                // Console.WriteLine("Player Ground Bomb dropped.");

                return true;
            }

            return false;
        }

        private bool AnimatePlayerBombGround(Construct bomb)
        {
            PlayerBombGround playerBombGround = bomb as PlayerBombGround;

            DropShadow dropShadow = _scene_game.Children.OfType<DropShadow>().First(x => x.Id == bomb.Id);

            if (playerBombGround.IsBlasting)
            {
                var speed = (_scene_game.Speed + bomb.SpeedOffset);

                bomb.SetLeft(bomb.GetLeft() + speed);
                bomb.SetTop(bomb.GetTop() + speed * bomb.IsometricDisplacement);

                bomb.Expand();
                bomb.Fade(0.02);

                // make the shadow fade with the bomb blast
                dropShadow.Opacity = bomb.Opacity;

                // while in blast check if it intersects with any vehicle, if it does then the vehicle stops honking and slows down
                if (_scene_game.Children.OfType<Vehicle>()
                    .Where(x => x.IsAnimating && x.WillHonk)
                    .FirstOrDefault(x => x.GetCloseHitBox().IntersectsWith(bomb.GetCloseHitBox())) is Vehicle vehicle)
                {
                    vehicle.SetBlast();
                    _game_score_bar.GainScore(5);
                }
            }
            else
            {
                var speed = (_scene_game.Speed + bomb.SpeedOffset);

                bomb.Pop();

                bomb.SetLeft(bomb.GetLeft() + speed);
                bomb.SetTop(bomb.GetTop() + speed);

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    // start blast animation when the bomb touches it's shadow
                    if (dropShadow.GetCloseHitBox().IntersectsWith(bomb.GetCloseHitBox()))
                        playerBombGround.SetBlast();
                }
            }

            return true;
        }

        private bool RecyclePlayerBombGround(Construct bomb)
        {
            if (bomb.IsFadingComplete)
            {
                bomb.IsAnimating = false;
                bomb.IsGravitating = false;

                bomb.SetPosition(
                    left: -500,
                    top: -500);

                return true;
            }

            return false;
        }

        #endregion

        #region PlayerBombSeeking

        private bool SpawnPlayerBombSeekingsInScene()
        {
            for (int i = 0; i < 3; i++)
            {
                PlayerBombSeeking bomb = new(
                    animateAction: AnimatePlayerBombSeeking,
                    recycleAction: RecyclePlayerBombSeeking,
                    downScaling: _scene_game.DownScaling);

                bomb.SetPosition(
                    left: -500,
                    top: -500,
                    z: 7);

                _scene_game.AddToScene(bomb);

                SpawnDropShadowInScene(source: bomb);
            }

            return true;
        }

        private bool GeneratePlayerBombSeekingInScene()
        {
            // generate a seeking bomb if one is not in scene
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is Boss boss &&
                _scene_game.Children.OfType<PlayerBombSeeking>().FirstOrDefault(x => x.IsAnimating == false) is PlayerBombSeeking playerBombSeeking)
            {
                _player.SetAttackStance();

                playerBombSeeking.Reset();
                playerBombSeeking.IsAnimating = true;
                playerBombSeeking.SetPopping();

                playerBombSeeking.Reposition(
                    player: _player,
                    downScaling: _scene_game.DownScaling);

                SyncDropShadow(playerBombSeeking);

                // use up the power up
                if (_powerUp_health_bar.HasHealth)
                    _powerUp_health_bar.SetValue(_powerUp_health_bar.GetValue() - 1);

                // Console.WriteLine("Player Seeking Bomb dropped.");

                return true;
            }

            return false;
        }

        private bool AnimatePlayerBombSeeking(Construct bomb)
        {
            PlayerBombSeeking playerBombSeeking = bomb as PlayerBombSeeking;

            var speed = (_scene_game.Speed + bomb.SpeedOffset) * _scene_game.DownScaling;

            if (playerBombSeeking.IsBlasting)
            {
                MoveConstruct(construct: playerBombSeeking, speed: speed);

                bomb.Expand();
                bomb.Fade(0.02);

                DropShadow dropShadow = _scene_game.Children.OfType<DropShadow>().First(x => x.Id == bomb.Id);
                dropShadow.Opacity = bomb.Opacity;
            }
            else
            {
                bomb.Pop();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    // if there a boss bomb seeking the player then target that first and if they hit both bombs blast

                    if (_scene_game.Children.OfType<BossBombSeeking>().FirstOrDefault(x => x.IsAnimating) is BossBombSeeking bossBombSeeking)
                    {
                        playerBombSeeking.Seek(bossBombSeeking.GetCloseHitBox());

                        if (playerBombSeeking.GetCloseHitBox().IntersectsWith(bossBombSeeking.GetCloseHitBox()))
                        {
                            playerBombSeeking.SetBlast();
                            bossBombSeeking.SetBlast();
                        }
                    }

                    // make the player bomb seek boss

                    if (_scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is Boss boss)
                    {
                        playerBombSeeking.Seek(boss.GetCloseHitBox());

                        if (playerBombSeeking.GetCloseHitBox().IntersectsWith(boss.GetCloseHitBox()))
                        {
                            playerBombSeeking.SetBlast();
                            LooseBossHealth(boss);
                        }
                        else
                        {
                            if (playerBombSeeking.RunOutOfTimeToBlast())
                                playerBombSeeking.SetBlast();
                        }
                    }
                    else
                    {
                        playerBombSeeking.SetBlast();
                    }
                }
            }

            return true;
        }

        private bool RecyclePlayerBombSeeking(Construct bomb)
        {
            var hitbox = bomb.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (bomb.IsFadingComplete || hitbox.Left > _scene_game.Width || hitbox.Right < 0 || hitbox.Top < 0 || hitbox.Bottom > _scene_game.Height)
            {
                bomb.IsAnimating = false;

                bomb.SetPosition(
                    left: -500,
                    top: -500);

                return true;
            }

            return false;
        }

        #endregion

        #region Vehicle

        private bool SpawnVehiclesInScene()
        {
            for (int i = 0; i < 10; i++)
            {
                Vehicle vehicle = new(
                    animateAction: AnimateVehicle,
                    recycleAction: RecycleVehicle,
                    downScaling: _scene_game.DownScaling);

                _scene_game.AddToScene(vehicle);

                vehicle.SetPosition(
                    left: -500,
                    top: -500);
            }

            return true;
        }

        private bool GenerateVehicleInScene()
        {
            if (_scene_game.Children.OfType<Vehicle>().FirstOrDefault(x => x.IsAnimating == false) is Vehicle vehicle)
            {
                vehicle.IsAnimating = true;
                vehicle.Reset();

                // generate top and left corner lane wise vehicles

                var topOrLeft = _random.Next(0, 2);

                var lane = _random.Next(0, 2);

                switch (topOrLeft)
                {
                    case 0:
                        {
                            var xLaneWidth = _scene_game.Width / 4;

                            vehicle.SetPosition(
                                left: lane == 0 ? 0 : xLaneWidth - vehicle.Width * _scene_game.DownScaling,
                                top: vehicle.Height * -1);
                        }
                        break;
                    case 1:
                        {
                            var yLaneWidth = (_scene_game.Height / 2) / 2;

                            vehicle.SetPosition(
                                left: vehicle.Width * -1,
                                top: lane == 0 ? 0 : yLaneWidth * _scene_game.DownScaling);
                        }
                        break;
                    default:
                        break;
                }

                vehicle.SetZ(3);

                // Console.WriteLine("Vehicle generated.");
                return true;
            }


            return false;
        }

        private bool AnimateVehicle(Construct vehicle)
        {
            var speed = (_scene_game.Speed + vehicle.SpeedOffset);

            MoveConstruct(construct: vehicle, speed: speed);

            // TODO: fix hitbox for safe distance between vehicles

            var hitHox = vehicle.GetHorizontalHitBox();

            // prevent overlapping

            if (_scene_game.Children.OfType<Vehicle>().FirstOrDefault(x => x.IsAnimating && x.GetHorizontalHitBox().IntersectsWith(hitHox)) is Construct collidingVehicle)
            {
                if (collidingVehicle.SpeedOffset < vehicle.SpeedOffset)
                {
                    collidingVehicle.SpeedOffset = vehicle.SpeedOffset;
                }
                else if (collidingVehicle.SpeedOffset > vehicle.SpeedOffset)
                {
                    vehicle.SpeedOffset = collidingVehicle.SpeedOffset;
                }
            }

            Vehicle vehicle1 = vehicle as Vehicle;

            vehicle.Pop();

            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                vehicle1.Honk())
            {
                GenerateHonkInScene(vehicle1);
            }

            return true;
        }

        private bool RecycleVehicle(Construct vehicle)
        {
            var hitBox = vehicle.GetHitBox();

            if (hitBox.Top > _scene_game.Height || hitBox.Left > _scene_game.Width)
            {
                vehicle.SetPosition(
                    left: -500,
                    top: -500);

                vehicle.IsAnimating = false;
            }

            return true;
        }

        #endregion

        #region RoadSlab

        //private bool SpawnRoadSlabsInScene()
        //{
        //    for (int i = 0; i < 8; i++)
        //    {
        //        RoadSlab roadSlab = new(
        //            animateAction: AnimateRoadSlab,
        //            recycleAction: RecycleRoadSlab,
        //            downScaling: _scene_game.DownScaling);

        //        roadSlab.SetPosition(
        //            left: -1500,
        //            top: -1500);

        //        _scene_game.AddToScene(roadSlab);
        //    }

        //    return true;
        //}

        //private bool GenerateRoadSlabInSceneTop()
        //{
        //    if (_scene_game.Children.OfType<RoadSlab>().FirstOrDefault(x => x.IsAnimating == false) is RoadSlab roadSlab)
        //    {
        //        roadSlab.IsAnimating = true;

        //        roadSlab.SetPosition(
        //            left: (_scene_game.Width / 2 - roadSlab.Width / 1.5) * _scene_game.DownScaling,
        //            top: (0 - roadSlab.Width) * _scene_game.DownScaling,
        //            z: 0);

        //        // Console.WriteLine("RoadSlab Mark generated.");

        //        return true;
        //    }

        //    return false;
        //}

        //private bool GenerateRoadSlabInSceneBottom()
        //{
        //    if (_scene_game.Children.OfType<RoadSlab>().FirstOrDefault(x => x.IsAnimating == false) is RoadSlab roadSlab)
        //    {
        //        roadSlab.IsAnimating = true;

        //        roadSlab.SetPosition(
        //            left: (-1 * roadSlab.Width) * _scene_game.DownScaling,
        //            top: (_scene_game.Height / 2.5) * _scene_game.DownScaling,
        //            z: 0);

        //        // Console.WriteLine("RoadSlab Mark generated.");

        //        return true;
        //    }

        //    return false;
        //}

        //private bool AnimateRoadSlab(Construct roadSlab)
        //{
        //    var speed = (_scene_game.Speed + roadSlab.SpeedOffset);
        //    MoveConstruct(construct: roadSlab, speed: speed);
        //    return true;
        //}

        //private bool RecycleRoadSlab(Construct roadSlab)
        //{
        //    var hitBox = roadSlab.GetHitBox();

        //    if (hitBox.Top > _scene_game.Height || hitBox.Left > _scene_game.Width)
        //    {
        //        roadSlab.SetPosition(
        //            left: -1500,
        //            top: -1500);

        //        roadSlab.IsAnimating = false;
        //    }

        //    return true;
        //}

        #endregion

        #region RoadMark

        private bool SpawnRoadMarksInScene()
        {
            for (int i = 0; i < 20; i++)
            {
                RoadMark roadMark = new(
                    animateAction: AnimateRoadMark,
                    recycleAction: RecycleRoadMark,
                    downScaling: _scene_game.DownScaling);

                roadMark.SetPosition(
                    left: -500,
                    top: -500);

                _scene_game.AddToScene(roadMark);
            }

            return true;
        }

        private bool GenerateRoadMarkInScene()
        {
            if (_scene_game.Children.OfType<RoadMark>().FirstOrDefault(x => x.IsAnimating == false) is RoadMark roadMark)
            {
                roadMark.IsAnimating = true;

                roadMark.SetPosition(
                    left: 0,
                    top: 0,
                    z: 1);

                // Console.WriteLine("Road Mark generated.");

                return true;
            }

            return false;
        }

        private bool AnimateRoadMark(Construct roadMark)
        {
            var speed = (_scene_game.Speed + roadMark.SpeedOffset);
            MoveConstruct(construct: roadMark, speed: speed);
            return true;
        }

        private bool RecycleRoadMark(Construct roadMark)
        {
            var hitBox = roadMark.GetHitBox();

            if (hitBox.Top > _scene_game.Height || hitBox.Left > _scene_game.Width)
            {
                roadMark.SetPosition(
                    left: -500,
                    top: -500);

                roadMark.IsAnimating = false;
            }

            return true;
        }

        #endregion

        #region Tree

        private bool SpawnTreesInScene()
        {
            for (int i = 0; i < 10; i++)
            {
                Tree tree = new(
                    animateAction: AnimateTree,
                    recycleAction: RecycleTree,
                    downScaling: _scene_game.DownScaling);

                tree.SetPosition(
                    left: -500,
                    top: -500,
                    z: 2);

                _scene_game.AddToScene(tree);

                SpawnDropShadowInScene(source: tree);
            }

            return true;
        }

        private bool GenerateTreeInSceneTop()
        {
            if (_scene_game.Children.OfType<Tree>().FirstOrDefault(x => x.IsAnimating == false) is Tree tree)
            {
                tree.IsAnimating = true;

                tree.SetPosition(
                    left: (_scene_game.Width / 2 - tree.Width) * _scene_game.DownScaling,
                    top: (0 - tree.Width) * _scene_game.DownScaling,
                    z: 2);

                SyncDropShadow(tree);

                // Console.WriteLine("Tree generated.");

                return true;
            }

            return false;
        }

        private bool GenerateTreeInSceneBottom()
        {
            if (_scene_game.Children.OfType<Tree>().FirstOrDefault(x => x.IsAnimating == false) is Tree tree)
            {
                tree.IsAnimating = true;

                tree.SetPosition(
                    left: (-1 * tree.Width) * _scene_game.DownScaling,
                    top: (_scene_game.Height / 2.5) * _scene_game.DownScaling,
                    z: 4);

                SyncDropShadow(tree);

                // Console.WriteLine("Tree generated.");

                return true;
            }

            return false;
        }

        private bool AnimateTree(Construct tree)
        {
            var speed = (_scene_game.Speed + tree.SpeedOffset);
            MoveConstruct(construct: tree, speed: speed);
            return true;
        }

        private bool RecycleTree(Construct tree)
        {
            var hitBox = tree.GetHitBox();

            if (hitBox.Top > _scene_game.Height || hitBox.Left > _scene_game.Width)
            {
                tree.SetPosition(
                    left: -500,
                    top: -500);

                tree.IsAnimating = false;
            }

            return true;
        }

        #endregion

        #region HealthPickup

        private bool SpawnHealthPickupsInScene()
        {
            for (int i = 0; i < 3; i++)
            {
                HealthPickup HealthPickup = new(
                    animateAction: AnimateHealthPickup,
                    recycleAction: RecycleHealthPickup,
                    downScaling: _scene_game.DownScaling);

                HealthPickup.SetPosition(
                    left: -500,
                    top: -500,
                    z: 6);

                _scene_game.AddToScene(HealthPickup);
            }

            return true;
        }

        private bool GenerateHealthPickupsInScene()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                HealthPickup.ShouldGenerate(_player.Health) &&
                _scene_game.Children.OfType<HealthPickup>().FirstOrDefault(x => x.IsAnimating == false) is HealthPickup healthPickup)
            {
                healthPickup.IsAnimating = true;
                healthPickup.Reset();

                var topOrLeft = _random.Next(0, 2);

                var lane = _random.Next(0, 2);

                switch (topOrLeft)
                {
                    case 0:
                        {
                            var xLaneWidth = _scene_game.Width / 4;
                            healthPickup.SetPosition(
                                left: _random.Next(0, Convert.ToInt32(xLaneWidth - healthPickup.Width)) * _scene_game.DownScaling,
                                top: healthPickup.Height * -1);
                        }
                        break;
                    case 1:
                        {
                            var yLaneWidth = (_scene_game.Height / 2) / 2;
                            healthPickup.SetPosition(
                                left: healthPickup.Width * -1,
                                top: _random.Next(0, Convert.ToInt32(yLaneWidth)) * _scene_game.DownScaling);
                        }
                        break;
                    default:
                        break;
                }

                SyncDropShadow(healthPickup);

                return true;
            }

            return false;
        }

        private bool AnimateHealthPickup(Construct healthPickup)
        {
            var speed = _scene_game.Speed + healthPickup.SpeedOffset;

            HealthPickup healthPickup1 = healthPickup as HealthPickup;

            if (healthPickup1.IsPickedUp)
            {
                healthPickup1.Shrink();
            }
            else
            {
                MoveConstruct(construct: healthPickup, speed: speed);

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    var hitbox = healthPickup.GetCloseHitBox();

                    if (_player.GetCloseHitBox().IntersectsWith(hitbox))
                    {
                        _player.GainHealth();
                        _player_health_bar.SetValue(_player.Health);
                        healthPickup1.IsPickedUp = true;
                    }
                }
            }

            return true;
        }

        private bool RecycleHealthPickup(Construct healthPickup)
        {
            var hitBox = healthPickup.GetHitBox();

            if (hitBox.Top > _scene_game.Height || hitBox.Left > _scene_game.Width || healthPickup.IsShrinkingComplete)
            {
                healthPickup.SetPosition(
                    left: -500,
                    top: -500);

                healthPickup.IsAnimating = false;
            }

            return true;
        }

        #endregion

        #region PowerUpPickup

        private bool SpawnPowerUpPickupsInScene()
        {
            for (int i = 0; i < 3; i++)
            {
                PowerUpPickup powerUpPickup = new(
                    animateAction: AnimatePowerUpPickup,
                    recycleAction: RecyclePowerUpPickup,
                    downScaling: _scene_game.DownScaling);

                powerUpPickup.SetPosition(
                    left: -500,
                    top: -500,
                    z: 6);

                _scene_game.AddToScene(powerUpPickup);
            }

            return true;
        }

        private bool GeneratePowerUpPickupsInScene()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is Boss boss &&
                _scene_game.Children.OfType<PowerUpPickup>().FirstOrDefault(x => x.IsAnimating == false) is PowerUpPickup powerUpPickup)
            {
                powerUpPickup.IsAnimating = true;
                powerUpPickup.Reset();

                var topOrLeft = _random.Next(0, 2);

                var lane = _random.Next(0, 2);

                switch (topOrLeft)
                {
                    case 0:
                        {
                            var xLaneWidth = _scene_game.Width / 4;
                            powerUpPickup.SetPosition(
                                left: _random.Next(0, Convert.ToInt32(xLaneWidth - powerUpPickup.Width)) * _scene_game.DownScaling,
                                top: powerUpPickup.Height * -1);
                        }
                        break;
                    case 1:
                        {
                            var yLaneWidth = (_scene_game.Height / 2) / 2;
                            powerUpPickup.SetPosition(
                                left: powerUpPickup.Width * -1,
                                top: _random.Next(0, Convert.ToInt32(yLaneWidth)) * _scene_game.DownScaling);
                        }
                        break;
                    default:
                        break;
                }

                SyncDropShadow(powerUpPickup);

                return true;
            }

            return false;
        }

        private bool AnimatePowerUpPickup(Construct powerUpPickup)
        {
            var speed = _scene_game.Speed + powerUpPickup.SpeedOffset;

            PowerUpPickup powerUpPickup1 = powerUpPickup as PowerUpPickup;

            if (powerUpPickup1.IsPickedUp)
            {
                powerUpPickup1.Shrink();
            }
            else
            {
                MoveConstruct(construct: powerUpPickup, speed: speed);

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    var hitbox = powerUpPickup.GetCloseHitBox();

                    // if player picks up seeking bomb pickup
                    if (_player.GetCloseHitBox().IntersectsWith(hitbox))
                    {
                        powerUpPickup1.IsPickedUp = true;

                        // allow using a burst of 3 seeking bombs 3 times
                        _powerUp_health_bar.SetMaxiumHealth(9);
                        _powerUp_health_bar.SetValue(9);

                        _powerUp_health_bar.SetIcon(Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP).Uri);
                        _powerUp_health_bar.SetBarForegroundColor(color: Colors.Green);
                    }
                }
            }

            return true;
        }

        private bool RecyclePowerUpPickup(Construct powerUpPickup)
        {
            var hitBox = powerUpPickup.GetHitBox();

            if (hitBox.Top > _scene_game.Height || hitBox.Left > _scene_game.Width || powerUpPickup.IsShrinkingComplete)
            {
                powerUpPickup.SetPosition(
                    left: -500,
                    top: -500);

                powerUpPickup.IsAnimating = false;
            }

            return true;
        }

        #endregion

        #region Honk

        private bool SpawnHonksInScene()
        {
            for (int i = 0; i < 10; i++)
            {
                Honk honk = new(
                    animateAction: AnimateHonk,
                    recycleAction: RecycleHonk,
                    downScaling: _scene_game.DownScaling);

                honk.SetPosition(
                    left: -500,
                    top: -500);

                _scene_game.AddToScene(honk);
            }

            return true;
        }

        private bool GenerateHonkInScene(Vehicle vehicle)
        {
            // if there are no bosses in the scene the vehicles will honk
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<Honk>().FirstOrDefault(x => x.IsAnimating == false) is Honk honk &&
                !_scene_game.Children.OfType<Boss>().Any(x => x.IsAnimating && x.IsAttacking))
            {
                honk.IsAnimating = true;
                honk.SetPopping();

                honk.Reset();

                var hitBox = vehicle.GetCloseHitBox();

                honk.SetPosition(
                    left: hitBox.Left - vehicle.Width / 2,
                    top: hitBox.Top - (25 * _scene_game.DownScaling),
                    z: 5);

                honk.SetRotation(_random.Next(-30, 30));
                honk.SetZ(vehicle.GetZ() + 1);

                return true;
            }

            return false;
        }

        private bool AnimateHonk(Construct honk)
        {
            honk.Pop();
            var speed = (_scene_game.Speed + honk.SpeedOffset);
            MoveConstruct(construct: honk, speed: speed);
            return true;
        }

        private bool RecycleHonk(Construct honk)
        {
            Honk honk1 = honk as Honk;

            honk1.Fade();

            if (honk1.IsFadingComplete)
            {
                honk.IsAnimating = false;

                honk.SetPosition(
                    left: -500,
                    top: -500);
            }
            //_scene.DisposeFromScene(honk);

            return true;
        }

        #endregion

        #region Cloud

        private bool SpawnCloudsInScene()
        {
            for (int i = 0; i < 10; i++)
            {
                Cloud cloud = new(
                    animateAction: AnimateCloud,
                    recycleAction: RecycleCloud,
                    downScaling: _scene_game.DownScaling);

                cloud.SetPosition(
                    left: -500,
                    top: -500,
                    z: 9);

                _scene_game.AddToScene(cloud);

                //SpawnDropShadowInScene(source: cloud);
            }

            return true;
        }

        private bool GenerateCloudInScene()
        {
            if (_scene_game.Children.OfType<Cloud>().FirstOrDefault(x => x.IsAnimating == false) is Cloud cloud)
            {
                cloud.IsAnimating = true;
                cloud.Reset();

                var topOrLeft = _random.Next(0, 2);

                var lane = _random.Next(0, 2);

                switch (topOrLeft)
                {
                    case 0:
                        {
                            var xLaneWidth = _scene_game.Width / 4;
                            cloud.SetPosition(
                                left: _random.Next(0, Convert.ToInt32(xLaneWidth - cloud.Width)) * _scene_game.DownScaling,
                                top: cloud.Height * -1);
                        }
                        break;
                    case 1:
                        {
                            var yLaneWidth = (_scene_game.Height / 2) / 2;
                            cloud.SetPosition(
                                left: cloud.Width * -1,
                                top: _random.Next(0, Convert.ToInt32(yLaneWidth)) * _scene_game.DownScaling);
                        }
                        break;
                    default:
                        break;
                }

                //SyncDropShadow(cloud);

                return true;
            }

            return false;
        }

        private bool AnimateCloud(Construct cloud)
        {
            var speed = (_scene_game.Speed + cloud.SpeedOffset);
            MoveConstruct(construct: cloud, speed: speed);
            return true;
        }

        private bool RecycleCloud(Construct cloud)
        {
            var hitBox = cloud.GetHitBox();

            if (hitBox.Top > _scene_game.Height || hitBox.Left > _scene_game.Width)
            {
                cloud.SetPosition(
                    left: -500,
                    top: -500);

                cloud.IsAnimating = false;
            }

            return true;
        }

        #endregion

        #region Boss

        private bool SpawnBossesInScene()
        {
            for (int i = 0; i < 3; i++)
            {
                Boss boss = new(
                    animateAction: AnimateBoss,
                    recycleAction: RecycleBoss,
                    downScaling: _scene_game.DownScaling);

                boss.SetPosition(
                    left: -500,
                    top: -500,
                    z: 8);

                _scene_game.AddToScene(boss);

                SpawnDropShadowInScene(source: boss);
            }

            return true;
        }

        private bool GenerateBossInScene()
        {
            //TODO: _scoreBar.IsBossPointScore() &&

            // if scene doesn't contain a boss then pick a random boss and add to scene

            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _game_score_bar.IsBossPointScore() &&
                !_scene_game.Children.OfType<Boss>().Any(x => x.IsAnimating) &&
                _scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating == false) is Boss boss)
            {
                boss.IsAnimating = true;
                boss.Reset();
                boss.SetPosition(
                    left: 0,
                    top: boss.Height * -1);

                SyncDropShadow(boss);

                // set boss health
                boss.Health = _game_score_bar.GetBossPointScoreDifference() * 1.5;

                _boss_health_bar.SetMaxiumHealth(boss.Health);
                _boss_health_bar.SetValue(boss.Health);
                _boss_health_bar.SetIcon(boss.GetContentUri());
                _boss_health_bar.SetBarForegroundColor(color: Colors.Crimson);

                GenerateInterimScreenInScene("Beware of Boss");

                _scene_game.ActivateSlowMotion();

                return true;
            }

            return false;
        }

        private bool AnimateBoss(Construct boss)
        {
            Boss boss1 = boss as Boss;

            if (boss1.IsDead)
            {
                boss.Shrink();

                // Console.WriteLine($"Boss ScaleX: {boss.GetScaleX()} ScaleY: {boss.GetScaleY()}");
            }
            else
            {
                boss1.Hover();

                boss.Pop();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    var speed = (_scene_game.Speed + boss.SpeedOffset) * _scene_game.DownScaling;

                    // bring boss to a suitable distance from player and then start attacking

                    #region [PATTERN CHANGING] Unpredictable Movement

                    if (boss1.IsAttacking)
                    {
                        boss1.Move(
                            speed: speed,
                            sceneWidth: _scene_game.Width,
                            sceneHeight: _scene_game.Height,
                            playerPoint: _player.GetCloseHitBox());
                    }
                    else
                    {
                        MoveConstruct(construct: boss, speed: speed);

                        if (boss.GetLeft() > (_scene_game.Width / 3) * 1.5)
                        {
                            boss1.IsAttacking = true;
                        }
                    }

                    #endregion

                    #region [SEEKING] Player Seeking Movement

                    //if (boss1.IsAttacking)
                    //{
                    //    boss1.SeekPlayer(_player.GetCloseHitBox());
                    //}
                    //else
                    //{
                    //    if (boss.GetLeft() > (_scene.Width / 3) * 1.5)
                    //    {
                    //        boss1.IsAttacking = true;
                    //    }
                    //}

                    #endregion

                    #region [L SSHAPED] Back and Forth Movement

                    //if (boss.GetLeft() > (_scene.Width / 3) * 1.5)
                    //{
                    //    if (boss1.IsAttacking &&
                    //        !boss1.AwaitMoveLeft && !boss1.AwaitMoveRight &&
                    //        !boss1.AwaitMoveUp && !boss1.AwaitMoveDown)
                    //    {
                    //        boss1.AwaitMoveLeft = true;
                    //    }
                    //    else
                    //    {
                    //        boss1.IsAttacking = true;
                    //    }
                    //}

                    //if (boss1.IsAttacking)
                    //{
                    //    if (boss1.AwaitMoveLeft)
                    //    {
                    //        boss1.MoveLeft(speed);

                    //        if (boss.GetLeft() < 0 || boss.GetBottom() > _scene.Height)
                    //        {
                    //            boss1.AwaitMoveLeft = false;
                    //            boss1.AwaitMoveRight = true;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if (boss1.AwaitMoveRight)
                    //        {
                    //            boss1.MoveRight(speed);

                    //            if (boss.GetTop() < 0)
                    //            {
                    //                boss1.AwaitMoveRight = false;
                    //                boss1.AwaitMoveDown = true;
                    //            }
                    //        }
                    //        else
                    //        {
                    //            if (boss1.AwaitMoveDown)
                    //            {
                    //                boss1.MoveDown(speed);

                    //                if (boss1.GetRight() > _scene.Width || boss1.GetBottom() > _scene.Height)
                    //                {
                    //                    boss1.AwaitMoveUp = true;
                    //                    boss1.AwaitMoveDown = false;
                    //                }
                    //            }
                    //            else
                    //            {
                    //                if (boss1.AwaitMoveUp)
                    //                {
                    //                    boss1.MoveUp(speed);

                    //                    if (boss1.GetTop() < 0 || boss1.GetLeft() < 0)
                    //                    {
                    //                        boss1.AwaitMoveUp = false;
                    //                        boss1.AwaitMoveLeft = true;
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                    #endregion

                    #region [SQUARE] Rounding Movement

                    //if (boss.GetLeft() > (_scene.Width / 3) * 1.5)
                    //{
                    //    if (boss1.IsAttacking &&
                    //        !boss1.AwaitMoveLeft && !boss1.AwaitMoveRight &&
                    //        !boss1.AwaitMoveUp && !boss1.AwaitMoveDown)
                    //    {
                    //        boss1.AwaitMoveRight = true;
                    //    }
                    //    else
                    //    {
                    //        boss1.IsAttacking = true;
                    //    }
                    //}

                    //if (boss1.IsAttacking)
                    //{
                    //    if (boss1.AwaitMoveRight)
                    //    {
                    //        boss1.MoveRight(speed);

                    //        if (boss.GetTop() < 0)
                    //        {
                    //            boss1.AwaitMoveRight = false;
                    //            boss1.AwaitMoveDown = true;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if (boss1.AwaitMoveDown)
                    //        {
                    //            boss1.MoveDown(speed);

                    //            if (boss1.GetRight() > _scene.Width || boss1.GetBottom() > _scene.Height)
                    //            {
                    //                boss1.AwaitMoveDown = false;
                    //                boss1.AwaitMoveLeft = true;
                    //            }
                    //        }
                    //        else
                    //        {
                    //            if (boss1.AwaitMoveLeft)
                    //            {
                    //                boss1.MoveLeft(speed);

                    //                if (boss.GetLeft() < 0 || boss.GetBottom() > _scene.Height)
                    //                {
                    //                    boss1.AwaitMoveLeft = false;
                    //                    boss1.AwaitMoveUp = true;
                    //                }
                    //            }
                    //            else
                    //            {
                    //                if (boss1.AwaitMoveUp)
                    //                {
                    //                    boss1.MoveUp(speed);

                    //                    if (boss1.GetTop() < 0 || boss1.GetLeft() < 0)
                    //                    {
                    //                        boss1.AwaitMoveUp = false;
                    //                        boss1.AwaitMoveRight = true;
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                    #endregion 
                }
            }

            return true;
        }

        private bool RecycleBoss(Construct boss)
        {
            if (boss.IsShrinkingComplete)
            {
                boss.SetPosition(
                    left: -500,
                    top: -500);

                boss.IsAnimating = false;

                _game_score_bar.GainScore(5);
            }

            return true;
        }

        private void LooseBossHealth(Boss boss)
        {
            boss.SetPopping();
            boss.LooseHealth();

            _boss_health_bar.SetValue(boss.Health);

            if (boss.IsDead && boss.IsAttacking)
            {
                GenerateInterimScreenInScene("Boss Busted");
                boss.IsAttacking = false;
                _scene_game.ActivateSlowMotion();
            }
        }

        #endregion

        #region BossBomb

        private bool SpawnBossBombsInScene()
        {
            for (int i = 0; i < 5; i++)
            {
                BossBomb bomb = new(
                    animateAction: AnimateBossBomb,
                    recycleAction: RecycleBossBomb,
                    downScaling: _scene_game.DownScaling);

                bomb.SetPosition(
                    left: -500,
                    top: -500,
                    z: 7);

                _scene_game.AddToScene(bomb);

                SpawnDropShadowInScene(source: bomb);
            }

            return true;
        }

        private bool GenerateBossBombInScene()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is Boss boss &&
                _scene_game.Children.OfType<BossBomb>().FirstOrDefault(x => x.IsAnimating == false) is BossBomb bossBomb)
            {
                bossBomb.Reset();
                bossBomb.IsAnimating = true;
                bossBomb.SetPopping();

                bossBomb.Reposition(
                    boss: boss,
                    downScaling: _scene_game.DownScaling);

                SyncDropShadow(bossBomb);

                #region [OBSOLETE] Back & Forth Movement

                //if (boss.AwaitMoveLeft || boss.AwaitMoveRight)
                //{
                //    // player is on the right side of the boss
                //    if (_player.GetLeft() > boss.GetRight())
                //    {
                //        bossBomb.AwaitMoveDown = true;
                //        bossBomb.SetRotation(33);
                //    }
                //    else
                //    {
                //        bossBomb.AwaitMoveUp = true;
                //        bossBomb.SetRotation(125);
                //    }
                //}
                //else if (boss.AwaitMoveUp || boss.AwaitMoveDown)
                //{
                //    // player is above the boss
                //    if (_player.GetBottom() < boss.GetTop())
                //    {
                //        bossBomb.AwaitMoveRight = true;
                //        bossBomb.SetRotation(-33);
                //    }
                //    else
                //    {
                //        bossBomb.AwaitMoveLeft = true;
                //        bossBomb.SetRotation(125);
                //    }
                //} 

                #endregion

                #region Target Based Movement

                // player is on the bottom right side of the boss
                if (_player.GetTop() > boss.GetTop() && _player.GetLeft() > boss.GetLeft())
                {
                    bossBomb.AwaitMoveDown = true;
                    bossBomb.SetRotation(33);
                }
                // player is on the bottom left side of the boss
                else if (_player.GetTop() > boss.GetTop() && _player.GetLeft() < boss.GetLeft())
                {
                    bossBomb.AwaitMoveLeft = true;
                    bossBomb.SetRotation(123);
                }
                // if player is on the top left side of the boss
                else if (_player.GetTop() < boss.GetTop() && _player.GetLeft() < boss.GetLeft())
                {
                    bossBomb.AwaitMoveUp = true;
                    bossBomb.SetRotation(123);
                }
                // if player is on the top right side of the boss
                else if (_player.GetTop() < boss.GetTop() && _player.GetLeft() > boss.GetLeft())
                {
                    bossBomb.AwaitMoveRight = true;
                    bossBomb.SetRotation(-33);
                }
                else
                {
                    bossBomb.AwaitMoveDown = true;
                    bossBomb.SetRotation(33);
                }

                #endregion

                // Console.WriteLine("Boss Bomb dropped.");

                return true;
            }

            return false;
        }

        private bool AnimateBossBomb(Construct bomb)
        {
            BossBomb bossBomb = bomb as BossBomb;

            var speed = (_scene_game.Speed + bomb.SpeedOffset) * _scene_game.DownScaling;

            if (bossBomb.AwaitMoveLeft)
            {
                bossBomb.MoveLeft(speed);
            }
            else if (bossBomb.AwaitMoveRight)
            {
                bossBomb.MoveRight(speed);
            }
            else if (bossBomb.AwaitMoveUp)
            {
                bossBomb.MoveUp(speed);
            }
            else if (bossBomb.AwaitMoveDown)
            {
                bossBomb.MoveDown(speed);
            }

            if (bossBomb.IsBlasting)
            {
                bomb.Expand();
                bomb.Fade(0.02);

                DropShadow dropShadow = _scene_game.Children.OfType<DropShadow>().First(x => x.Id == bomb.Id);
                dropShadow.Opacity = bomb.Opacity;
            }
            else
            {
                bomb.Pop();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (bossBomb.GetCloseHitBox().IntersectsWith(_player.GetCloseHitBox()))
                    {
                        bossBomb.SetBlast();

                        LoosePlayerHealth();
                    }
                }
            }

            return true;
        }

        private bool RecycleBossBomb(Construct bomb)
        {
            var hitbox = bomb.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (bomb.IsFadingComplete || hitbox.Left > _scene_game.Width || hitbox.Right < 0 || hitbox.Top < 0 || hitbox.Bottom > _scene_game.Height)
            {
                bomb.IsAnimating = false;

                bomb.SetPosition(
                    left: -500,
                    top: -500);

                return true;
            }

            return false;
        }

        #endregion

        #region BossBombSeeking

        private bool SpawnBossBombSeekingsInScene()
        {
            for (int i = 0; i < 2; i++)
            {
                BossBombSeeking bomb = new(
                    animateAction: AnimateBossBombSeeking,
                    recycleAction: RecycleBossBombSeeking,
                    downScaling: _scene_game.DownScaling);

                bomb.SetPosition(
                    left: -500,
                    top: -500,
                    z: 7);

                _scene_game.AddToScene(bomb);

                SpawnDropShadowInScene(source: bomb);
            }

            return true;
        }

        private bool GenerateBossBombSeekingInScene()
        {
            // generate a seeking bomb if one is not in scene
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is Boss boss &&
                !_scene_game.Children.OfType<BossBombSeeking>().Any(x => x.IsAnimating) &&
                _scene_game.Children.OfType<BossBombSeeking>().FirstOrDefault(x => x.IsAnimating == false) is BossBombSeeking bossBombSeeking)
            {
                bossBombSeeking.Reset();
                bossBombSeeking.IsAnimating = true;
                bossBombSeeking.SetPopping();

                bossBombSeeking.Reposition(
                    boss: boss,
                    downScaling: _scene_game.DownScaling);

                SyncDropShadow(bossBombSeeking);

                // Console.WriteLine("Boss Seeking Bomb dropped.");

                return true;
            }

            return false;
        }

        private bool AnimateBossBombSeeking(Construct bomb)
        {
            BossBombSeeking bossBombSeeking = bomb as BossBombSeeking;

            var speed = (_scene_game.Speed + bomb.SpeedOffset) * _scene_game.DownScaling;

            if (bossBombSeeking.IsBlasting)
            {
                MoveConstruct(construct: bossBombSeeking, speed: speed);

                bomb.Expand();
                bomb.Fade(0.02);

                DropShadow dropShadow = _scene_game.Children.OfType<DropShadow>().First(x => x.Id == bomb.Id);
                dropShadow.Opacity = bomb.Opacity;
            }
            else
            {
                bomb.Pop();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (_scene_game.Children.OfType<Boss>().Any(x => x.IsAnimating && x.IsAttacking))
                    {
                        bossBombSeeking.Seek(_player.GetCloseHitBox());

                        if (bossBombSeeking.GetCloseHitBox().IntersectsWith(_player.GetCloseHitBox()))
                        {
                            bossBombSeeking.SetBlast();

                            LoosePlayerHealth();
                        }
                        else
                        {
                            if (bossBombSeeking.RunOutOfTimeToBlast())
                                bossBombSeeking.SetBlast();
                        }
                    }
                    else
                    {
                        bossBombSeeking.SetBlast();
                    }
                }
            }

            return true;
        }

        private bool RecycleBossBombSeeking(Construct bomb)
        {
            var hitbox = bomb.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (bomb.IsFadingComplete || hitbox.Left > _scene_game.Width || hitbox.Right < 0 || hitbox.Top < 0 || hitbox.Bottom > _scene_game.Height)
            {
                bomb.IsAnimating = false;

                bomb.SetPosition(
                    left: -500,
                    top: -500);

                return true;
            }

            return false;
        }

        #endregion

        #region DropShadow

        private bool SpawnDropShadowInScene(Construct source)
        {
            DropShadow dropShadow = new(
                animateAction: AnimateDropShadow,
                recycleAction: RecycleDropShadow,
                downScaling: _scene_game.DownScaling);

            _scene_game.AddToScene(dropShadow);

            dropShadow.SetParent(construct: source);
            dropShadow.Move();
            dropShadow.SetZ(source.GetZ() - 1);

            return true;
        }

        private bool AnimateDropShadow(Construct construct)
        {
            DropShadow dropShadow = construct as DropShadow;
            dropShadow.SyncWidth();
            dropShadow.Move();

            return true;
        }

        private bool RecycleDropShadow(Construct dropShadow)
        {
            DropShadow dropShadow1 = dropShadow as DropShadow;

            if (!dropShadow1.ParentConstruct.IsAnimating)
            {
                dropShadow.IsAnimating = false;

                dropShadow.SetPosition(
                    left: -500,
                    top: -500);

                return true;
            }

            return false;
        }

        private void SyncDropShadow(Construct source)
        {
            if (_scene_game.Children.OfType<DropShadow>().FirstOrDefault(x => x.Id == source.Id) is DropShadow dropShadow)
            {
                dropShadow.Opacity = 1;
                dropShadow.ParentConstructSpeed = _scene_game.Speed + source.SpeedOffset;
                dropShadow.IsAnimating = true;

                dropShadow.SetZ(source.GetZ() - 2);

                dropShadow.Reset();
            }
        }

        #endregion

        #region Construct

        private void MoveConstruct(Construct construct, double speed)
        {
            speed *= _scene_game.DownScaling;

            construct.SetLeft(construct.GetLeft() + speed);
            construct.SetTop(construct.GetTop() + speed * construct.IsometricDisplacement);
        }

        #endregion

        #region Controller

        private void SetController()
        {
            _game_controller.SetScene(_scene_game);
            _game_controller.RequiresScreenOrientationChange += Controller_RequiresScreenOrientationChange;
            _game_controller.PauseButton.Click += PauseButton_Click;
        }

        private void ToggleHudVisibility(Visibility visibility)
        {
            _game_controller.Visibility = visibility;
            _game_score_bar.Visibility = visibility;
            _health_bars.Visibility = visibility;
        }

        #endregion

        #region Scene

        private void SetScene()
        {
            _scene_game.Clear();
            _scene_main_menu.Clear();

            _powerUp_health_bar.Reset();
            _boss_health_bar.Reset();
            _game_score_bar.Reset();

            //// first add road slabs
            //_scene_game.AddToScene(new Generator(
            //    generationDelay: 70,
            //    generationAction: GenerateRoadSlabInSceneTop,
            //    startUpAction: SpawnRoadSlabsInScene));

            //// first add road slabs
            //_scene_game.AddToScene(new Generator(
            //    generationDelay: 70,
            //    generationAction: GenerateRoadSlabInSceneBottom,
            //    startUpAction: SpawnRoadSlabsInScene));

            // then add road marks
            _scene_game.AddToScene(new Generator(
                generationDelay: 30,
                generationAction: GenerateRoadMarkInScene,
                startUpAction: SpawnRoadMarksInScene));

            // then add the top trees
            _scene_game.AddToScene(new Generator(
                generationDelay: 30,
                generationAction: GenerateTreeInSceneTop,
                startUpAction: SpawnTreesInScene));

            // then add the vehicles which will appear forward in z wrt the top trees
            _scene_game.AddToScene(new Generator(
                generationDelay: 80,
                generationAction: GenerateVehicleInScene,
                startUpAction: SpawnVehiclesInScene));

            // then add the bottom trees which will appear forward in z wrt to the vehicles
            _scene_game.AddToScene(new Generator(
                generationDelay: 30,
                generationAction: GenerateTreeInSceneBottom,
                startUpAction: SpawnTreesInScene));

            // add the honks which will appear forward in z wrt to everything on the road
            _scene_game.AddToScene(new Generator(
                generationDelay: 0,
                generationAction: () => { return true; },
                startUpAction: SpawnHonksInScene));

            // add the player in scene which will appear forward in z wrt to all else
            _scene_game.AddToScene(new Generator(
                generationDelay: 0,
                generationAction: () => { return true; },
                startUpAction: SpawnPlayerInScene));

            _scene_game.AddToScene(new Generator(
                generationDelay: 0,
                generationAction: () => { return true; },
                startUpAction: SpawnPlayerBombsInScene));

            _scene_game.AddToScene(new Generator(
                generationDelay: 0,
                generationAction: () => { return true; },
                startUpAction: SpawnPlayerBombGroundsInScene));

            // add the clouds which are abve the player z
            _scene_game.AddToScene(new Generator(
                generationDelay: 100,
                generationAction: GenerateCloudInScene,
                startUpAction: SpawnCloudsInScene));

            _scene_game.AddToScene(new Generator(
                generationDelay: 100,
                generationAction: GenerateBossInScene,
                startUpAction: SpawnBossesInScene));

            _scene_game.AddToScene(new Generator(
                generationDelay: 50,
                generationAction: GenerateBossBombInScene,
                startUpAction: SpawnBossBombsInScene,
                randomizeGenerationDelay: true));

            _scene_game.AddToScene(new Generator(
                generationDelay: 200,
                generationAction: GenerateBossBombSeekingInScene,
                startUpAction: SpawnBossBombSeekingsInScene,
                randomizeGenerationDelay: true));

            _scene_game.AddToScene(new Generator(
                generationDelay: 0,
                generationAction: () => { return true; },
                startUpAction: SpawnPlayerBombSeekingsInScene));

            _scene_game.AddToScene(new Generator(
                generationDelay: 250,
                generationAction: GenerateHealthPickupsInScene,
                startUpAction: SpawnHealthPickupsInScene));

            _scene_game.AddToScene(new Generator(
                generationDelay: 600,
                generationAction: GeneratePowerUpPickupsInScene,
                startUpAction: SpawnPowerUpPickupsInScene,
                randomizeGenerationDelay: true));

            _scene_main_menu.AddToScene(new Generator(
               generationDelay: 0,
               generationAction: () => { return true; },
               startUpAction: SpawnTitleScreenInScene));

            _scene_game.AddToScene(new Generator(
              generationDelay: 0,
              generationAction: () => { return true; },
              startUpAction: SpawnInterimScreenInScene));

            _scene_game.Speed = 5;
            _scene_game.Play();

            _scene_main_menu.Speed = 5;
            _scene_main_menu.Play();
        }

        #endregion

        #endregion

        #region Events

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _scene_game.Width = 1920;
            _scene_game.Height = 1080;

            _scene_main_menu.Width = 1920;
            _scene_main_menu.Height = 1080;

            SetController();
            SetScene();
            GenerateTitleScreenInScene("Honk Trooper");

            SizeChanged += MainPage_SizeChanged;

            ScreenExtensions.DisplayInformation.OrientationChanged += DisplayInformation_OrientationChanged;
            ScreenExtensions.RequiredDisplayOrientation = DisplayOrientations.Landscape;

            // set display orientation to required orientation
            if (ScreenExtensions.GetDisplayOrienation() != ScreenExtensions.RequiredDisplayOrientation)
                ScreenExtensions.SetDisplayOrientation(ScreenExtensions.RequiredDisplayOrientation);

            ScreenExtensions.EnterFullScreen(true);
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= MainPage_SizeChanged;
            ScreenExtensions.DisplayInformation.OrientationChanged -= DisplayInformation_OrientationChanged;
            _game_controller.RequiresScreenOrientationChange -= Controller_RequiresScreenOrientationChange;
            _game_controller.PauseButton.Click -= PauseButton_Click;
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            var _windowWidth = args.NewSize.Width;
            var _windowHeight = args.NewSize.Height;

            _scene_game.Width = _windowWidth;
            _scene_game.Height = _windowHeight;

            _scene_main_menu.Width = _windowWidth;
            _scene_main_menu.Height = _windowHeight;

            _game_controller.Width = _windowWidth;
            _game_controller.Height = _windowHeight;

            if (_scene_game.SceneState == SceneState.GAME_RUNNING)
            {
                _player.Reposition();
                SyncDropShadow(_player);
            }

            if (_scene_main_menu.Children.OfType<TitleScreen>().FirstOrDefault(x => x.IsAnimating) is TitleScreen GamePlay)
                GamePlay.Reposition();
        }

        private void Controller_RequiresScreenOrientationChange(object sender, DisplayOrientations e)
        {
            // Console.WriteLine($"Required Orientation {e}");
        }

        private void DisplayInformation_OrientationChanged(DisplayInformation sender, object args)
        {
            if (_scene_game.IsAnimating)
                _scene_game.Pause();

            // Console.WriteLine($"{sender.CurrentOrientation}");
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            PauseGame();
        }

        #endregion
    }
}
