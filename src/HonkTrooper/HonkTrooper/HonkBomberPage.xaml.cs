using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using Windows.Graphics.Display;

namespace HonkTrooper
{
    public sealed partial class HonkBomberPage : Page
    {
        #region Fields

        private Player _player;
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

        //TODO: set defaults
        private readonly double _boss_threashold_limit = 50; // after reaching 50 score first boss will appear
        private readonly double _boss_threashold_limit_increase = 15;

        //TODO: set defaults
        private readonly double _enemy_threashold_limit = 100; // after reaching 100 score first enemies will appear
        private readonly double _enemy_threashold_limit_increase = 10;

        private double _enemy_kill_count;
        private readonly double _enemy_kill_count_limit = 30;

        private bool _enemy_appeared;

        private readonly Sound[] _ambience_sounds;
        private readonly Sound[] _game_background_music_sounds;
        private readonly Sound[] _enemy_entry_sounds;
        private readonly Sound[] _game_start_sounds;
        private readonly Sound[] _game_pause_sounds;
        private readonly Sound[] _game_over_sounds;


        private Sound _game_start_sound_playing;
        private Sound _game_pause_sound_playing;
        private Sound _game_over_sound_playing;
        private Sound _ambience_sound_playing;
        private Sound _game_background_music_sound_playing;

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

            _game_background_music_sounds = Constants.SOUND_TEMPLATES.Where(x => x.SoundType == SoundType.GAME_BACKGROUND_MUSIC).Select(x => x.Uri).Select(uri => new Sound(uri: uri, volume: 0.5, loop: true)).ToArray();
            _ambience_sounds = Constants.SOUND_TEMPLATES.Where(x => x.SoundType == SoundType.AMBIENCE).Select(x => x.Uri).Select(uri => new Sound(uri: uri, volume: 0.4, loop: true)).ToArray();

            _game_start_sounds = Constants.SOUND_TEMPLATES.Where(x => x.SoundType == SoundType.GAME_START).Select(x => x.Uri).Select(uri => new Sound(uri: uri)).ToArray();
            _game_pause_sounds = Constants.SOUND_TEMPLATES.Where(x => x.SoundType == SoundType.GAME_PAUSE).Select(x => x.Uri).Select(uri => new Sound(uri: uri)).ToArray();
            _game_over_sounds = Constants.SOUND_TEMPLATES.Where(x => x.SoundType == SoundType.GAME_OVER).Select(x => x.Uri).Select(uri => new Sound(uri: uri)).ToArray();

            _enemy_entry_sounds = Constants.SOUND_TEMPLATES.Where(x => x.SoundType == SoundType.ENEMY_ENTRY).Select(x => x.Uri).Select(uri => new Sound(uri: uri)).ToArray();

            Loaded += HonkBomberPage_Loaded;
            Unloaded += HonkBomberPage_Unloaded;
        }

        #endregion

        #region Methods

        #region Game

        private void PauseGame()
        {
            PlayGamePauseSound();
            PauseSoundLoops();

            ToggleHudVisibility(Visibility.Collapsed);

            _scene_game.Pause();
            _scene_main_menu.Play();

            GenerateTitleScreenInScene("Game Paused");
        }

        private void ResumeGame(TitleScreen se)
        {
            ResumeSoundLoops();

            ToggleHudVisibility(Visibility.Visible);

            _scene_game.Play();
            _scene_main_menu.Pause();

            RecycleTitleScreen(se);

            _game_controller.AttackButton.Focus(FocusState.Programmatic);
        }

        private void NewGame(TitleScreen se)
        {
            PlaySoundLoops();

            _game_controller.Reset();

            _powerUp_health_bar.Reset();
            _boss_health_bar.Reset();
            _game_score_bar.Reset();

            _boss_threashold.Reset(_boss_threashold_limit);
            _enemy_threashold.Reset(_enemy_threashold_limit);
            _enemy_kill_count = 0;
            _enemy_appeared = false;

            GeneratePlayerInScene();

            foreach (var vehicle in _scene_game.Children.OfType<Vehicle>())
            {
                vehicle.SetPosition(
                     left: -500,
                     top: -500);

                vehicle.IsAnimating = false;
            }

            foreach (var honk in _scene_game.Children.OfType<Honk>())
            {
                honk.SetPosition(
                     left: -500,
                     top: -500);

                honk.IsAnimating = false;
            }

            foreach (var playerRocket in _scene_game.Children.OfType<PlayerRocket>())
            {
                playerRocket.SetPosition(
                     left: -500,
                     top: -500);

                playerRocket.IsAnimating = false;
            }

            foreach (var playerRocket in _scene_game.Children.OfType<PlayerRocketSeeking>())
            {
                playerRocket.SetPosition(
                     left: -500,
                     top: -500);

                playerRocket.IsAnimating = false;
            }

            if (_scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating) is Boss boss)
            {
                boss.Health = 0;
                boss.IsAttacking = false;
                boss.IsAnimating = false;

                boss.SetPosition(left: -500, top: -500);

                //Console.WriteLine("Boss relocated");
            }

            foreach (var bossRocket in _scene_game.Children.OfType<BossRocket>())
            {
                bossRocket.SetPosition(
                     left: -500,
                     top: -500);

                bossRocket.IsAnimating = false;
            }

