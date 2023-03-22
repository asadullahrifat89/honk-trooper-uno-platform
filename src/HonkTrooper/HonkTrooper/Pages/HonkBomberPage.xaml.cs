using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Display;

namespace HonkTrooper
{
    public sealed partial class HonkBomberPage : Page
    {
        #region Fields

        private PlayerBalloon _player;
        private readonly Random _random;

        private readonly Scene _scene_game;
        private readonly Scene _scene_main_menu;
        private readonly Controller _game_controller;

        private readonly HealthBar _player_health_bar;
        private readonly HealthBar _boss_health_bar;
        private readonly HealthBar _powerUp_health_bar;

        private readonly ScoreBar _game_score_bar;
        private readonly StackPanel _health_bars;

        private readonly Threashold _boss_threashold;
        private readonly Threashold _enemy_threashold;

        //TODO: set defaults _boss_threashold_limit = 50
        private readonly double _boss_threashold_limit = 50; // first boss will appear
        private readonly double _boss_threashold_limit_increase = 15;

        //TODO: set defaults _enemy_threashold_limit = 80
        private readonly double _enemy_threashold_limit = 80; // after first enemies will appear
        private readonly double _enemy_threashold_limit_increase = 10;

        private double _enemy_kill_count;
        private readonly double _enemy_kill_count_limit = 20;

        private bool _enemy_fleet_appeared;

        private AudioStub _audio_stub;

        private int _selected_player_template;


        #endregion

        #region Ctor

        public HonkBomberPage()
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

            _boss_threashold = new Threashold(_boss_threashold_limit);
            _enemy_threashold = new Threashold(_enemy_threashold_limit);

            ToggleHudVisibility(Visibility.Collapsed);

            _random = new Random();

            _audio_stub = new AudioStub(
                (SoundType.GAME_BACKGROUND_MUSIC, 0.5, true),
                (SoundType.BOSS_BACKGROUND_MUSIC, 0.5, true),
                (SoundType.AMBIENCE, 0.6, true),
                (SoundType.GAME_START, 1, false),
                (SoundType.GAME_PAUSE, 1, false),
                (SoundType.GAME_OVER, 1, false),
                (SoundType.ENEMY_ENTRY, 1, false));

            _scene_main_menu.SetRenderTransformOrigin(0.5);

            Loaded += HonkBomberPage_Loaded;
            Unloaded += HonkBomberPage_Unloaded;
        }

        #endregion

        #region Methods

        #region Game

        private bool PauseGame()
        {
            _audio_stub.Play(SoundType.GAME_PAUSE);

            _audio_stub.Pause(SoundType.AMBIENCE);

            if (BossExists())
            {
                //_audio_stub.Pause(SoundType.BOSS_BACKGROUND_MUSIC);
            }
            else
            {
                _audio_stub.Pause(SoundType.GAME_BACKGROUND_MUSIC);
            }

            ToggleHudVisibility(Visibility.Collapsed);

            _scene_game.Pause();
            _scene_main_menu.Play();

            _game_controller.DeactivateGyrometerReading();
            _game_controller.SetDefaultThumbstickPosition();

            GenerateTitleScreen("Game Paused");

            return true;
        }

        private void ResumeGame()
        {
            _audio_stub.Resume(SoundType.AMBIENCE);

            if (BossExists())
            {
                //_audio_stub.Resume(SoundType.BOSS_BACKGROUND_MUSIC);
            }
            else
            {
                _audio_stub.Resume(SoundType.GAME_BACKGROUND_MUSIC);
            }

            ToggleHudVisibility(Visibility.Visible);

            _scene_game.Play();
            _scene_main_menu.Pause();

            _game_controller.ActivateGyrometerReading();
            _game_controller.FocusAttackButton();
        }

        private void NewGame()
        {
            LoggerExtensions.Log("New Game Started.");

            _audio_stub.Play(SoundType.AMBIENCE, SoundType.GAME_BACKGROUND_MUSIC);

            _game_controller.Reset();

            _powerUp_health_bar.Reset();
            _boss_health_bar.Reset();
            _game_score_bar.Reset();

            _boss_threashold.Reset(_boss_threashold_limit);
            _enemy_threashold.Reset(_enemy_threashold_limit);
            _enemy_kill_count = 0;
            _enemy_fleet_appeared = false;

            GeneratePlayerBalloon();
            RepositionLogicalConstructs();

            _scene_game.SceneState = SceneState.GAME_RUNNING;

            if (!_scene_game.IsAnimating)
                _scene_game.Play();

            _scene_main_menu.Pause();

            ToggleHudVisibility(Visibility.Visible);

            _game_controller.FocusAttackButton();

            _game_controller.SetDefaultThumbstickPosition();
            _game_controller.ActivateGyrometerReading();
        }

        private void GameOver()
        {
            // if player is dead game keeps playing in the background but scene state goes to game over
            if (_player.IsDead)
            {
                _audio_stub.Stop(SoundType.AMBIENCE, SoundType.GAME_BACKGROUND_MUSIC/*, SoundType.BOSS_BACKGROUND_MUSIC*/);

                if (_scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating) is Boss boss)
                {
                    boss.SetWinStance();
                    boss.StopSoundLoop();
                }

                _audio_stub.Play(SoundType.GAME_OVER);

                _scene_main_menu.Play();
                _scene_game.SceneState = SceneState.GAME_STOPPED;

                ToggleHudVisibility(Visibility.Collapsed);
                GenerateTitleScreen("Game Over");

                _game_controller.DeactivateGyrometerReading();
            }
        }

        private void RepositionLogicalConstructs()
        {
            foreach (var construct in _scene_game.Children.OfType<Construct>()
                .Where(x => x.ConstructType is
                ConstructType.VEHICLE_LARGE or
                ConstructType.VEHICLE_SMALL or
                ConstructType.HONK or
                ConstructType.PLAYER_ROCKET or
                ConstructType.PLAYER_ROCKET_SEEKING or
                ConstructType.PLAYER_FIRE_CRACKER or
                ConstructType.BOSS_ROCKET or
                ConstructType.BOSS_ROCKET_SEEKING or
                ConstructType.ENEMY or
                ConstructType.ENEMY_ROCKET or
                ConstructType.POWERUP_PICKUP or
                ConstructType.HEALTH_PICKUP or
                ConstructType.BOSS))
            {
                construct.IsAnimating = false;

                construct.SetPosition(
                     left: -3000,
                     top: -3000);

                if (construct is Boss boss1)
                {
                    boss1.IsAttacking = false;
                    boss1.Health = 0;
                }
            }
        }

        private void RepositionHoveringTitleScreens()
        {
            foreach (var screen in _scene_main_menu.Children.OfType<HoveringTitleScreen>().Where(x => x.IsAnimating))
            {
                screen.Reposition();
            }
        }

        #endregion

        #region DisplayOrientationChangeScreen

        private bool SpawnDisplayOrientationChangeScreen()
        {
            DisplayOrientationChangeScreen displayOrientationChangeScreen = null;

            displayOrientationChangeScreen = new(
                animateAction: AnimateDisplayOrientationChangeScreen,
                recycleAction: (se) => { return true; });

            displayOrientationChangeScreen.SetPosition(
                left: -3000,
                top: -3000);

            _scene_main_menu.AddToScene(displayOrientationChangeScreen);

            return true;
        }

        private bool GenerateDisplayOrientationChangeScreen()
        {
            if (_scene_main_menu.Children.OfType<DisplayOrientationChangeScreen>().FirstOrDefault(x => x.IsAnimating == false) is DisplayOrientationChangeScreen displayOrientationChangeScreen)
            {
                displayOrientationChangeScreen.IsAnimating = true;
                displayOrientationChangeScreen.Reposition();

                LoggerExtensions.Log("Screen Orientation Change Promt Generated.");

                return true;
            }

            return false;
        }

        private bool AnimateDisplayOrientationChangeScreen(Construct displayOrientationChangeScreen)
        {
            DisplayOrientationChangeScreen screen1 = displayOrientationChangeScreen as DisplayOrientationChangeScreen;
            screen1.Hover();
            return true;
        }

        private void RecycleDisplayOrientationChangeScreen(DisplayOrientationChangeScreen displayOrientationChangeScreen)
        {
            displayOrientationChangeScreen.IsAnimating = false;
            displayOrientationChangeScreen.SetPosition(left: -3000, top: -3000);

            LoggerExtensions.Log("Screen Orientation Change Promt Recyled.");
        }

        #endregion

        #region TitleScreen

        private bool SpawnTitleScreen()
        {
            TitleScreen titleScreen = null;

            titleScreen = new(
                animateAction: AnimateTitleScreen,
                recycleAction: (se) => { return true; },
                playAction: () =>
                {
                    if (_scene_game.SceneState == SceneState.GAME_STOPPED)
                    {
                        if (ScreenExtensions.RequiredDisplayOrientation == ScreenExtensions.GetDisplayOrienation())
                        {
                            RecycleTitleScreen(titleScreen);
                            GeneratePlayerSelectionScreen();
                            ScreenExtensions.EnterFullScreen(true);
                        }
                        else
                        {
                            ScreenExtensions.SetDisplayOrientation(ScreenExtensions.RequiredDisplayOrientation);
                        }
                    }
                    else
                    {
                        if (!_scene_game.IsAnimating)
                        {
                            if (ScreenExtensions.RequiredDisplayOrientation == ScreenExtensions.GetDisplayOrienation())
                            {
                                ResumeGame();
                                RecycleTitleScreen(titleScreen);
                            }
                            else
                            {
                                ScreenExtensions.SetDisplayOrientation(ScreenExtensions.RequiredDisplayOrientation);
                            }
                        }
                    }

                    return true;
                });

            titleScreen.SetPosition(
                left: -3000,
                top: -3000);

            _scene_main_menu.AddToScene(titleScreen);

            return true;
        }

        private bool GenerateTitleScreen(string title)
        {
            if (_scene_main_menu.Children.OfType<TitleScreen>().FirstOrDefault(x => x.IsAnimating == false) is TitleScreen titleScreen)
            {
                titleScreen.SetTitle(title);
                titleScreen.IsAnimating = true;
                titleScreen.Reposition();

                if (_player is not null)
                    titleScreen.SetContent(_player.GetContentUri());

                return true;
            }

            return false;
        }

        private bool AnimateTitleScreen(Construct titleScreen)
        {
            TitleScreen screen1 = titleScreen as TitleScreen;
            screen1.Hover();
            return true;
        }

        private void RecycleTitleScreen(TitleScreen titleScreen)
        {
            titleScreen.IsAnimating = false;
            titleScreen.SetPosition(left: -3000, top: -3000);
        }

        #endregion

        #region PlayerSelectionScreen

        private bool SpawnPlayerSelectionScreen()
        {
            PlayerSelectionScreen playerSelectionScreen = null;

            playerSelectionScreen = new(
                animateAction: AnimatePlayerSelectionScreen,
                recycleAction: (se) => { return true; },
                playAction: (int playerTemplate) =>
                {
                    _selected_player_template = playerTemplate;

                    if (_scene_game.SceneState == SceneState.GAME_STOPPED)
                    {
                        RecyclePlayerSelectionScreen(playerSelectionScreen);
                        NewGame();
                    }

                    return true;
                },
                backAction: () =>
                {
                    RecyclePlayerSelectionScreen(playerSelectionScreen);
                    GenerateTitleScreen("Honk Trooper");
                    return true;
                });

            playerSelectionScreen.SetPosition(
                left: -3000,
                top: -3000);

            _scene_main_menu.AddToScene(playerSelectionScreen);

            return true;
        }

        private bool GeneratePlayerSelectionScreen()
        {
            if (_scene_main_menu.Children.OfType<PlayerSelectionScreen>().FirstOrDefault(x => x.IsAnimating == false) is PlayerSelectionScreen playerSelectionScreen)
            {
                playerSelectionScreen.IsAnimating = true;
                playerSelectionScreen.Reposition();

                LoggerExtensions.Log("Player Selection Screen Generated.");

                return true;
            }

            return false;
        }

        private bool AnimatePlayerSelectionScreen(Construct playerSelectionScreen)
        {
            PlayerSelectionScreen screen1 = playerSelectionScreen as PlayerSelectionScreen;
            screen1.Hover();
            return true;
        }

        private void RecyclePlayerSelectionScreen(PlayerSelectionScreen playerSelectionScreen)
        {
            playerSelectionScreen.IsAnimating = false;
            playerSelectionScreen.SetPosition(left: -3000, top: -3000);

            LoggerExtensions.Log("Player Selection Screen Recyled.");
        }

        #endregion

        #region InterimScreen

        private bool SpawnInterimScreen()
        {
            InterimScreen interimScreen = null;

            interimScreen = new(
                animateAction: AnimateInterimScreen,
                recycleAction: RecycleInterimScreen);

            interimScreen.SetPosition(
                left: -3000,
                top: -3000);

            _scene_game.AddToScene(interimScreen);

            return true;
        }

        private bool GenerateInterimScreen(string title)
        {
            if (_scene_game.Children.OfType<InterimScreen>().FirstOrDefault(x => x.IsAnimating == false) is InterimScreen interimScreen)
            {
                interimScreen.IsAnimating = true;
                interimScreen.SetTitle(title);
                interimScreen.Reposition();
                interimScreen.Reset();

                LoggerExtensions.Log("Game title generated.");

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
                interimScreen.IsAnimating = false;
                interimScreen.SetPosition(left: -3000, top: -3000);

                return true;
            }

            return false;
        }

        #endregion

        #region PlayerBalloon

        private bool SpawnPlayerBalloon()
        {
            var playerTemplate = _random.Next(1, 3);
            LoggerExtensions.Log($"Player Template: {playerTemplate}");

            _player = new(
                animateAction: AnimatePlayerBalloon,
                recycleAction: (_player) => { return true; });

            _player.SetPosition(
                  left: -3000,
                  top: -3000);

            SpawnDropShadow(_player);

            _scene_game.AddToScene(_player);

            return true;
        }

        private bool GeneratePlayerBalloon()
        {
            _player.IsAnimating = true;
            _player.Reset();
            _player.Reposition();
            _player.SetPlayerTemplate(_selected_player_template);

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

        private bool AnimatePlayerBalloon(Construct player)
        {
            _player.Pop();
            _player.Hover();
            _player.DepleteAttackStance();
            _player.DepleteWinStance();
            _player.DepleteHitStance();

            var speed = (_scene_game.Speed + player.SpeedOffset);

            var halfHeight = _player.Height / 2;
            var halfWidth = _player.Width / 2;

            if (_scene_game.SceneState == SceneState.GAME_RUNNING)
            {
                ProcessPlayerBalloonMovement(speed, halfHeight, halfWidth);
                ProcessPlayerAttack();
            }

            return true;
        }

        private void ProcessPlayerAttack()
        {
            if (_game_controller.IsAttacking)
            {
                if (EnemyExists() || BossExists())
                {
                    if (_powerUp_health_bar.HasHealth && (PowerUpType)_powerUp_health_bar.Tag == PowerUpType.SEEKING_BALLS)
                        GeneratePlayerRocketSeeking();
                    else
                        GeneratePlayerRocket();
                }
                else
                {
                    GeneratePlayerFireCracker();
                }

                _game_controller.IsAttacking = false;
            }
        }

        private void ProcessPlayerBalloonMovement(double speed, double halfHeight, double halfWidth)
        {
            if (_game_controller.IsMoveUp && _game_controller.IsMoveLeft)
            {
                if (_player.GetTop() + halfHeight > 0 && _player.GetLeft() + halfWidth > 0)
                    _player.MoveUpLeft(speed);
            }
            else if (_game_controller.IsMoveUp && _game_controller.IsMoveRight)
            {
                if (_player.GetRight() - halfWidth < _scene_game.Width && _player.GetTop() + halfHeight > 0)
                    _player.MoveUpRight(speed);
            }
            else if (_game_controller.IsMoveUp)
            {
                if (_player.GetTop() + halfHeight > 0)
                    _player.MoveUp(speed);
            }
            else if (_game_controller.IsMoveDown && _game_controller.IsMoveRight)
            {
                if (_player.GetBottom() - halfHeight < _scene_game.Height && _player.GetRight() - halfWidth < _scene_game.Width)
                    _player.MoveDownRight(speed);
            }
            else if (_game_controller.IsMoveDown && _game_controller.IsMoveLeft)
            {
                if (_player.GetLeft() + halfWidth > 0 && _player.GetBottom() - halfHeight < _scene_game.Height)
                    _player.MoveDownLeft(speed);
            }
            else if (_game_controller.IsMoveDown)
            {
                if (_player.GetBottom() - halfHeight < _scene_game.Height)
                    _player.MoveDown(speed);
            }
            else if (_game_controller.IsMoveRight)
            {
                if (_player.GetRight() - halfWidth < _scene_game.Width)
                    _player.MoveRight(speed);
            }
            else if (_game_controller.IsMoveLeft)
            {
                if (_player.GetLeft() + halfWidth > 0)
                    _player.MoveLeft(speed);
            }
            else
            {
                // if player is already out of bounds then prevent stop movement animation

                if (_player.GetBottom() > 0 && _player.GetRight() > 0 &&
                    _player.GetTop() < _scene_game.Height && _player.GetLeft() < _scene_game.Width &&
                    _player.GetRight() > 0 && _player.GetTop() < _scene_game.Height &&
                    _player.GetLeft() < _scene_game.Width && _player.GetBottom() > 0)
                {
                    _player.StopMovement();
                }
            }
        }

        private void LoosePlayerHealth()
        {
            _player.SetPopping();

            if (_powerUp_health_bar.HasHealth && (PowerUpType)_powerUp_health_bar.Tag == PowerUpType.FORCE_SHIELD)
            {
                DepletePowerUp();
            }
            else
            {
                _player.LooseHealth();
                _player.SetHitStance();

                _player_health_bar.SetValue(_player.Health);

                if (_scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is Boss boss)
                    boss.SetWinStance();

                GameOver();
            }
        }

        #endregion

        #region Vehicle

        private bool SpawnVehicles()
        {
            for (int i = 0; i < 10; i++)
            {
                Vehicle vehicle = new(
                    animateAction: AnimateVehicle,
                    recycleAction: RecycleVehicle);

                _scene_game.AddToScene(vehicle);

                vehicle.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        private bool GenerateVehicle()
        {
            if (!BossExists() && _scene_game.Children.OfType<Vehicle>().FirstOrDefault(x => x.IsAnimating == false) is Vehicle vehicle)
            {
                vehicle.IsAnimating = true;
                vehicle.Reset();

                var topOrLeft = _random.Next(2); // generate top and left corner lane wise vehicles
                var lane = _random.Next(2); // generate number of lanes based of screen height

                switch (topOrLeft)
                {
                    case 0:
                        {
                            var xLaneWidth = _scene_game.Width / 4;

                            vehicle.SetPosition(
                                left: lane == 0 ? 0 : (xLaneWidth - vehicle.Width / 2),
                                top: vehicle.Height * -1);
                        }
                        break;
                    case 1:
                        {
                            var yLaneHeight = _scene_game.Height / 6;

                            vehicle.SetPosition(
                                left: vehicle.Width * -1,
                                top: lane == 0 ? 0 : (yLaneHeight));
                        }
                        break;
                    default:
                        break;
                }

                vehicle.SetZ(3);

                LoggerExtensions.Log("Vehicle generated.");
                return true;
            }


            return false;
        }

        private bool AnimateVehicle(Construct vehicle)
        {
            Vehicle vehicle1 = vehicle as Vehicle;

            vehicle.Pop();

            var speed = (_scene_game.Speed + vehicle.SpeedOffset);

            MoveConstructBottomRight(construct: vehicle, speed: speed);

            if (_scene_game.SceneState == SceneState.GAME_RUNNING)
            {
                if (vehicle1.Honk())
                    GenerateVehicleHonk(vehicle1);
            }

            PreventVehicleOverlapping(vehicle);

            return true;
        }

        private bool RecycleVehicle(Construct vehicle)
        {
            var hitBox = vehicle.GetHitBox();

            if (hitBox.Top > _scene_game.Height || hitBox.Left > _scene_game.Width)
            {
                vehicle.IsAnimating = false;

                vehicle.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        private void PreventVehicleOverlapping(Construct vehicle)
        {
            var vehicle_distantHitBox = vehicle.GetDistantHitBox();

            // fix vehicle z order
            //if (_scene_game.Children.OfType<Vehicle>().FirstOrDefault(x => x.GetDistantHitBox() is Rect x_DistantHitBox &&
            //    x_DistantHitBox.IntersectsWith(vehicle_distantHitBox) &&
            //    vehicle_distantHitBox.Bottom > x_DistantHitBox.Bottom &&
            //    vehicle.GetZ() <= x.GetZ()) is Vehicle belowVehicle)
            //{
            //    vehicle.SetZ(belowVehicle.GetZ() + 1);
            //}

            //if (_scene_game.Children.OfType<Vehicle>().FirstOrDefault(x => x.GetDistantHitBox() is Rect x_DistantHitBox &&
            //    x_DistantHitBox.IntersectsWith(vehicle_distantHitBox) &&
            //    vehicle_distantHitBox.Bottom < x_DistantHitBox.Bottom &&
            //    vehicle.GetZ() >= x.GetZ()) is Vehicle overVehicle)
            //{
            //    vehicle.SetZ(overVehicle.GetZ() - 1);
            //}

            if (_scene_game.Children.OfType<Vehicle>().FirstOrDefault(x => x.IsAnimating && x.GetHorizontalHitBox().IntersectsWith(vehicle.GetHorizontalHitBox())) is Construct collidingVehicle)
            {
                var hitBox = vehicle.GetHitBox();

                if (vehicle.SpeedOffset == collidingVehicle.SpeedOffset)
                {
                    if (vehicle.SpeedOffset > -2)
                        vehicle.SpeedOffset--;
                }
                else
                {
                    if (vehicle.SpeedOffset > collidingVehicle.SpeedOffset) // vehicle is faster
                    {
                        vehicle.SpeedOffset = collidingVehicle.SpeedOffset;
                    }
                    else if (collidingVehicle.SpeedOffset > vehicle.SpeedOffset) // colliding vehicle is faster
                    {
                        collidingVehicle.SpeedOffset = vehicle.SpeedOffset;
                    }
                }
            }
        }

        #endregion        

        #region RoadMark

        private bool SpawnRoadMarks()
        {
            for (int i = 0; i < 20; i++)
            {
                RoadMark roadMark = new(
                    animateAction: AnimateRoadMark,
                    recycleAction: RecycleRoadMark);

                roadMark.SetPosition(
                    left: -3000,
                    top: -3000);

                _scene_game.AddToScene(roadMark);
            }

            return true;
        }

        private bool GenerateRoadMark()
        {
            if (_scene_game.Children.OfType<RoadMark>().FirstOrDefault(x => x.IsAnimating == false) is RoadMark roadMark)
            {
                roadMark.IsAnimating = true;

                roadMark.SetPosition(
                  left: roadMark.Width * -1,
                  top: roadMark.Height / 3 * -1,
                  z: 1);

                LoggerExtensions.Log("Road Mark generated.");

                return true;
            }

            return false;
        }

        private bool AnimateRoadMark(Construct roadMark)
        {
            var speed = (_scene_game.Speed + roadMark.SpeedOffset);
            MoveConstructBottomRight(construct: roadMark, speed: speed);
            return true;
        }

        private bool RecycleRoadMark(Construct roadMark)
        {
            var hitBox = roadMark.GetHitBox();

            if (hitBox.Top > _scene_game.Height || hitBox.Left > _scene_game.Width)
            {
                roadMark.IsAnimating = false;

                roadMark.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        #endregion

        #region RoadSideStripe

        private bool SpawnRoadSideStripes()
        {
            for (int i = 0; i < 10; i++)
            {
                RoadSideStripe roadSideStripe = new(
                    animateAction: AnimateRoadSideStripe,
                    recycleAction: RecycleRoadSideStripe);

                roadSideStripe.SetPosition(
                    left: -3000,
                    top: -3000);

                _scene_game.AddToScene(roadSideStripe);
            }

            return true;
        }

        private bool GenerateRoadSideStripeTop()
        {
            if (_scene_game.Children.OfType<RoadSideStripe>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideStripe roadSideStripe)
            {
                roadSideStripe.IsAnimating = true;

                roadSideStripe.SetPosition(
                    left: _scene_game.Width / 4.7,
                    top: roadSideStripe.Height * -1.1,
                    z: 0);

                return true;
            }

            return false;
        }

        private bool GenerateRoadSideStripeBottom()
        {
            if (_scene_game.Children.OfType<RoadSideStripe>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideStripe roadSideStripe)
            {
                roadSideStripe.IsAnimating = true;

                roadSideStripe.SetPosition(
                    left: -1 * roadSideStripe.Height,
                    top: _scene_game.Height / 6.7,
                    z: 0);

                return true;
            }

            return false;
        }

        private bool AnimateRoadSideStripe(Construct roadSideStripe)
        {
            var speed = (_scene_game.Speed + roadSideStripe.SpeedOffset);
            MoveConstructBottomRight(construct: roadSideStripe, speed: speed);
            return true;
        }

        private bool RecycleRoadSideStripe(Construct roadSideStripe)
        {
            var hitBox = roadSideStripe.GetHitBox();

            if (hitBox.Top - roadSideStripe.Height > _scene_game.Height || hitBox.Left - roadSideStripe.Height > _scene_game.Width)
            {
                roadSideStripe.IsAnimating = false;

                roadSideStripe.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        #endregion

        #region RoadSidePatch

        private bool SpawnRoadSidePatchs()
        {
            for (int i = 0; i < 5; i++)
            {
                RoadSidePatch RoadSidePatch = new(
                animateAction: AnimateRoadSidePatch,
                recycleAction: RecycleRoadSidePatch);

                RoadSidePatch.SetPosition(
                    left: -3000,
                    top: -3000);

                _scene_game.AddToScene(RoadSidePatch);
            }

            return true;
        }

        private bool GenerateRoadSidePatchBottom()
        {
            if (_scene_game.Children.OfType<RoadSidePatch>().FirstOrDefault(x => x.IsAnimating == false) is RoadSidePatch roadSidePatch)
            {
                roadSidePatch.IsAnimating = true;

                roadSidePatch.SetPosition(
                    left: (roadSidePatch.Height * -1),
                    top: (_scene_game.Height / 3.1 + roadSidePatch.Height / 2),
                    z: 0);

                return true;
            }

            return false;
        }

        private bool GenerateRoadSidePatchTop()
        {
            if (_scene_game.Children.OfType<RoadSidePatch>().FirstOrDefault(x => x.IsAnimating == false) is RoadSidePatch roadSidePatch)
            {
                roadSidePatch.IsAnimating = true;

                roadSidePatch.SetPosition(
                    left: (_scene_game.Width / 2 - roadSidePatch.Width),
                    top: roadSidePatch.Height * -1,
                    z: 2);

                return true;
            }

            return false;
        }

        private bool AnimateRoadSidePatch(Construct roadSidePatch)
        {
            var speed = (_scene_game.Speed + roadSidePatch.SpeedOffset);
            MoveConstructBottomRight(construct: roadSidePatch, speed: speed);
            return true;
        }

        private bool RecycleRoadSidePatch(Construct roadSidePatch)
        {
            var hitBox = roadSidePatch.GetHitBox();

            if (hitBox.Top > _scene_game.Height || hitBox.Left - roadSidePatch.Width > _scene_game.Width)
            {
                roadSidePatch.IsAnimating = false;

                roadSidePatch.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        #endregion

        #region Tree

        private bool SpawnTrees()
        {
            for (int i = 0; i < 10; i++)
            {
                Tree tree = new(
                    animateAction: AnimateTree,
                    recycleAction: RecycleTree);

                tree.SetPosition(
                    left: -3000,
                    top: -3000);

                _scene_game.AddToScene(tree);

                SpawnDropShadow(source: tree);
            }

            return true;
        }

        private bool GenerateTreeTop()
        {
            if (_scene_game.Children.OfType<Tree>().FirstOrDefault(x => x.IsAnimating == false) is Tree tree)
            {
                tree.IsAnimating = true;

                tree.SetPosition(
                  left: (_scene_game.Width / 2 - tree.Width),
                  top: (tree.Height * 1.1) * -1,
                  z: 2);

                SyncDropShadow(tree);

                LoggerExtensions.Log("Tree generated.");

                return true;
            }

            return false;
        }

        private bool GenerateTreeBottom()
        {
            if (_scene_game.Children.OfType<Tree>().FirstOrDefault(x => x.IsAnimating == false) is Tree tree)
            {
                tree.IsAnimating = true;

                tree.SetPosition(
                  left: (-1 * tree.Width),
                  top: (_scene_game.Height / 3),
                  z: 4);

                SyncDropShadow(tree);

                LoggerExtensions.Log("Tree generated.");

                return true;
            }

            return false;
        }

        private bool AnimateTree(Construct tree)
        {
            var speed = (_scene_game.Speed + tree.SpeedOffset);
            MoveConstructBottomRight(construct: tree, speed: speed);
            return true;
        }

        private bool RecycleTree(Construct tree)
        {
            var hitBox = tree.GetHitBox();

            if (hitBox.Top > _scene_game.Height || hitBox.Left > _scene_game.Width)
            {
                tree.IsAnimating = false;

                tree.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        #endregion

        #region Hedge

        private bool SpawnHedges()
        {
            for (int i = 0; i < 15; i++)
            {
                Hedge hedge = new(
                    animateAction: AnimateHedge,
                    recycleAction: RecycleHedge);

                hedge.SetPosition(
                    left: -3000,
                    top: -3000);

                _scene_game.AddToScene(hedge);

                SpawnDropShadow(source: hedge);
            }

            return true;
        }

        private bool GenerateHedgeTop()
        {
            if (_scene_game.Children.OfType<Hedge>().FirstOrDefault(x => x.IsAnimating == false) is Hedge hedge)
            {
                hedge.IsAnimating = true;

                hedge.SetPosition(
                  left: (_scene_game.Width / 2 - hedge.Width * 2.3),
                  top: hedge.Height * -1.1,
                  z: 2);

                LoggerExtensions.Log("Hedge generated.");

                return true;
            }

            return false;
        }

        private bool GenerateHedgeBottom()
        {
            if (_scene_game.Children.OfType<Hedge>().FirstOrDefault(x => x.IsAnimating == false) is Hedge hedge)
            {
                hedge.IsAnimating = true;

                hedge.SetPosition(
                  left: -1 * hedge.Width,
                  top: (_scene_game.Height / 3 + hedge.Height / 3),
                  z: 3);

                LoggerExtensions.Log("Hedge generated.");

                return true;
            }

            return false;
        }

        private bool AnimateHedge(Construct hedge)
        {
            var speed = (_scene_game.Speed + hedge.SpeedOffset);
            MoveConstructBottomRight(construct: hedge, speed: speed);
            return true;
        }

        private bool RecycleHedge(Construct hedge)
        {
            var hitBox = hedge.GetHitBox();

            if (hitBox.Top > _scene_game.Height || hitBox.Left > _scene_game.Width)
            {
                hedge.IsAnimating = false;

                hedge.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        #endregion

        #region Honk

        private bool SpawnHonks()
        {
            for (int i = 0; i < 10; i++)
            {
                Honk honk = new(
                    animateAction: AnimateHonk,
                    recycleAction: RecycleHonk);

                honk.SetPosition(
                    left: -3000,
                    top: -3000);

                _scene_game.AddToScene(honk);
            }

            return true;
        }

        private bool GenerateHonk(Construct source)
        {
            if (_scene_game.Children.OfType<Honk>().FirstOrDefault(x => x.IsAnimating == false) is Honk honk)
            {
                honk.IsAnimating = true;
                honk.SetPopping();

                honk.Reset();

                var hitBox = source.GetCloseHitBox();

                honk.Reposition(source: source);
                honk.SetRotation(_random.Next(-30, 30));
                honk.SetZ(source.GetZ() + 1);

                source.SetPopping();

                return true;
            }

            return false;
        }

        private bool AnimateHonk(Construct honk)
        {
            honk.Pop();
            honk.Fade(0.06);
            return true;
        }

        private bool RecycleHonk(Construct honk)
        {
            if (honk.IsFadingComplete)
            {
                honk.IsAnimating = false;

                honk.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        private bool GenerateVehicleHonk(Vehicle source)
        {
            // if there are no bosses or enemies in the scene the vehicles will honk

            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !BossExists() && !EnemyExists())
            {
                return GenerateHonk(source);
            }

            return true;
        }

        private bool GenerateEnemyHonk(Enemy source)
        {
            // if there are no bosses in the scene the vehicles will honk

            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !BossExists())
            {
                return GenerateHonk(source);
            }

            return true;
        }

        #endregion

        #region Cloud

        private bool SpawnClouds()
        {
            for (int i = 0; i < 5; i++)
            {
                Cloud cloud = new(
                    animateAction: AnimateCloud,
                    recycleAction: RecycleCloud);

                cloud.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 9);

                _scene_game.AddToScene(cloud);

                //SpawnDropShadow(source: cloud);
            }

            return true;
        }

        private bool GenerateCloud()
        {
            if (_scene_game.Children.OfType<Cloud>().FirstOrDefault(x => x.IsAnimating == false) is Cloud cloud)
            {
                cloud.IsAnimating = true;
                cloud.Reset();

                var topOrLeft = _random.Next(2);

                var lane = _random.Next(2);

                switch (topOrLeft)
                {
                    case 0:
                        {
                            var xLaneWidth = _scene_game.Width / 4;
                            cloud.SetPosition(
                                left: _random.Next(Convert.ToInt32(xLaneWidth - cloud.Width)),
                                top: cloud.Height * -1);
                        }
                        break;
                    case 1:
                        {
                            var yLaneWidth = (_scene_game.Height / 2) / 2;
                            cloud.SetPosition(
                                left: cloud.Width * -1,
                                top: _random.Next(Convert.ToInt32(yLaneWidth)));
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
            MoveConstructBottomRight(construct: cloud, speed: speed);
            return true;
        }

        private bool RecycleCloud(Construct cloud)
        {
            var hitBox = cloud.GetHitBox();

            if (hitBox.Top > _scene_game.Height || hitBox.Left > _scene_game.Width)
            {
                cloud.IsAnimating = false;

                cloud.SetPosition(
                    left: -3000,
                    top: -3000);

            }

            return true;
        }

        #endregion

        #region Boss

        private bool SpawnBosses()
        {
            for (int i = 0; i < 3; i++)
            {
                Boss boss = new(
                    animateAction: AnimateBoss,
                    recycleAction: RecycleBoss);

                boss.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 8);

                _scene_game.AddToScene(boss);

                SpawnDropShadow(source: boss);
            }

            return true;
        }

        private bool GenerateBoss()
        {
            // if scene doesn't contain a boss then pick a random boss and add to scene

            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _boss_threashold.ShouldRelease(_game_score_bar.GetScore()) &&
                !_scene_game.Children.OfType<Boss>().Any(x => x.IsAnimating) &&
                _scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating == false) is Boss boss)
            {
                _audio_stub.Stop(SoundType.GAME_BACKGROUND_MUSIC);

                //_audio_stub.Play(SoundType.BOSS_BACKGROUND_MUSIC);

                _audio_stub.SetVolume(SoundType.AMBIENCE, 0.2);

                boss.IsAnimating = true;
                boss.Reset();
                boss.SetPosition(
                    left: 0,
                    top: boss.Height * -1);

                SyncDropShadow(boss);

                // set boss health
                boss.Health = _boss_threashold.GetReleasePointDifference() * 1.5;

                _boss_threashold.IncreaseThreasholdLimit(increment: _boss_threashold_limit_increase, currentPoint: _game_score_bar.GetScore());

                _boss_health_bar.SetMaxiumHealth(boss.Health);
                _boss_health_bar.SetValue(boss.Health);
                _boss_health_bar.SetIcon(boss.GetContentUri());
                _boss_health_bar.SetBarForegroundColor(color: Colors.Crimson);

                GenerateInterimScreen("Beware of Boss");
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
            }
            else
            {
                boss.Pop();
                boss1.Hover();
                boss1.DepleteHitStance();
                boss1.DepleteWinStance();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    var speed = (_scene_game.Speed + boss.SpeedOffset);

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
                        MoveConstructBottomRight(construct: boss, speed: speed);

                        if (boss.GetLeft() > (_scene_game.Width / 3)) // bring boss to a suitable distance from player and then start attacking
                        {
                            boss1.IsAttacking = true;
                        }
                    }
                }
            }

            return true;
        }

        private bool RecycleBoss(Construct boss)
        {
            if (boss.IsShrinkingComplete)
            {
                boss.IsAnimating = false;

                boss.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        private void LooseBossHealth(Boss boss)
        {
            boss.SetPopping();
            boss.LooseHealth();
            boss.SetHitStance();

            _boss_health_bar.SetValue(boss.Health);

            if (boss.IsDead && boss.IsAttacking)
            {
                //_audio_stub.Stop(SoundType.BOSS_BACKGROUND_MUSIC);

                _audio_stub.Play(SoundType.GAME_BACKGROUND_MUSIC);

                _audio_stub.SetVolume(SoundType.AMBIENCE, 0.6);

                boss.IsAttacking = false;

                _player.SetWinStance();
                _game_score_bar.GainScore(5);

                GenerateInterimScreen("Boss Busted");

                _scene_game.ActivateSlowMotion();
            }
        }

        private bool BossExists()
        {
            return _scene_game.Children.OfType<Boss>().Any(x => x.IsAnimating && x.IsAttacking);
        }

        #endregion

        #region Enemy

        private bool SpawnEnemys()
        {
            for (int i = 0; i < 10; i++)
            {
                Enemy enemy = new(
                    animateAction: AnimateEnemy,
                    recycleAction: RecycleEnemy);

                _scene_game.AddToScene(enemy);

                enemy.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 8);

                SpawnDropShadow(enemy);
            }

            return true;
        }

        private bool GenerateEnemy()
        {
            if (!BossExists() &&
                _enemy_threashold.ShouldRelease(_game_score_bar.GetScore()) &&
                _scene_game.Children.OfType<Enemy>().FirstOrDefault(x => x.IsAnimating == false) is Enemy enemy)
            {
                enemy.IsAnimating = true;
                enemy.Reset();

                var topOrLeft = _random.Next(2);

                switch (topOrLeft)
                {
                    case 0:
                        {
                            var xLaneWidth = _scene_game.Width / 2;

                            enemy.SetPosition(
                                left: _random.Next((int)(xLaneWidth - enemy.Width)),
                                top: enemy.Height * -1);
                        }
                        break;
                    case 1:
                        {
                            var yLaneWidth = _scene_game.Height / 2;

                            enemy.SetPosition(
                                left: enemy.Width * -1,
                                top: _random.Next((int)(yLaneWidth - enemy.Height)));
                        }
                        break;
                    default:
                        break;
                }

                SyncDropShadow(enemy);

                LoggerExtensions.Log("Enemy generated.");

                if (!_enemy_fleet_appeared)
                {
                    _audio_stub.Play(SoundType.ENEMY_ENTRY);

                    GenerateInterimScreen("Beware of Aliens");
                    _scene_game.ActivateSlowMotion();
                    _enemy_fleet_appeared = true;
                }

                return true;
            }

            return false;
        }

        private bool AnimateEnemy(Construct enemy)
        {
            Enemy enemy1 = enemy as Enemy;

            if (enemy1.IsDead)
            {
                enemy1.Shrink();
            }
            else
            {
                enemy1.Hover();
                enemy1.Pop();

                var speed = _scene_game.Speed + enemy.SpeedOffset;

                MoveConstructBottomRight(construct: enemy, speed: speed);

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (enemy1.Honk())
                        GenerateEnemyHonk(enemy1);

                    if (enemy1.Attack())
                        GenerateEnemyRocket(enemy1);
                }
            }

            return true;
        }

        private bool RecycleEnemy(Construct enemy)
        {
            var hitbox = enemy.GetHitBox();

            // enemy is dead or goes out of bounds
            if (enemy.IsShrinkingComplete ||
                hitbox.Left > _scene_game.Width || hitbox.Top > _scene_game.Height ||
                hitbox.Right < 0 || hitbox.Bottom < 0)
            {
                enemy.IsAnimating = false;

                enemy.SetPosition(
                    left: -3000,
                    top: -3000);

                LoggerExtensions.Log("Enemy Recycled");
            }

            return true;
        }

        private void LooseEnemyHealth(Enemy enemy)
        {
            enemy.SetPopping();
            enemy.LooseHealth();

            if (enemy.IsDead)
            {
                _game_score_bar.GainScore(5);

                _enemy_kill_count++;

                // after killing 15 enemies increase the threadhold limit
                if (_enemy_kill_count > _enemy_kill_count_limit)
                {
                    _enemy_threashold.IncreaseThreasholdLimit(increment: _enemy_threashold_limit_increase, currentPoint: _game_score_bar.GetScore());
                    _enemy_kill_count = 0;
                    _enemy_fleet_appeared = false;

                    GenerateInterimScreen("Alien Fleet Vanquished");
                    _scene_game.ActivateSlowMotion();
                }

                LoggerExtensions.Log("Enemy dead");
            }
        }

        private bool EnemyExists()
        {
            return _scene_game.Children.OfType<Enemy>().Any(x => x.IsAnimating);
        }

        #endregion

        #region PlayerFireCracker

        private bool SpawnPlayerFireCrackers()
        {
            for (int i = 0; i < 3; i++)
            {
                PlayerFireCracker bomb = new(
                    animateAction: AnimatePlayerFireCracker,
                    recycleAction: RecyclePlayerFireCracker);

                bomb.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 7);

                _scene_game.AddToScene(bomb);

                SpawnDropShadow(source: bomb);
            }

            return true;
        }

        private bool GeneratePlayerFireCracker()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !_scene_game.IsSlowMotionActivated)
            {
                if (_scene_game.Children.OfType<Vehicle>().Any(x => x.IsAnimating) &&
                    _scene_game.Children.OfType<PlayerFireCracker>().FirstOrDefault(x => x.IsAnimating == false) is PlayerFireCracker playerFireCracker)
                {
                    _player.SetAttackStance();

                    playerFireCracker.Reset();
                    playerFireCracker.IsAnimating = true;
                    playerFireCracker.IsGravitating = true;
                    playerFireCracker.SetPopping();

                    playerFireCracker.SetRotation(_random.Next(-30, 30));

                    playerFireCracker.Reposition(
                        player: _player);

                    SyncDropShadow(playerFireCracker);

                    LoggerExtensions.Log("Player Ground Bomb dropped.");

                    return true;
                }
                else
                {
                    _player.SetWinStance();
                }
            }

            return false;
        }

        private bool AnimatePlayerFireCracker(Construct playerFireCracker)
        {
            PlayerFireCracker playerFireCracker1 = playerFireCracker as PlayerFireCracker;

            var speed = (_scene_game.Speed + playerFireCracker.SpeedOffset);

            if (playerFireCracker1.IsBlasting)
            {
                playerFireCracker.Expand();
                playerFireCracker.Fade(0.02);

                MoveConstructBottomRight(construct: playerFireCracker, speed: speed);

                // while in blast check if it intersects with any vehicle, if it does then the vehicle stops honking and slows down
                if (_scene_game.Children.OfType<Vehicle>()
                    .Where(x => x.IsAnimating && x.WillHonk)
                    .FirstOrDefault(x => x.GetCloseHitBox().IntersectsWith(playerFireCracker.GetCloseHitBox())) is Vehicle vehicle)
                {
                    vehicle.SetBlast();
                    _game_score_bar.GainScore(5);
                }
            }
            else
            {
                playerFireCracker.Pop();

                playerFireCracker.SetLeft(playerFireCracker.GetLeft() + speed);
                playerFireCracker.SetTop(playerFireCracker.GetTop() + speed * 1.2);

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    DropShadow dropShadow = _scene_game.Children.OfType<DropShadow>().First(x => x.Id == playerFireCracker.Id);

                    var drpShdwHitBox = dropShadow.GetCloseHitBox();
                    var bmbHitBox = playerFireCracker.GetCloseHitBox();

                    // start blast animation when the bomb touches it's shadow
                    if (drpShdwHitBox.IntersectsWith(drpShdwHitBox) && playerFireCracker.GetBottom() > dropShadow.GetBottom())
                        playerFireCracker1.SetBlast();
                }
            }

            return true;
        }

        private bool RecyclePlayerFireCracker(Construct playerFireCracker)
        {
            if (playerFireCracker.IsFadingComplete)
            {
                playerFireCracker.IsAnimating = false;
                playerFireCracker.IsGravitating = false;

                playerFireCracker.SetPosition(
                    left: -3000,
                    top: -3000);

                return true;
            }

            return false;
        }

        #endregion

        #region Rocket

        private void SetPlayerRocketDirection(Construct source, Rocket rocket, Construct rocketTarget)
        {
            // rocket target is on the bottom right side of the boss
            if (rocketTarget.GetTop() > source.GetTop() && rocketTarget.GetLeft() > source.GetLeft())
            {
                rocket.AwaitMoveDownRight = true;
                rocket.SetRotation(33);
            }
            // rocket target is on the bottom left side of the boss
            else if (rocketTarget.GetTop() > source.GetTop() && rocketTarget.GetLeft() < source.GetLeft())
            {
                rocket.AwaitMoveDownLeft = true;
                rocket.SetRotation(-213);
            }
            // if rocket target is on the top left side of the boss
            else if (rocketTarget.GetTop() < source.GetTop() && rocketTarget.GetLeft() < source.GetLeft())
            {
                rocket.AwaitMoveUpLeft = true;
                rocket.SetRotation(213);
            }
            // if rocket target is on the top right side of the boss
            else if (rocketTarget.GetTop() < source.GetTop() && rocketTarget.GetLeft() > source.GetLeft())
            {
                rocket.AwaitMoveUpRight = true;
                rocket.SetRotation(-33);
            }
            else
            {
                rocket.AwaitMoveUpLeft = true;
                rocket.SetRotation(213);
            }
        }

        private void SetBossRocketDirection(Construct source, Rocket rocket, Construct rocketTarget)
        {
            // rocket target is on the bottom right side of the boss
            if (rocketTarget.GetTop() > source.GetTop() && rocketTarget.GetLeft() > source.GetLeft())
            {
                rocket.AwaitMoveDownRight = true;
                rocket.SetRotation(33);
            }
            // rocket target is on the bottom left side of the boss
            else if (rocketTarget.GetTop() > source.GetTop() && rocketTarget.GetLeft() < source.GetLeft())
            {
                rocket.AwaitMoveDownLeft = true;
                rocket.SetRotation(-213);
            }
            // if rocket target is on the top left side of the boss
            else if (rocketTarget.GetTop() < source.GetTop() && rocketTarget.GetLeft() < source.GetLeft())
            {
                rocket.AwaitMoveUpLeft = true;
                rocket.SetRotation(213);
            }
            // if rocket target is on the top right side of the boss
            else if (rocketTarget.GetTop() < source.GetTop() && rocketTarget.GetLeft() > source.GetLeft())
            {
                rocket.AwaitMoveUpRight = true;
                rocket.SetRotation(-33);
            }
            else
            {
                rocket.AwaitMoveDownRight = true;
                rocket.SetRotation(33);
            }
        }

        #endregion

        #region PlayerRocket

        private bool SpawnPlayerRockets()
        {
            for (int i = 0; i < 5; i++)
            {
                PlayerRocket bomb = new(
                    animateAction: AnimatePlayerRocket,
                    recycleAction: RecyclePlayerRocket);

                bomb.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 7);

                _scene_game.AddToScene(bomb);

                SpawnDropShadow(source: bomb);
            }

            return true;
        }

        private bool GeneratePlayerRocket()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !_scene_game.IsSlowMotionActivated &&
                _scene_game.Children.OfType<PlayerRocket>().FirstOrDefault(x => x.IsAnimating == false) is PlayerRocket playerRocket)
            {
                _player.SetAttackStance();

                playerRocket.Reset();
                playerRocket.IsAnimating = true;
                playerRocket.SetPopping();

                playerRocket.Reposition(
                    Player: _player);

                SyncDropShadow(playerRocket);

                var playerDistantHitBox = _player.GetDistantHitBox();

                // get closest possible target
                BossRocketSeeking bossRocketSeeking = _scene_game.Children.OfType<BossRocketSeeking>()?.FirstOrDefault(x => x.IsAnimating && !x.IsBlasting && x.GetHitBox().IntersectsWith(playerDistantHitBox));
                Boss boss = _scene_game.Children.OfType<Boss>()?.FirstOrDefault(x => x.IsAnimating && x.IsAttacking && x.GetHitBox().IntersectsWith(playerDistantHitBox));
                Enemy enemy = _scene_game.Children.OfType<Enemy>()?.FirstOrDefault(x => x.IsAnimating && !x.IsFadingComplete && x.GetHitBox().IntersectsWith(playerDistantHitBox));

                // if not found then find random target
                bossRocketSeeking ??= _scene_game.Children.OfType<BossRocketSeeking>().FirstOrDefault(x => x.IsAnimating && !x.IsBlasting);
                boss ??= _scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking);
                enemy ??= _scene_game.Children.OfType<Enemy>().FirstOrDefault(x => x.IsAnimating && !x.IsFadingComplete);

                LoggerExtensions.Log("Player Bomb dropped.");

                if (enemy is not null)
                {
                    SetPlayerRocketDirection(source: _player, rocket: playerRocket, rocketTarget: enemy);
                }
                else if (boss is not null)
                {
                    SetPlayerRocketDirection(source: _player, rocket: playerRocket, rocketTarget: boss);
                }
                else if (bossRocketSeeking is not null)
                {
                    SetPlayerRocketDirection(source: _player, rocket: playerRocket, rocketTarget: bossRocketSeeking);
                }

                return true;
            }

            return false;
        }

        private bool AnimatePlayerRocket(Construct bomb)
        {
            PlayerRocket PlayerRocket = bomb as PlayerRocket;

            var hitBox = PlayerRocket.GetCloseHitBox();

            var speed = (_scene_game.Speed + bomb.SpeedOffset);

            if (PlayerRocket.AwaitMoveDownLeft)
            {
                PlayerRocket.MoveDownLeft(speed);
            }
            else if (PlayerRocket.AwaitMoveUpRight)
            {
                PlayerRocket.MoveUpRight(speed);
            }
            else if (PlayerRocket.AwaitMoveUpLeft)
            {
                PlayerRocket.MoveUpLeft(speed);
            }
            else if (PlayerRocket.AwaitMoveDownRight)
            {
                PlayerRocket.MoveDownRight(speed);
            }

            if (PlayerRocket.IsBlasting)
            {
                bomb.Expand();
                bomb.Fade(0.02);
            }
            else
            {
                bomb.Pop();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    // if player bomb touches boss, it blasts, boss looses health
                    if (_scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking && x.GetCloseHitBox().IntersectsWith(hitBox)) is Boss boss)
                    {
                        PlayerRocket.SetBlast();
                        LooseBossHealth(boss);
                    }

                    // if player bomb touches boss's seeking bomb, it blasts
                    if (_scene_game.Children.OfType<BossRocketSeeking>().FirstOrDefault(x => x.IsAnimating && !x.IsBlasting && x.GetCloseHitBox().IntersectsWith(hitBox)) is BossRocketSeeking bossRocketSeeking)
                    {
                        PlayerRocket.SetBlast();
                        bossRocketSeeking.SetBlast();
                    }

                    // if player bomb touches enemy, it blasts, enemy looses health
                    if (_scene_game.Children.OfType<Enemy>().FirstOrDefault(x => x.IsAnimating && !x.IsDead && x.GetCloseHitBox().IntersectsWith(hitBox)) is Enemy enemy)
                    {
                        PlayerRocket.SetBlast();
                        LooseEnemyHealth(enemy);
                    }

                    if (PlayerRocket.AutoBlast())
                        PlayerRocket.SetBlast();
                }
            }

            return true;
        }

        private bool RecyclePlayerRocket(Construct playerRocket)
        {
            var hitbox = playerRocket.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (playerRocket.IsFadingComplete || hitbox.Left > _scene_game.Width || hitbox.Right < 0 /*|| hitbox.Top < 0 || hitbox.Top > _scene_game.Height*/)
            {
                playerRocket.IsAnimating = false;

                playerRocket.SetPosition(
                    left: -3000,
                    top: -3000);

                return true;
            }

            return false;
        }

        #endregion

        #region EnemyRocket

        private bool SpawnEnemyRockets()
        {
            for (int i = 0; i < 10; i++)
            {
                EnemyRocket bomb = new(
                    animateAction: AnimateEnemyRocket,
                    recycleAction: RecycleEnemyRocket);

                bomb.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 8);

                _scene_game.AddToScene(bomb);

                SpawnDropShadow(source: bomb);
            }

            return true;
        }

        private bool GenerateEnemyRocket(Enemy source)
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<EnemyRocket>().FirstOrDefault(x => x.IsAnimating == false) is EnemyRocket enemyRocket)
            {
                enemyRocket.Reset();
                enemyRocket.IsAnimating = true;
                enemyRocket.SetPopping();

                enemyRocket.Reposition(
                    Enemy: source);

                SyncDropShadow(enemyRocket);

                LoggerExtensions.Log("Enemy Bomb dropped.");

                return true;
            }

            return false;
        }

        private bool AnimateEnemyRocket(Construct bomb)
        {
            EnemyRocket enemyRocket = bomb as EnemyRocket;

            var speed = _scene_game.Speed + bomb.SpeedOffset;

            MoveConstructBottomRight(construct: enemyRocket, speed: speed);

            if (enemyRocket.IsBlasting)
            {
                bomb.Expand();
                bomb.Fade(0.02);
            }
            else
            {
                bomb.Pop();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (enemyRocket.GetCloseHitBox().IntersectsWith(_player.GetCloseHitBox()))
                    {
                        enemyRocket.SetBlast();
                        LoosePlayerHealth();
                    }

                    if (enemyRocket.AutoBlast())
                        enemyRocket.SetBlast();
                }
            }

            return true;
        }

        private bool RecycleEnemyRocket(Construct bomb)
        {
            var hitbox = bomb.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (bomb.IsFadingComplete || hitbox.Left > _scene_game.Width || hitbox.Right < 0 || hitbox.Top < 0 || hitbox.Bottom > _scene_game.Height)
            {
                bomb.IsAnimating = false;

                bomb.SetPosition(
                    left: -3000,
                    top: -3000);

                return true;
            }

            return false;
        }

        #endregion

        #region BossRocket

        private bool SpawnBossRockets()
        {
            for (int i = 0; i < 5; i++)
            {
                BossRocket bomb = new(
                    animateAction: AnimateBossRocket,
                    recycleAction: RecycleBossRocket);

                bomb.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 7);

                _scene_game.AddToScene(bomb);

                SpawnDropShadow(source: bomb);
            }

            return true;
        }

        private bool GenerateBossRocket()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is Boss boss &&
                _scene_game.Children.OfType<BossRocket>().FirstOrDefault(x => x.IsAnimating == false) is BossRocket bossRocket)
            {
                bossRocket.Reset();
                bossRocket.IsAnimating = true;
                bossRocket.SetPopping();

                bossRocket.Reposition(
                    boss: boss);

                SyncDropShadow(bossRocket);

                #region Target Based Movement

                SetBossRocketDirection(source: boss, rocket: bossRocket, rocketTarget: _player);

                #endregion

                LoggerExtensions.Log("Boss Bomb dropped.");

                return true;
            }

            return false;
        }

        private bool AnimateBossRocket(Construct bomb)
        {
            BossRocket BossRocket = bomb as BossRocket;

            var speed = (_scene_game.Speed + bomb.SpeedOffset);

            if (BossRocket.AwaitMoveDownLeft)
            {
                BossRocket.MoveDownLeft(speed);
            }
            else if (BossRocket.AwaitMoveUpRight)
            {
                BossRocket.MoveUpRight(speed);
            }
            else if (BossRocket.AwaitMoveUpLeft)
            {
                BossRocket.MoveUpLeft(speed);
            }
            else if (BossRocket.AwaitMoveDownRight)
            {
                BossRocket.MoveDownRight(speed);
            }

            if (BossRocket.IsBlasting)
            {
                bomb.Expand();
                bomb.Fade(0.02);
            }
            else
            {
                bomb.Pop();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (BossRocket.GetCloseHitBox().IntersectsWith(_player.GetCloseHitBox()))
                    {
                        BossRocket.SetBlast();
                        LoosePlayerHealth();
                    }

                    if (BossRocket.AutoBlast())
                        BossRocket.SetBlast();
                }
            }

            return true;
        }

        private bool RecycleBossRocket(Construct bomb)
        {
            //var hitbox = bomb.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (bomb.IsFadingComplete /*|| hitbox.Left > _scene_game.Width || hitbox.Right < 0 || hitbox.Top < 0 || hitbox.Top > _scene_game.Height*/)
            {
                bomb.IsAnimating = false;

                bomb.SetPosition(
                    left: -3000,
                    top: -3000);

                return true;
            }

            return false;
        }

        #endregion

        #region PlayerRocketSeeking

        private bool SpawnPlayerRocketSeekings()
        {
            for (int i = 0; i < 3; i++)
            {
                PlayerRocketSeeking bomb = new(
                    animateAction: AnimatePlayerRocketSeeking,
                    recycleAction: RecyclePlayerRocketSeeking);

                bomb.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 7);

                _scene_game.AddToScene(bomb);

                SpawnDropShadow(source: bomb);
            }

            return true;
        }

        private bool GeneratePlayerRocketSeeking()
        {
            // generate a seeking bomb if one is not in scene

            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !_scene_game.IsSlowMotionActivated &&
                _scene_game.Children.OfType<PlayerRocketSeeking>().FirstOrDefault(x => x.IsAnimating == false) is PlayerRocketSeeking playerRocketSeeking)
            {
                _player.SetAttackStance();

                playerRocketSeeking.Reset();
                playerRocketSeeking.IsAnimating = true;
                playerRocketSeeking.SetPopping();

                playerRocketSeeking.Reposition(
                    player: _player);

                SyncDropShadow(playerRocketSeeking);

                if (_powerUp_health_bar.HasHealth && (PowerUpType)_powerUp_health_bar.Tag == PowerUpType.SEEKING_BALLS)
                    DepletePowerUp();

                LoggerExtensions.Log("Player Seeking Bomb dropped.");

                return true;
            }

            return false;
        }

        private bool AnimatePlayerRocketSeeking(Construct playerRocketSeeking)
        {
            PlayerRocketSeeking playerRocketSeeking1 = playerRocketSeeking as PlayerRocketSeeking;

            if (playerRocketSeeking1.IsBlasting)
            {
                var speed = _scene_game.Speed + playerRocketSeeking.SpeedOffset;

                MoveConstructBottomRight(construct: playerRocketSeeking1, speed: speed);

                playerRocketSeeking.Expand();
                playerRocketSeeking.Fade(0.02);
            }
            else
            {
                playerRocketSeeking.Pop();
                playerRocketSeeking.Rotate(rotationSpeed: 3.5);

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (_scene_game.Children.OfType<BossRocketSeeking>().FirstOrDefault(x => x.IsAnimating && !x.IsBlasting) is BossRocketSeeking bossRocketSeeking) // target boss bomb seeking
                    {
                        playerRocketSeeking1.Seek(bossRocketSeeking.GetCloseHitBox());

                        if (playerRocketSeeking1.GetCloseHitBox().IntersectsWith(bossRocketSeeking.GetCloseHitBox()))
                        {
                            playerRocketSeeking1.SetBlast();
                            bossRocketSeeking.SetBlast();
                        }
                    }
                    else if (_scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is Boss boss) // target boss
                    {
                        playerRocketSeeking1.Seek(boss.GetCloseHitBox());

                        if (playerRocketSeeking1.GetCloseHitBox().IntersectsWith(boss.GetCloseHitBox()))
                        {
                            playerRocketSeeking1.SetBlast();
                            LooseBossHealth(boss);
                        }
                    }
                    else if (_scene_game.Children.OfType<Enemy>().FirstOrDefault(x => x.IsAnimating && !x.IsFadingComplete) is Enemy enemy) // target enemy
                    {
                        playerRocketSeeking1.Seek(enemy.GetCloseHitBox());

                        if (playerRocketSeeking1.GetCloseHitBox().IntersectsWith(enemy.GetCloseHitBox()))
                        {
                            playerRocketSeeking1.SetBlast();
                            LooseEnemyHealth(enemy);
                        }
                    }

                    if (playerRocketSeeking1.RunOutOfTimeToBlast())
                        playerRocketSeeking1.SetBlast();
                }
            }

            return true;
        }

        private bool RecyclePlayerRocketSeeking(Construct playerRocketSeeking)
        {
            var hitbox = playerRocketSeeking.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (playerRocketSeeking.IsFadingComplete || hitbox.Left > _scene_game.Width || hitbox.Right < 0 || hitbox.Top < 0 || hitbox.Bottom > _scene_game.Height)
            {
                playerRocketSeeking.IsAnimating = false;

                playerRocketSeeking.SetPosition(
                    left: -3000,
                    top: -3000);

                return true;
            }

            return false;
        }

        private void DepletePowerUp()
        {
            // use up the power up
            if (_powerUp_health_bar.HasHealth)
                _powerUp_health_bar.SetValue(_powerUp_health_bar.GetValue() - 1);
        }

        #endregion

        #region BossRocketSeeking

        private bool SpawnBossRocketSeekings()
        {
            for (int i = 0; i < 2; i++)
            {
                BossRocketSeeking bomb = new(
                    animateAction: AnimateBossRocketSeeking,
                    recycleAction: RecycleBossRocketSeeking);

                bomb.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 7);

                _scene_game.AddToScene(bomb);

                SpawnDropShadow(source: bomb);
            }

            return true;
        }

        private bool GenerateBossRocketSeeking()
        {
            // generate a seeking bomb if one is not in scene
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is Boss boss &&
                !_scene_game.Children.OfType<BossRocketSeeking>().Any(x => x.IsAnimating) &&
                _scene_game.Children.OfType<BossRocketSeeking>().FirstOrDefault(x => x.IsAnimating == false) is BossRocketSeeking bossRocketSeeking)
            {
                bossRocketSeeking.Reset();
                bossRocketSeeking.IsAnimating = true;
                bossRocketSeeking.SetPopping();

                bossRocketSeeking.Reposition(
                    boss: boss);

                SyncDropShadow(bossRocketSeeking);

                LoggerExtensions.Log("Boss Seeking Bomb dropped.");

                return true;
            }

            return false;
        }

        private bool AnimateBossRocketSeeking(Construct bossRocketSeeking)
        {
            BossRocketSeeking bossRocketSeeking1 = bossRocketSeeking as BossRocketSeeking;

            var speed = (_scene_game.Speed + bossRocketSeeking.SpeedOffset);

            if (bossRocketSeeking1.IsBlasting)
            {
                MoveConstructBottomRight(construct: bossRocketSeeking1, speed: speed);

                bossRocketSeeking.Expand();
                bossRocketSeeking.Fade(0.02);
            }
            else
            {
                bossRocketSeeking.Pop();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (_scene_game.Children.OfType<Boss>().Any(x => x.IsAnimating && x.IsAttacking))
                    {
                        bossRocketSeeking1.Seek(_player.GetCloseHitBox());

                        if (bossRocketSeeking1.GetCloseHitBox().IntersectsWith(_player.GetCloseHitBox()))
                        {
                            bossRocketSeeking1.SetBlast();
                            LoosePlayerHealth();
                        }
                        else
                        {
                            if (bossRocketSeeking1.RunOutOfTimeToBlast())
                                bossRocketSeeking1.SetBlast();
                        }
                    }
                    else
                    {
                        bossRocketSeeking1.SetBlast();
                    }
                }
            }

            return true;
        }

        private bool RecycleBossRocketSeeking(Construct bomb)
        {
            var hitbox = bomb.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (bomb.IsFadingComplete || hitbox.Left > _scene_game.Width || hitbox.Right < 0 || hitbox.Top < 0 || hitbox.Bottom > _scene_game.Height)
            {
                bomb.IsAnimating = false;

                bomb.SetPosition(
                    left: -3000,
                    top: -3000);

                return true;
            }

            return false;
        }

        #endregion

        #region DropShadow

        private bool SpawnDropShadow(Construct source)
        {
            DropShadow dropShadow = new(
                animateAction: AnimateDropShadow,
                recycleAction: RecycleDropShadow);

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
                    left: -3000,
                    top: -3000);

                return true;
            }

            return false;
        }

        private void SyncDropShadow(Construct source)
        {
            if (_scene_game.Children.OfType<DropShadow>().FirstOrDefault(x => x.Id == source.Id) is DropShadow dropShadow)
            {
                dropShadow.ParentConstructSpeed = _scene_game.Speed + source.SpeedOffset;
                dropShadow.IsAnimating = true;

                dropShadow.SetZ(source.GetZ() - 2);

                dropShadow.Reset();
            }
        }

        #endregion

        #region HealthPickup

        private bool SpawnHealthPickups()
        {
            for (int i = 0; i < 3; i++)
            {
                HealthPickup HealthPickup = new(
                    animateAction: AnimateHealthPickup,
                    recycleAction: RecycleHealthPickup);

                HealthPickup.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 6);

                _scene_game.AddToScene(HealthPickup);
            }

            return true;
        }

        private bool GenerateHealthPickups()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING && HealthPickup.ShouldGenerate(_player.Health) &&
                _scene_game.Children.OfType<HealthPickup>().FirstOrDefault(x => x.IsAnimating == false) is HealthPickup healthPickup)
            {
                healthPickup.IsAnimating = true;
                healthPickup.Reset();

                var topOrLeft = _random.Next(2);

                var lane = _random.Next(2);

                switch (topOrLeft)
                {
                    case 0:
                        {
                            var xLaneWidth = _scene_game.Width / 4;
                            healthPickup.SetPosition(
                                left: _random.Next(Convert.ToInt32(xLaneWidth - healthPickup.Width)),
                                top: healthPickup.Height * -1);
                        }
                        break;
                    case 1:
                        {
                            var yLaneWidth = (_scene_game.Height / 2) / 2;
                            healthPickup.SetPosition(
                                left: healthPickup.Width * -1,
                                top: _random.Next(Convert.ToInt32(yLaneWidth)));
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
                MoveConstructBottomRight(construct: healthPickup, speed: speed);

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    var hitbox = healthPickup.GetCloseHitBox();

                    if (_player.GetCloseHitBox().IntersectsWith(hitbox))
                    {
                        healthPickup1.PickedUp();

                        _player.GainHealth();
                        _player_health_bar.SetValue(_player.Health);
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
                    left: -3000,
                    top: -3000);

                healthPickup.IsAnimating = false;
            }

            return true;
        }

        #endregion

        #region PowerUpPickup

        private bool SpawnPowerUpPickups()
        {
            for (int i = 0; i < 3; i++)
            {
                PowerUpPickup powerUpPickup = new(
                    animateAction: AnimatePowerUpPickup,
                    recycleAction: RecyclePowerUpPickup);

                powerUpPickup.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 6);

                _scene_game.AddToScene(powerUpPickup);
            }

            return true;
        }

        private bool GeneratePowerUpPickups()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING)
            {
                if ((BossExists() || EnemyExists()) && !_powerUp_health_bar.HasHealth) // if a boss or enemy exists and currently player has no other power up
                {
                    if (_scene_game.Children.OfType<PowerUpPickup>().FirstOrDefault(x => x.IsAnimating == false) is PowerUpPickup powerUpPickup)
                    {
                        powerUpPickup.IsAnimating = true;
                        powerUpPickup.Reset();

                        var topOrLeft = _random.Next(2);

                        var lane = _random.Next(2);

                        switch (topOrLeft)
                        {
                            case 0:
                                {
                                    var xLaneWidth = _scene_game.Width / 4;
                                    powerUpPickup.SetPosition(
                                        left: _random.Next(Convert.ToInt32(xLaneWidth - powerUpPickup.Width)),
                                        top: powerUpPickup.Height * -1);
                                }
                                break;
                            case 1:
                                {
                                    var yLaneWidth = (_scene_game.Height / 2) / 2;
                                    powerUpPickup.SetPosition(
                                        left: powerUpPickup.Width * -1,
                                        top: _random.Next(Convert.ToInt32(yLaneWidth)));
                                }
                                break;
                            default:
                                break;
                        }

                        SyncDropShadow(powerUpPickup);

                        return true;
                    }
                }
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
                MoveConstructBottomRight(construct: powerUpPickup, speed: speed);

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    var hitbox = powerUpPickup.GetCloseHitBox();

                    // if player picks up seeking bomb pickup
                    if (_player.GetCloseHitBox().IntersectsWith(hitbox))
                    {
                        powerUpPickup1.PickedUp();

                        // if seeking balls powerup, allow using a burst of 3 seeking bombs 3 times
                        _powerUp_health_bar.Tag = powerUpPickup1.PowerUpType;
                        _powerUp_health_bar.SetMaxiumHealth(9);
                        _powerUp_health_bar.SetValue(9);
                        _powerUp_health_bar.SetIcon(powerUpPickup1.GetContentUri());
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
                powerUpPickup.IsAnimating = false;

                powerUpPickup.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        #endregion

        #region Construct

        private void MoveConstructBottomRight(Construct construct, double speed)
        {
            //speed *= _scene_game.DownScaling;

            if (_scene_game.IsSlowMotionActivated)
                speed /= Constants.DEFAULT_SLOW_MOTION_REDUCTION_FACTOR;

            construct.SetLeft(construct.GetLeft() + speed);
            construct.SetTop(construct.GetTop() + speed * construct.IsometricDisplacement);
        }

        #endregion

        #region Controller

        private void SetController()
        {
            _game_controller.SetScene(_scene_game);
            _game_controller.SetPauseAction(PauseGame);
            _game_controller.SetGyrometer();
        }

        private void UnsetController()
        {
            _game_controller.SetScene(null);
            _game_controller.UnsetGyrometer();
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

            AddGeneratorsToScene();

            _scene_game.Speed = 5;

            if (ScreenExtensions.GetDisplayOrienation() == ScreenExtensions.RequiredDisplayOrientation)
                _scene_game.Play();

            _scene_main_menu.Speed = 5;
            _scene_main_menu.Play();
        }

        private void AddGeneratorsToScene()
        {
            // add road

            _scene_game.AddToScene(new Generator(
               generationDelay: 35,
               generationAction: GenerateRoadSideStripeTop,
               startUpAction: SpawnRoadSideStripes));

            _scene_game.AddToScene(new Generator(
                generationDelay: 35,
                generationAction: GenerateRoadSideStripeBottom,
                startUpAction: SpawnRoadSideStripes));

            _scene_game.AddToScene(

                // add road side patches
                new Generator(
                    generationDelay: 60,
                    generationAction: GenerateRoadSidePatchBottom,
                    startUpAction: SpawnRoadSidePatchs),

                new Generator(
                   generationDelay: 60,
                   generationAction: GenerateRoadSidePatchTop,
                   startUpAction: SpawnRoadSidePatchs),

                // then add road marks
                new Generator(
                    generationDelay: 30,
                    generationAction: GenerateRoadMark,
                    startUpAction: SpawnRoadMarks),

                // then add the top trees
                new Generator(
                    generationDelay: 30,
                    generationAction: GenerateTreeTop,
                    startUpAction: SpawnTrees),

                // then add the bottom trees which will appear forward in z wrt to the vehicles
                new Generator(
                    generationDelay: 30,
                    generationAction: GenerateTreeBottom,
                    startUpAction: SpawnTrees),

                // then add the top Hedges
                new Generator(
                    generationDelay: 12,
                    generationAction: GenerateHedgeTop,
                    startUpAction: SpawnHedges),

                // then add the bottom Hedges which will appear forward in z wrt to the vehicles
                new Generator(
                    generationDelay: 12,
                    generationAction: GenerateHedgeBottom,
                    startUpAction: SpawnHedges),

                // then add the vehicles which will appear forward in z wrt the top trees
                new Generator(
                    generationDelay: 100,
                    generationAction: GenerateVehicle,
                    startUpAction: SpawnVehicles),

                // add the honks which will appear forward in z wrt to everything on the road
                new Generator(
                    generationDelay: 0,
                    generationAction: () => { return true; },
                    startUpAction: SpawnHonks),

                // add the player in scene which will appear forward in z wrt to all else
                new Generator(
                    generationDelay: 0,
                    generationAction: () => { return true; },
                    startUpAction: SpawnPlayerBalloon),

                new Generator(
                    generationDelay: 0,
                    generationAction: () => { return true; },
                    startUpAction: SpawnPlayerRockets),

                new Generator(
                    generationDelay: 0,
                    generationAction: () => { return true; },
                    startUpAction: SpawnPlayerFireCrackers),

                // add the clouds which are above the player z
                new Generator(
                    generationDelay: 400,
                    generationAction: GenerateCloud,
                    startUpAction: SpawnClouds,
                    randomizeGenerationDelay: true),

                new Generator(
                    generationDelay: 100,
                    generationAction: GenerateBoss,
                    startUpAction: SpawnBosses),

                new Generator(
                    generationDelay: 50,
                    generationAction: GenerateBossRocket,
                    startUpAction: SpawnBossRockets,
                    randomizeGenerationDelay: true),

                new Generator(
                    generationDelay: 200,
                    generationAction: GenerateBossRocketSeeking,
                    startUpAction: SpawnBossRocketSeekings,
                    randomizeGenerationDelay: true),

                new Generator(
                    generationDelay: 0,
                    generationAction: () => { return true; },
                    startUpAction: SpawnPlayerRocketSeekings),

                new Generator(
                    generationDelay: 600,
                    generationAction: GenerateHealthPickups,
                    startUpAction: SpawnHealthPickups,
                    randomizeGenerationDelay: true),

                new Generator(
                    generationDelay: 600,
                    generationAction: GeneratePowerUpPickups,
                    startUpAction: SpawnPowerUpPickups,
                    randomizeGenerationDelay: true),

                new Generator(
                    generationDelay: 0,
                    generationAction: () => { return true; },
                    startUpAction: SpawnInterimScreen),

                new Generator(
                    generationDelay: 180,
                    generationAction: GenerateEnemy,
                    startUpAction: SpawnEnemys,
                    randomizeGenerationDelay: true),

                 new Generator(
                    generationDelay: 0,
                    generationAction: () => { return true; },
                    startUpAction: SpawnEnemyRockets)
                );

            _scene_main_menu.AddToScene(

                new Generator(
                    generationDelay: 0,
                    generationAction: () => { return true; },
                    startUpAction: SpawnTitleScreen),

                new Generator(
                    generationDelay: 0,
                    generationAction: () => { return true; },
                    startUpAction: SpawnPlayerSelectionScreen),

                  new Generator(
                    generationDelay: 0,
                    generationAction: () => { return true; },
                    startUpAction: SpawnDisplayOrientationChangeScreen)
                );
        }

        #endregion

        #endregion

        #region Events

        private async void HonkBomberPage_Loaded(object sender, RoutedEventArgs e)
        {
            ScreenExtensions.DisplayInformation.OrientationChanged += DisplayInformation_OrientationChanged;
            ScreenExtensions.RequiredDisplayOrientation = DisplayOrientations.Landscape;

            // set display orientation to required orientation
            if (ScreenExtensions.GetDisplayOrienation() != ScreenExtensions.RequiredDisplayOrientation)
                ScreenExtensions.SetDisplayOrientation(ScreenExtensions.RequiredDisplayOrientation);

            SetController();
            SetScene();

            SizeChanged += HonkBomberPage_SizeChanged;

            if (ScreenExtensions.GetDisplayOrienation() == ScreenExtensions.RequiredDisplayOrientation)
            {
                ScreenExtensions.EnterFullScreen(true);

                await Task.Delay(1500);

                GenerateTitleScreen("Honk Trooper");
                _audio_stub.Play(SoundType.GAME_BACKGROUND_MUSIC);
            }
            else
            {
                GenerateDisplayOrientationChangeScreen();
            }
        }

        private void HonkBomberPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= HonkBomberPage_SizeChanged;
            ScreenExtensions.DisplayInformation.OrientationChanged -= DisplayInformation_OrientationChanged;
            UnsetController();
        }

        private void HonkBomberPage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            var _windowWidth = args.NewSize.Width;
            var _windowHeight = args.NewSize.Height;

            var scaling = ScreenExtensions.GetScreenSpaceScaling(_windowWidth);

            // resize the main menu
            _scene_main_menu.Width = _windowWidth;
            _scene_main_menu.Height = _windowHeight;

            // scale the scenes
            _scene_game.SetScaleTransform(scaling);
            _scene_main_menu.SetScaleTransform(scaling);

            if (_scene_game.SceneState == SceneState.GAME_RUNNING)
            {
                _player.Reposition();
                SyncDropShadow(_player);
            }

            RepositionHoveringTitleScreens();
            RepositionLogicalConstructs();
        }

        private void DisplayInformation_OrientationChanged(DisplayInformation sender, object args)
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING) // if screen orientation is changed while game is running, pause the game
            {
                if (_scene_game.IsAnimating)
                    PauseGame();
            }
            else
            {
                if (ScreenExtensions.GetDisplayOrienation() == ScreenExtensions.RequiredDisplayOrientation)
                {
                    if (_scene_main_menu.Children.OfType<DisplayOrientationChangeScreen>().FirstOrDefault(x => x.IsAnimating) is DisplayOrientationChangeScreen DisplayOrientationChangeScreen)
                    {
                        RecycleDisplayOrientationChangeScreen(DisplayOrientationChangeScreen);

                        _audio_stub.Play(SoundType.GAME_BACKGROUND_MUSIC);
                        GenerateTitleScreen("Honk Trooper");

                        if (!_scene_game.IsAnimating)
                            _scene_game.Play();

                        if (!_scene_main_menu.IsAnimating)
                            _scene_main_menu.Play();
                    }
                }
                else // ask to change orientation
                {
                    if (_scene_game.IsAnimating)
                        _scene_game.Pause();

                    if (!_scene_main_menu.IsAnimating)
                        _scene_main_menu.Play();

                    foreach (var hoveringTitleScreen in _scene_main_menu.Children.OfType<HoveringTitleScreen>().Where(x => x.IsAnimating))
                    {
                        hoveringTitleScreen.IsAnimating = false;
                        hoveringTitleScreen.SetPosition(left: -3000, top: -3000);
                    }

                    foreach (var construct in _scene_game.Children.OfType<Construct>())
                    {
                        construct.IsAnimating = false;
                        construct.SetPosition(left: -3000, top: -3000);
                    }

                    GenerateDisplayOrientationChangeScreen();
                }

                ScreenExtensions.EnterFullScreen(true);
            }

            LoggerExtensions.Log($"{sender.CurrentOrientation}");
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            PauseGame();
        }

        #endregion
    }
}