            foreach (var bossRocket in _scene_game.Children.OfType<BossRocketSeeking>())
            {
                bossRocket.SetPosition(
                     left: -500,
                     top: -500);

                bossRocket.IsAnimating = false;
            }

            foreach (var enemy in _scene_game.Children.OfType<Enemy>())
            {
                enemy.SetPosition(
                     left: -500,
                     top: -500);

                enemy.IsAnimating = false;
            }

            foreach (var enemyRocket in _scene_game.Children.OfType<EnemyRocket>())
            {
                enemyRocket.SetPosition(
                     left: -500,
                     top: -500);

                enemyRocket.IsAnimating = false;
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
                StopSoundLoops();

                if (_scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating) is Boss boss)
                    boss.StopSoundLoops();

                PlayGameOverSound();

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
                    PlayGameStartSound();

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
                interimScreen.IsAnimating = true;
                interimScreen.SetTitle(title);
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
            _player.DepleteWinStance();
            _player.DepleteHitStance();

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
                    if (BossExistsInScene() || EnemyExistsInScene())
                    {
                        if (_powerUp_health_bar.HasHealth && (PowerUpType)_powerUp_health_bar.Tag == PowerUpType.SEEKING_BALLS)
                            GeneratePlayerRocketSeekingInScene();
                        else
                            GeneratePlayerRocketInScene();
                    }
                    else
                    {
                        GeneratePlayerFireCrackerInScene();
                    }

                    _game_controller.IsAttacking = false;
                }
            }

            return true;
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

                GameOver();
            }
        }

        #endregion

        #region PlayerRocket

        private bool SpawnPlayerRocketsInScene()
        {
            for (int i = 0; i < 4; i++)
            {
                PlayerRocket bomb = new(
                    animateAction: AnimatePlayerRocket,
                    recycleAction: RecyclePlayerRocket,
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

        private bool GeneratePlayerRocketInScene()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !_scene_game.IsSlowMotionActivated &&
                _scene_game.Children.OfType<PlayerRocket>().FirstOrDefault(x => x.IsAnimating == false) is PlayerRocket PlayerRocket)
            {
                _player.SetAttackStance();

                PlayerRocket.Reset();
                PlayerRocket.IsAnimating = true;
                PlayerRocket.SetPopping();

                PlayerRocket.Reposition(
                    Player: _player,
                    downScaling: _scene_game.DownScaling);

                SyncDropShadow(PlayerRocket);

                var playerDistantHitBox = _player.GetDistantHitBox();

                // get closest possible target
                BossRocketSeeking bossRocketSeeking = _scene_game.Children.OfType<BossRocketSeeking>().FirstOrDefault(x => x.IsAnimating && x.GetHitBox().IntersectsWith(playerDistantHitBox));
                Boss boss = _scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking && x.GetHitBox().IntersectsWith(playerDistantHitBox));
                Enemy enemy = _scene_game.Children.OfType<Enemy>().FirstOrDefault(x => x.IsAnimating && x.GetHitBox().IntersectsWith(playerDistantHitBox));

                // if not found then find random target
                bossRocketSeeking ??= _scene_game.Children.OfType<BossRocketSeeking>().FirstOrDefault(x => x.IsAnimating);
                boss ??= _scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking);
                enemy ??= _scene_game.Children.OfType<Enemy>().FirstOrDefault(x => x.IsAnimating);

                // Console.WriteLine("Player Bomb dropped.");

                #region Target Based Movement

                if (enemy is not null)
                {
                    SetPlayerRocketDirection(playerRocket: PlayerRocket, target: enemy);
                }
                else if (bossRocketSeeking is not null)
                {
                    SetPlayerRocketDirection(playerRocket: PlayerRocket, target: bossRocketSeeking);
                }
                else if (boss is not null)
                {
                    SetPlayerRocketDirection(playerRocket: PlayerRocket, target: boss);
                }

                #endregion

                return true;
            }

            return false;
        }

        private void SetPlayerRocketDirection(PlayerRocket playerRocket, Construct target)
        {
            if (_player.GetLeft() < target.GetLeft()) // player is on the left side of the target
            {
                if ((_player.GetTop() > target.GetTop())) // player is below the target
                {
                    playerRocket.AwaitMoveRight = true;
                    playerRocket.SetRotation(-33);
                }
                else // player is above the target
                {
                    playerRocket.AwaitMoveDown = true;
                    playerRocket.SetRotation(123);
                }
            }
            else if (_player.GetLeft() > target.GetLeft()) // player is on the right side of the target
            {
                if ((_player.GetTop() > target.GetTop())) // player is below the target
                {
                    playerRocket.AwaitMoveUp = true;
                    playerRocket.SetRotation(213);

                }
                else // player is above the target
                {
                    playerRocket.AwaitMoveLeft = true;
                    playerRocket.SetRotation(123);
                }
            }
        }

        private bool AnimatePlayerRocket(Construct bomb)
        {
            PlayerRocket PlayerRocket = bomb as PlayerRocket;

            var hitBox = PlayerRocket.GetCloseHitBox();

            var speed = (_scene_game.Speed + bomb.SpeedOffset) * _scene_game.DownScaling;

            if (PlayerRocket.AwaitMoveLeft)
            {
                PlayerRocket.MoveLeft(speed);
            }
            else if (PlayerRocket.AwaitMoveRight)
            {
                PlayerRocket.MoveRight(speed);
            }
            else if (PlayerRocket.AwaitMoveUp)
            {
                PlayerRocket.MoveUp(speed);
            }
            else if (PlayerRocket.AwaitMoveDown)
            {
                PlayerRocket.MoveDown(speed);
            }

            if (PlayerRocket.IsBlasting)
            {
                bomb.Expand();
                bomb.Fade(0.02);

                //DropShadow dropShadow = _scene_game.Children.OfType<DropShadow>().First(x => x.Id == bomb.Id);
                //dropShadow.Opacity = bomb.Opacity;
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
                    if (_scene_game.Children.OfType<BossRocketSeeking>().FirstOrDefault(x => x.IsAnimating && x.GetCloseHitBox().IntersectsWith(hitBox)) is BossRocketSeeking BossRocketSeeking)
                    {
                        PlayerRocket.SetBlast();
                        BossRocketSeeking.SetBlast();
                    }

                    // if player bomb touches enemy, it blasts, enemy looses health
                    if (_scene_game.Children.OfType<Enemy>().FirstOrDefault(x => x.IsAnimating && x.GetCloseHitBox().IntersectsWith(hitBox)) is Enemy enemy)
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

        private bool RecyclePlayerRocket(Construct PlayerRocket)
        {
            var hitbox = PlayerRocket.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (PlayerRocket.IsFadingComplete ||
                hitbox.Left > _scene_game.Width || hitbox.Top > _scene_game.Height ||
                hitbox.Right < 0 || hitbox.Bottom < 0)
            {
                PlayerRocket.IsAnimating = false;

                PlayerRocket.SetPosition(
                    left: -500,
                    top: -500);

                return true;
            }

            return false;
        }

        #endregion

        #region PlayerFireCracker

        private bool SpawnPlayerFireCrackersInScene()
        {
            for (int i = 0; i < 3; i++)
            {
                PlayerFireCracker bomb = new(
                    animateAction: AnimatePlayerFireCracker,
                    recycleAction: RecyclePlayerFireCracker,
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

        private bool GeneratePlayerFireCrackerInScene()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !_scene_game.IsSlowMotionActivated &&
                _scene_game.Children.OfType<Vehicle>().Any(x => x.IsAnimating && x.WillHonk) &&
                _scene_game.Children.OfType<PlayerFireCracker>().FirstOrDefault(x => x.IsAnimating == false) is PlayerFireCracker bomb)
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

        private bool AnimatePlayerFireCracker(Construct bomb)
        {
            PlayerFireCracker PlayerFireCracker = bomb as PlayerFireCracker;

            DropShadow dropShadow = _scene_game.Children.OfType<DropShadow>().First(x => x.Id == bomb.Id);

            if (PlayerFireCracker.IsBlasting)
            {
                var speed = (_scene_game.Speed + bomb.SpeedOffset);

                bomb.SetLeft(bomb.GetLeft() + speed);
                bomb.SetTop(bomb.GetTop() + speed * bomb.IsometricDisplacement);

                bomb.Expand();
                bomb.Fade(0.02);

                // make the shadow fade with the bomb blast
                //dropShadow.Opacity = bomb.Opacity;

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
                        PlayerFireCracker.SetBlast();
                }
            }

            return true;
        }

        private bool RecyclePlayerFireCracker(Construct bomb)
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

        #region PlayerRocketSeeking

        private bool SpawnPlayerRocketSeekingsInScene()
        {
            for (int i = 0; i < 3; i++)
            {
                PlayerRocketSeeking bomb = new(
                    animateAction: AnimatePlayerRocketSeeking,
                    recycleAction: RecyclePlayerRocketSeeking,
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

        private bool GeneratePlayerRocketSeekingInScene()
        {
            // generate a seeking bomb if one is not in scene
            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !_scene_game.IsSlowMotionActivated &&
                _scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is Boss boss &&
                _scene_game.Children.OfType<PlayerRocketSeeking>().FirstOrDefault(x => x.IsAnimating == false) is PlayerRocketSeeking PlayerRocketSeeking)
            {
                _player.SetAttackStance();

                PlayerRocketSeeking.Reset();
                PlayerRocketSeeking.IsAnimating = true;
                PlayerRocketSeeking.SetPopping();

                PlayerRocketSeeking.Reposition(
                    player: _player,
                    downScaling: _scene_game.DownScaling);

                SyncDropShadow(PlayerRocketSeeking);

                if (_powerUp_health_bar.HasHealth && (PowerUpType)_powerUp_health_bar.Tag == PowerUpType.SEEKING_BALLS)
                    DepletePowerUp();

                // Console.WriteLine("Player Seeking Bomb dropped.");

                return true;
            }

            return false;
        }

        private bool AnimatePlayerRocketSeeking(Construct PlayerRocketSeeking)
        {
            PlayerRocketSeeking PlayerRocketSeeking1 = PlayerRocketSeeking as PlayerRocketSeeking;

            if (PlayerRocketSeeking1.IsBlasting)
            {
                var speed = _scene_game.Speed + PlayerRocketSeeking.SpeedOffset;

                MoveConstruct(construct: PlayerRocketSeeking1, speed: speed);

                PlayerRocketSeeking.Expand();
                PlayerRocketSeeking.Fade(0.02);

                //DropShadow dropShadow = _scene_game.Children.OfType<DropShadow>().First(x => x.Id == PlayerRocketSeeking.Id);
                //dropShadow.Opacity = PlayerRocketSeeking.Opacity;
            }
            else
            {
                PlayerRocketSeeking.Pop();
                PlayerRocketSeeking.Rotate(rotationSpeed: 3.5);

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    // if there a boss bomb seeking the player then target that first and if they hit both bombs blast

                    if (_scene_game.Children.OfType<BossRocketSeeking>().FirstOrDefault(x => x.IsAnimating) is BossRocketSeeking BossRocketSeeking)
                    {
                        PlayerRocketSeeking1.Seek(BossRocketSeeking.GetCloseHitBox());

                        if (PlayerRocketSeeking1.GetCloseHitBox().IntersectsWith(BossRocketSeeking.GetCloseHitBox()))
                        {
                            PlayerRocketSeeking1.SetBlast();
                            BossRocketSeeking.SetBlast();
                        }
                    }

                    // make the player bomb seek boss

                    if (_scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is Boss boss)
                    {
                        PlayerRocketSeeking1.Seek(boss.GetCloseHitBox());

                        if (PlayerRocketSeeking1.GetCloseHitBox().IntersectsWith(boss.GetCloseHitBox()))
                        {
                            PlayerRocketSeeking1.SetBlast();
                            LooseBossHealth(boss);
                        }
                    }

                    // make the player bomb seek enemy

                    if (_scene_game.Children.OfType<Enemy>().FirstOrDefault(x => x.IsAnimating) is Enemy enemy)
                    {
                        PlayerRocketSeeking1.Seek(enemy.GetCloseHitBox());

                        if (PlayerRocketSeeking1.GetCloseHitBox().IntersectsWith(enemy.GetCloseHitBox()))
                        {
                            PlayerRocketSeeking1.SetBlast();
                            LooseEnemyHealth(enemy);
                        }
                    }

                    if (PlayerRocketSeeking1.RunOutOfTimeToBlast())
                        PlayerRocketSeeking1.SetBlast();
                }
            }

            return true;
        }

        private bool RecyclePlayerRocketSeeking(Construct PlayerRocketSeeking)
        {
            var hitbox = PlayerRocketSeeking.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (PlayerRocketSeeking.IsFadingComplete || hitbox.Left > _scene_game.Width || hitbox.Right < 0 || hitbox.Top < 0 || hitbox.Bottom > _scene_game.Height)
            {
                PlayerRocketSeeking.IsAnimating = false;

                PlayerRocketSeeking.SetPosition(
                    left: -500,
                    top: -500);

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
            if (!BossExistsInScene() && _scene_game.Children.OfType<Vehicle>().FirstOrDefault(x => x.IsAnimating == false) is Vehicle vehicle)
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
            Vehicle vehicle1 = vehicle as Vehicle;

            vehicle.Pop();

            var speed = (_scene_game.Speed + vehicle.SpeedOffset);

            MoveConstruct(construct: vehicle, speed: speed);

            if (_scene_game.SceneState == SceneState.GAME_RUNNING)
            {
                if (vehicle1.Honk())
                    GenerateVehicleHonkInScene(vehicle1);
            }

            PreventVehicleOverlapping(vehicle);

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

        private void PreventVehicleOverlapping(Construct vehicle)
        {
            if (_scene_game.Children.OfType<Vehicle>().FirstOrDefault(x => x.IsAnimating && x.GetHorizontalHitBox().IntersectsWith(vehicle.GetHorizontalHitBox())) is Construct collidingVehicle)
            {
                var hitBox = vehicle.GetHitBox();

                if (hitBox.Top > 0 && hitBox.Left > 0 && vehicle.SpeedOffset == collidingVehicle.SpeedOffset)
                {
                    if (vehicle.SpeedOffset > -2)
                        vehicle.SpeedOffset--;
                }
                else
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
            }
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
                  top: roadMark.Height / 2,
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

                //tree.SetPosition(
                //    left: (_scene_game.Width / 2 - tree.Width) * _scene_game.DownScaling,
                //    top: (0 - tree.Width) * _scene_game.DownScaling,
                //    z: 2);

                tree.SetPosition(
                  left: _scene_game.Width / 2 - tree.Width * _scene_game.DownScaling,
                  top: tree.Height * -1,
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

                //tree.SetPosition(
                //    left: (-1 * tree.Width) * _scene_game.DownScaling,
                //    top: (_scene_game.Height / 2.5) * _scene_game.DownScaling,
                //    z: 4);

                tree.SetPosition(
                  left: -1 * tree.Width * _scene_game.DownScaling,
                  top: _scene_game.Height / 2 * _scene_game.DownScaling,
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

        private bool GenerateHonkInScene(Construct source)
        {
            if (_scene_game.Children.OfType<Honk>().FirstOrDefault(x => x.IsAnimating == false) is Honk honk)
            {
                honk.IsAnimating = true;
                honk.SetPopping();

                honk.Reset();

                var hitBox = source.GetCloseHitBox();

                honk.SetPosition(
                    left: hitBox.Left - source.Width / 2,
                    top: hitBox.Top - (25 * _scene_game.DownScaling),
                    z: 5);

                honk.SetRotation(_random.Next(-30, 30));
                honk.SetZ(source.GetZ() + 1);

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

        private bool GenerateVehicleHonkInScene(Vehicle source)
        {
            // if there are no bosses or enemies in the scene the vehicles will honk

            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !BossExistsInScene() && !EnemyExistsInScene())
            {
                return GenerateHonkInScene(source);
            }

            return true;
        }

        private bool GenerateEnemyHonkInScene(Enemy source)
        {
            // if there are no bosses in the scene the vehicles will honk

            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !BossExistsInScene())
            {
                return GenerateHonkInScene(source);
            }

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
            // if scene doesn't contain a boss then pick a random boss and add to scene

            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _boss_threashold.ShouldRelease(_game_score_bar.GetScore()) &&
                !_scene_game.Children.OfType<Boss>().Any(x => x.IsAnimating) &&
                _scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating == false) is Boss boss)
            {
                StopGameBackgroundSound();
                _ambience_sound_playing?.SetVolume(0.2);

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
            }
            else
            {
                boss1.Hover();
                boss.Pop();
                boss1.DepleteHitStance();

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
                PlayGameBackgroundSound();
                _ambience_sound_playing?.SetVolume(0.8);

                boss.IsAttacking = false;

                _player.SetWinStance();
                _game_score_bar.GainScore(5);

                GenerateInterimScreenInScene("Boss Busted");

                _scene_game.ActivateSlowMotion();
            }
        }

        private bool BossExistsInScene()
        {
            return _scene_game.Children.OfType<Boss>().Any(x => x.IsAnimating && x.IsAttacking);
        }

        #endregion

        #region Enemy

        private bool SpawnEnemysInScene()
        {
            for (int i = 0; i < 10; i++)
            {
                Enemy enemy = new(
                    animateAction: AnimateEnemy,
                    recycleAction: RecycleEnemy,
                    downScaling: _scene_game.DownScaling);

                _scene_game.AddToScene(enemy);

                enemy.SetPosition(
                    left: -500,
                    top: -500,
                    z: 8);

                SpawnDropShadowInScene(enemy);
            }

            return true;
        }

        private bool GenerateEnemyInScene()
        {
            if (!BossExistsInScene() &&
                _enemy_threashold.ShouldRelease(_game_score_bar.GetScore()) &&
                _scene_game.Children.OfType<Enemy>().FirstOrDefault(x => x.IsAnimating == false) is Enemy enemy)
            {
                enemy.IsAnimating = true;
                enemy.Reset();

                var topOrLeft = _random.Next(0, 2);

                switch (topOrLeft)
                {
                    case 0:
                        {
                            var xLaneWidth = _scene_game.Width / 2;

                            enemy.SetPosition(
                                left: _random.Next(0, (int)(xLaneWidth - enemy.Width)),
                                top: enemy.Height * -1);
                        }
                        break;
                    case 1:
                        {
                            var yLaneWidth = _scene_game.Height / 2;

                            enemy.SetPosition(
                                left: enemy.Width * -1,
                                top: _random.Next(0, (int)(yLaneWidth - enemy.Height)));
                        }
                        break;
                    default:
                        break;
                }

                SyncDropShadow(enemy);

                //Console.WriteLine("Enemy generated.");

                if (!_enemy_appeared)
                {
                    var sound = _enemy_entry_sounds[_random.Next(0, _enemy_entry_sounds.Length)];
                    sound.Play();

                    GenerateInterimScreenInScene("Beware of Aliens");
                    _scene_game.ActivateSlowMotion();
                    _enemy_appeared = true;
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

                MoveConstruct(construct: enemy, speed: speed);

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (enemy1.Honk())
                        GenerateEnemyHonkInScene(enemy1);

                    if (enemy1.Attack())
                        GenerateEnemyRocketInScene(enemy1);
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
                enemy.SetPosition(
                    left: -500,
                    top: -500);

                enemy.IsAnimating = false;

                //Console.WriteLine("Enemy Recycled");
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
                    _enemy_appeared = false;

                    GenerateInterimScreenInScene("Alien Fleet Busted");
                    _scene_game.ActivateSlowMotion();
                }

                Console.WriteLine("Enemy dead");
            }
        }

        private bool EnemyExistsInScene()
        {
            return _scene_game.Children.OfType<Enemy>().Any(x => x.IsAnimating);
        }

        #endregion

        #region EnemyRocket

        private bool SpawnEnemyRocketsInScene()
        {
            for (int i = 0; i < 10; i++)
            {
                EnemyRocket bomb = new(
                    animateAction: AnimateEnemyRocket,
                    recycleAction: RecycleEnemyRocket,
                    downScaling: _scene_game.DownScaling);

                bomb.SetPosition(
                    left: -500,
                    top: -500,
                    z: 8);

                _scene_game.AddToScene(bomb);

                SpawnDropShadowInScene(source: bomb);
            }

            return true;
        }

        private bool GenerateEnemyRocketInScene(Enemy source)
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<EnemyRocket>().FirstOrDefault(x => x.IsAnimating == false) is EnemyRocket EnemyRocket)
            {
                EnemyRocket.Reset();
                EnemyRocket.IsAnimating = true;
                EnemyRocket.SetPopping();

                EnemyRocket.Reposition(
                    Enemy: source,
                    downScaling: _scene_game.DownScaling);

                SyncDropShadow(EnemyRocket);

                // Console.WriteLine("Enemy Bomb dropped.");

                return true;
            }

            return false;
        }

        private bool AnimateEnemyRocket(Construct bomb)
        {
            EnemyRocket EnemyRocket = bomb as EnemyRocket;

            var speed = _scene_game.Speed + bomb.SpeedOffset;

            MoveConstruct(construct: EnemyRocket, speed: speed);

            if (EnemyRocket.IsBlasting)
            {
                bomb.Expand();
                bomb.Fade(0.02);

                //DropShadow dropShadow = _scene_game.Children.OfType<DropShadow>().First(x => x.Id == bomb.Id);
                //dropShadow.Opacity = bomb.Opacity;
            }
            else
            {
                bomb.Pop();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (EnemyRocket.GetCloseHitBox().IntersectsWith(_player.GetCloseHitBox()))
                    {
                        EnemyRocket.SetBlast();
                        LoosePlayerHealth();
                    }

                    if (EnemyRocket.AutoBlast())
                        EnemyRocket.SetBlast();
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
                    left: -500,
                    top: -500);

                return true;
            }

            return false;
        }

        #endregion

        #region BossRocket

        private bool SpawnBossRocketsInScene()
        {
            for (int i = 0; i < 5; i++)
            {
                BossRocket bomb = new(
                    animateAction: AnimateBossRocket,
                    recycleAction: RecycleBossRocket,
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

        private bool GenerateBossRocketInScene()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is Boss boss &&
                _scene_game.Children.OfType<BossRocket>().FirstOrDefault(x => x.IsAnimating == false) is BossRocket BossRocket)
            {
                BossRocket.Reset();
                BossRocket.IsAnimating = true;
                BossRocket.SetPopping();

                BossRocket.Reposition(
                    boss: boss,
                    downScaling: _scene_game.DownScaling);

                SyncDropShadow(BossRocket);

                #region [OBSOLETE] Back & Forth Movement

                //if (boss.AwaitMoveLeft || boss.AwaitMoveRight)
                //{
                //    // player is on the right side of the boss
                //    if (_player.GetLeft() > boss.GetRight())
                //    {
                //        BossRocket.AwaitMoveDown = true;
                //        BossRocket.SetRotation(33);
                //    }
                //    else
                //    {
                //        BossRocket.AwaitMoveUp = true;
                //        BossRocket.SetRotation(125);
                //    }
                //}
                //else if (boss.AwaitMoveUp || boss.AwaitMoveDown)
                //{
                //    // player is above the boss
                //    if (_player.GetBottom() < boss.GetTop())
                //    {
                //        BossRocket.AwaitMoveRight = true;
                //        BossRocket.SetRotation(-33);
                //    }
                //    else
                //    {
                //        BossRocket.AwaitMoveLeft = true;
                //        BossRocket.SetRotation(125);
                //    }
                //} 

                #endregion

                #region Target Based Movement

                // player is on the bottom right side of the boss
                if (_player.GetTop() > boss.GetTop() && _player.GetLeft() > boss.GetLeft())
                {
                    BossRocket.AwaitMoveDown = true;
                    BossRocket.SetRotation(33);
                }
                // player is on the bottom left side of the boss
                else if (_player.GetTop() > boss.GetTop() && _player.GetLeft() < boss.GetLeft())
                {
                    BossRocket.AwaitMoveLeft = true;
                    BossRocket.SetRotation(123);
                }
                // if player is on the top left side of the boss
                else if (_player.GetTop() < boss.GetTop() && _player.GetLeft() < boss.GetLeft())
                {
                    BossRocket.AwaitMoveUp = true;
                    BossRocket.SetRotation(123);
                }
                // if player is on the top right side of the boss
                else if (_player.GetTop() < boss.GetTop() && _player.GetLeft() > boss.GetLeft())
                {
                    BossRocket.AwaitMoveRight = true;
                    BossRocket.SetRotation(-33);
                }
                else
                {
                    BossRocket.AwaitMoveDown = true;
                    BossRocket.SetRotation(33);
                }

                #endregion

                // Console.WriteLine("Boss Bomb dropped.");

                return true;
            }

            return false;
        }

        private bool AnimateBossRocket(Construct bomb)
        {
            BossRocket BossRocket = bomb as BossRocket;

            var speed = (_scene_game.Speed + bomb.SpeedOffset) * _scene_game.DownScaling;

            if (BossRocket.AwaitMoveLeft)
            {
                BossRocket.MoveLeft(speed);
            }
            else if (BossRocket.AwaitMoveRight)
            {
                BossRocket.MoveRight(speed);
            }
            else if (BossRocket.AwaitMoveUp)
            {
                BossRocket.MoveUp(speed);
            }
            else if (BossRocket.AwaitMoveDown)
            {
                BossRocket.MoveDown(speed);
            }

            if (BossRocket.IsBlasting)
            {
                bomb.Expand();
                bomb.Fade(0.02);

                //DropShadow dropShadow = _scene_game.Children.OfType<DropShadow>().First(x => x.Id == bomb.Id);
                //dropShadow.Opacity = bomb.Opacity;
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

        #region BossRocketSeeking

        private bool SpawnBossRocketSeekingsInScene()
        {
            for (int i = 0; i < 2; i++)
            {
                BossRocketSeeking bomb = new(
                    animateAction: AnimateBossRocketSeeking,
                    recycleAction: RecycleBossRocketSeeking,
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

        private bool GenerateBossRocketSeekingInScene()
        {
            // generate a seeking bomb if one is not in scene
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is Boss boss &&
                !_scene_game.Children.OfType<BossRocketSeeking>().Any(x => x.IsAnimating) &&
                _scene_game.Children.OfType<BossRocketSeeking>().FirstOrDefault(x => x.IsAnimating == false) is BossRocketSeeking BossRocketSeeking)
            {
                BossRocketSeeking.Reset();
                BossRocketSeeking.IsAnimating = true;
                BossRocketSeeking.SetPopping();

                BossRocketSeeking.Reposition(
                    boss: boss,
                    downScaling: _scene_game.DownScaling);

                SyncDropShadow(BossRocketSeeking);

                // Console.WriteLine("Boss Seeking Bomb dropped.");

                return true;
            }

            return false;
        }

        private bool AnimateBossRocketSeeking(Construct BossRocketSeeking)
        {
            BossRocketSeeking BossRocketSeeking1 = BossRocketSeeking as BossRocketSeeking;

            var speed = (_scene_game.Speed + BossRocketSeeking.SpeedOffset) * _scene_game.DownScaling;

            if (BossRocketSeeking1.IsBlasting)
            {
                MoveConstruct(construct: BossRocketSeeking1, speed: speed);

                BossRocketSeeking.Expand();
                BossRocketSeeking.Fade(0.02);

                //DropShadow dropShadow = _scene_game.Children.OfType<DropShadow>().First(x => x.Id == BossRocketSeeking.Id);
                //dropShadow.Opacity = BossRocketSeeking.Opacity;
            }
            else
            {
                BossRocketSeeking.Pop();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (_scene_game.Children.OfType<Boss>().Any(x => x.IsAnimating && x.IsAttacking))
                    {
                        BossRocketSeeking1.Seek(_player.GetCloseHitBox());

                        if (BossRocketSeeking1.GetCloseHitBox().IntersectsWith(_player.GetCloseHitBox()))
                        {
                            BossRocketSeeking1.SetBlast();

                            LoosePlayerHealth();
                        }
                        else
                        {
                            if (BossRocketSeeking1.RunOutOfTimeToBlast())
                                BossRocketSeeking1.SetBlast();
                        }
                    }
                    else
                    {
                        BossRocketSeeking1.SetBlast();
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
                //dropShadow.Opacity = 1;
                dropShadow.ParentConstructSpeed = _scene_game.Speed + source.SpeedOffset;
                dropShadow.IsAnimating = true;

                dropShadow.SetZ(source.GetZ() - 2);

                dropShadow.Reset();
            }
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
            if (_scene_game.SceneState == SceneState.GAME_RUNNING && HealthPickup.ShouldGenerate(_player.Health) &&
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
            if (_scene_game.SceneState == SceneState.GAME_RUNNING)
            {
                if ((BossExistsInScene() || EnemyExistsInScene()) && !_powerUp_health_bar.HasHealth) // if a boss or enemy exists and currently player has no other power up
                {
                    if (_scene_game.Children.OfType<PowerUpPickup>().FirstOrDefault(x => x.IsAnimating == false) is PowerUpPickup powerUpPickup)
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
                MoveConstruct(construct: powerUpPickup, speed: speed);

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
                powerUpPickup.SetPosition(
                    left: -500,
                    top: -500);

                powerUpPickup.IsAnimating = false;
            }

            return true;
        }

        #endregion

        #region Construct

        private void MoveConstruct(Construct construct, double speed)
        {
            speed *= _scene_game.DownScaling;

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

            AddGeneratorsToScene();

            _scene_game.Speed = 5;
            _scene_game.Play();

            _scene_main_menu.Speed = 5;
            _scene_main_menu.Play();
        }

        private void AddGeneratorsToScene()
        {
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

            _scene_game.AddToScene(

                // then add road marks
                new Generator(
                    generationDelay: 30,
                    generationAction: GenerateRoadMarkInScene,
                    startUpAction: SpawnRoadMarksInScene),

                // then add the top trees
                new Generator(
                    generationDelay: 30,
                    generationAction: GenerateTreeInSceneTop,
                    startUpAction: SpawnTreesInScene),

                // then add the vehicles which will appear forward in z wrt the top trees
                new Generator(
                    generationDelay: 80,
                    generationAction: GenerateVehicleInScene,
                    startUpAction: SpawnVehiclesInScene),

                // then add the bottom trees which will appear forward in z wrt to the vehicles
                new Generator(
                    generationDelay: 30,
                    generationAction: GenerateTreeInSceneBottom,
                    startUpAction: SpawnTreesInScene),

                // add the honks which will appear forward in z wrt to everything on the road
                new Generator(
                    generationDelay: 0,
                    generationAction: () => { return true; },
                    startUpAction: SpawnHonksInScene),

                // add the player in scene which will appear forward in z wrt to all else
                new Generator(
                    generationDelay: 0,
                    generationAction: () => { return true; },
                    startUpAction: SpawnPlayerInScene),

                new Generator(
                    generationDelay: 0,
                    generationAction: () => { return true; },
                    startUpAction: SpawnPlayerRocketsInScene),

                new Generator(
                    generationDelay: 0,
                    generationAction: () => { return true; },
                    startUpAction: SpawnPlayerFireCrackersInScene),

                // add the clouds which are above the player z
                new Generator(
                    generationDelay: 100,
                    generationAction: GenerateCloudInScene,
                    startUpAction: SpawnCloudsInScene),

                new Generator(
                    generationDelay: 100,
                    generationAction: GenerateBossInScene,
                    startUpAction: SpawnBossesInScene),

                new Generator(
                    generationDelay: 50,
                    generationAction: GenerateBossRocketInScene,
                    startUpAction: SpawnBossRocketsInScene,
                    randomizeGenerationDelay: true),

                new Generator(
                    generationDelay: 200,
                    generationAction: GenerateBossRocketSeekingInScene,
                    startUpAction: SpawnBossRocketSeekingsInScene,
                    randomizeGenerationDelay: true),

                new Generator(
                    generationDelay: 0,
                    generationAction: () => { return true; },
                    startUpAction: SpawnPlayerRocketSeekingsInScene),

                new Generator(
                    generationDelay: 600,
                    generationAction: GenerateHealthPickupsInScene,
                    startUpAction: SpawnHealthPickupsInScene,
                    randomizeGenerationDelay: true),

                new Generator(
                    generationDelay: 600,
                    generationAction: GeneratePowerUpPickupsInScene,
                    startUpAction: SpawnPowerUpPickupsInScene,
                    randomizeGenerationDelay: true),

                new Generator(
                    generationDelay: 0,
                    generationAction: () => { return true; },
                    startUpAction: SpawnInterimScreenInScene),

                new Generator(
                    generationDelay: 180,
                    generationAction: GenerateEnemyInScene,
                    startUpAction: SpawnEnemysInScene,
                    randomizeGenerationDelay: true),

                 new Generator(
                    generationDelay: 0,
                    generationAction: () => { return true; },
                    startUpAction: SpawnEnemyRocketsInScene)
                );

            _scene_main_menu.AddToScene(new Generator(
                generationDelay: 0,
                generationAction: () => { return true; },
                startUpAction: SpawnTitleScreenInScene));
        }

        #endregion

        #region Sound

        private void PlaySoundLoops()
        {
            _ambience_sound_playing?.Stop();
            _ambience_sound_playing = _ambience_sounds[_random.Next(0, _ambience_sounds.Length)];
            _ambience_sound_playing.Play();

            //_game_background_music_sound_playing?.Stop();
            //_game_background_music_sound_playing = _game_background_music_sounds[_random.Next(0, _game_background_music_sounds.Length)];
            //_game_background_music_sound_playing.Play();
        }

        private void PauseSoundLoops()
        {
            _ambience_sound_playing?.Pause();
            //_game_background_music_sound_playing?.Pause();
        }

        private void ResumeSoundLoops()
        {
            _ambience_sound_playing?.Resume();
            //_game_background_music_sound_playing?.Resume();
        }

        private void StopSoundLoops()
        {
            _ambience_sound_playing?.Stop();
            //_game_background_music_sound_playing?.Stop();
        }

        private void PlayGameStartSound()
        {
            _game_start_sound_playing = _game_start_sounds[_random.Next(0, _game_start_sounds.Length)];
            _game_start_sound_playing.Play();
        }

        private void PlayGamePauseSound()
        {
            _game_pause_sound_playing = _game_pause_sounds[_random.Next(0, _game_pause_sounds.Length)];
            _game_pause_sound_playing.Play();
        }

        private void PlayGameOverSound()
        {
            _game_over_sound_playing = _game_over_sounds[_random.Next(0, _game_over_sounds.Length)];
            _game_over_sound_playing.Play();
        }

        private void PlayGameBackgroundSound()
        {
            //_game_background_music_sound_playing?.Stop();
            //_game_background_music_sound_playing = _game_background_music_sounds[_random.Next(0, _game_background_music_sounds.Length)];
            //_game_background_music_sound_playing.Play();
        }

        private void StopGameBackgroundSound()
        {
            _game_background_music_sound_playing?.Stop();
        }

        #endregion

        #endregion

        #region Events

        private void HonkBomberPage_Loaded(object sender, RoutedEventArgs e)
        {
            _scene_game.Width = 1920;
            _scene_game.Height = 1080;

            _scene_main_menu.Width = 1920;
            _scene_main_menu.Height = 1080;

            SetController();
            SetScene();
            GenerateTitleScreenInScene("Honk Trooper");

            SizeChanged += HonkBomberPage_SizeChanged;

            ScreenExtensions.DisplayInformation.OrientationChanged += DisplayInformation_OrientationChanged;
            ScreenExtensions.RequiredDisplayOrientation = DisplayOrientations.Landscape;

            // set display orientation to required orientation
            if (ScreenExtensions.GetDisplayOrienation() != ScreenExtensions.RequiredDisplayOrientation)
                ScreenExtensions.SetDisplayOrientation(ScreenExtensions.RequiredDisplayOrientation);

            ScreenExtensions.EnterFullScreen(true);

            PlayGameBackgroundSound();
        }

        private void HonkBomberPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= HonkBomberPage_SizeChanged;
            ScreenExtensions.DisplayInformation.OrientationChanged -= DisplayInformation_OrientationChanged;
            _game_controller.RequiresScreenOrientationChange -= Controller_RequiresScreenOrientationChange;
            _game_controller.PauseButton.Click -= PauseButton_Click;
        }

        private void HonkBomberPage_SizeChanged(object sender, SizeChangedEventArgs args)
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
