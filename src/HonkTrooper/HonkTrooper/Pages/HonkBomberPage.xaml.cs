using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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

        private readonly HealthBar _ufo_boss_health_bar;
        private readonly HealthBar _vehicle_boss_health_bar;
        private readonly HealthBar _zombie_boss_health_bar;

        private readonly HealthBar _powerUp_health_bar;
        private readonly HealthBar _sound_pollution_health_bar;

        private readonly ScoreBar _game_score_bar;
        private readonly StackPanel _health_bars;

        private readonly Threashold _ufo_boss_threashold;
        private readonly Threashold _vehicle_boss_threashold;
        private readonly Threashold _zombie_boss_threashold;
        private readonly Threashold _enemy_threashold;

        private readonly double _sound_pollution_max_limit = 6; // max 3 vehicles or ufos honking to trigger sound pollution limit

        //TODO: set defaults _vehicle_boss_threashold_limit = 25
        private readonly double _vehicle_boss_threashold_limit = 25; // first vehicle Boss will appear
        private readonly double _vehicle_boss_threashold_limit_increase = 15;

        //TODO: set defaults _ufo_boss_threashold_limit = 50
        private readonly double _ufo_boss_threashold_limit = 50; // first UfoBoss will appear
        private readonly double _ufo_boss_threashold_limit_increase = 15;

        //TODO: set defaults _zombie_boss_threashold_limit = 75
        private readonly double _zombie_boss_threashold_limit = 10; // first UfoBoss will appear
        private readonly double _zombie_boss_threashold_limit_increase = 15;

        //TODO: set defaults _enemy_threashold_limit = 80
        private readonly double _enemy_threashold_limit = 100; // after first enemies will appear
        private readonly double _enemy_threashold_limit_increase = 15;

        private double _enemy_kill_count;
        private readonly double _enemy_kill_count_limit = 20;

        private bool _enemy_fleet_appeared;

        private int _selected_player_template;

        private readonly AudioStub _audio_stub;

        #endregion

        #region Ctor

        public HonkBomberPage()
        {
            this.InitializeComponent();

            _scene_game = this.GameScene;
            _scene_main_menu = this.MainMenuScene;

            _player_health_bar = this.PlayerHealthBar;

            _ufo_boss_health_bar = this.UfoBossHealthBar;
            _zombie_boss_health_bar = this.ZombieBossHealthBar;
            _vehicle_boss_health_bar = this.VehicleBossHealthBar;

            _powerUp_health_bar = this.PowerUpHealthBar;
            _sound_pollution_health_bar = this.SoundPollutionBar;

            _game_controller = this.GameController;

            _game_score_bar = this.GameScoreBar;
            _health_bars = this.HealthBars;

            _ufo_boss_threashold = new Threashold(_ufo_boss_threashold_limit);
            _zombie_boss_threashold = new Threashold(_zombie_boss_threashold_limit);
            _vehicle_boss_threashold = new Threashold(_vehicle_boss_threashold_limit);
            _enemy_threashold = new Threashold(_enemy_threashold_limit);

            ToggleHudVisibility(Visibility.Collapsed);

            _random = new Random();

            _audio_stub = new AudioStub(
                (SoundType.GAME_BACKGROUND_MUSIC, 0.5, true),
                (SoundType.UFO_BOSS_BACKGROUND_MUSIC, 0.5, true),
                (SoundType.AMBIENCE, 0.6, true),
                (SoundType.GAME_START, 1, false),
                (SoundType.GAME_PAUSE, 1, false),
                (SoundType.GAME_OVER, 1, false),
                (SoundType.UFO_ENEMY_ENTRY, 1, false));

            ScreenExtensions.Width = Constants.DEFAULT_SCENE_WIDTH;
            ScreenExtensions.Height = Constants.DEFAULT_SCENE_HEIGHT;

            _scene_main_menu.SetRenderTransformOrigin(0.5);
            SetScreenScaling();

            Loaded += HonkBomberPage_Loaded;
            Unloaded += HonkBomberPage_Unloaded;
        }

        #endregion

        #region Events

        private async void HonkBomberPage_Loaded(object sender, RoutedEventArgs e)
        {
            ScreenExtensions.DisplayInformation.OrientationChanged += DisplayInformation_OrientationChanged;
            ScreenExtensions.RequiredScreenOrientation = DisplayOrientations.Landscape;

            // set display orientation to required orientation
            if (ScreenExtensions.GetScreenOrienation() != ScreenExtensions.RequiredScreenOrientation)
                ScreenExtensions.SetScreenOrientation(ScreenExtensions.RequiredScreenOrientation);

            SetController();
            SetScene();

            SizeChanged += HonkBomberPage_SizeChanged;

            if (ScreenExtensions.GetScreenOrienation() == ScreenExtensions.RequiredScreenOrientation)
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
            ScreenExtensions.Width = args.NewSize.Width <= Constants.DEFAULT_SCENE_WIDTH ? args.NewSize.Width : Constants.DEFAULT_SCENE_WIDTH;
            ScreenExtensions.Height = args.NewSize.Height <= Constants.DEFAULT_SCENE_HEIGHT ? args.NewSize.Height : Constants.DEFAULT_SCENE_HEIGHT;

            SetScreenScaling();

            if (_scene_game.SceneState == SceneState.GAME_RUNNING)
            {
                _player.Reposition();
                GenerateDropShadow(_player);
            }

            RepositionHoveringTitleScreens();
            LoggerExtensions.Log($"{ScreenExtensions.Width} x {ScreenExtensions.Height}");
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
                ScreenExtensions.EnterFullScreen(true);

                if (ScreenExtensions.GetScreenOrienation() == ScreenExtensions.RequiredScreenOrientation)
                {
                    if (_scene_main_menu.Children.OfType<DisplayOrientationChangeScreen>().FirstOrDefault(x => x.IsAnimating) is DisplayOrientationChangeScreen DisplayOrientationChangeScreen)
                    {
                        RecycleDisplayOrientationChangeScreen(DisplayOrientationChangeScreen);

                        _audio_stub.Play(SoundType.GAME_BACKGROUND_MUSIC);
                        GenerateTitleScreen("Honk Trooper");

                        _scene_game.Play();
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
            }

            LoggerExtensions.Log($"{sender.CurrentOrientation}");
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            PauseGame();
        }

        #endregion

        #region Methods

        #region Game

        private bool PauseGame()
        {
            _audio_stub.Play(SoundType.GAME_PAUSE);

            _audio_stub.Pause(SoundType.AMBIENCE);

            if (UfoBossExists())
            {
                //_audio_stub.Pause(SoundType.UFO_BOSS_BACKGROUND_MUSIC);
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

            if (UfoBossExists())
            {
                //_audio_stub.Resume(SoundType.UFO_BOSS_BACKGROUND_MUSIC);
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

            _game_score_bar.Reset();
            _powerUp_health_bar.Reset();

            _ufo_boss_health_bar.Reset();
            _vehicle_boss_health_bar.Reset();
            _zombie_boss_health_bar.Reset();

            _sound_pollution_health_bar.Reset();
            _sound_pollution_health_bar.SetMaxiumHealth(_sound_pollution_max_limit);
            _sound_pollution_health_bar.SetIcon(Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.HONK).Uri);
            _sound_pollution_health_bar.SetBarColor(color: Colors.Purple);

            _ufo_boss_threashold.Reset(_ufo_boss_threashold_limit);
            _vehicle_boss_threashold.Reset(_vehicle_boss_threashold_limit);
            _zombie_boss_threashold.Reset(_zombie_boss_threashold_limit);

            _enemy_threashold.Reset(_enemy_threashold_limit);
            _enemy_kill_count = 0;
            _enemy_fleet_appeared = false;

            GeneratePlayerBalloon();
            RecycleLogicalConstructs();

            _scene_game.SceneState = SceneState.GAME_RUNNING;
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
                _audio_stub.Stop(SoundType.AMBIENCE, SoundType.GAME_BACKGROUND_MUSIC, SoundType.UFO_BOSS_BACKGROUND_MUSIC);

                if (_scene_game.Children.OfType<UfoBoss>().FirstOrDefault(x => x.IsAnimating) is UfoBoss ufoBoss)
                {
                    ufoBoss.SetWinStance();
                    ufoBoss.StopSoundLoop();
                }

                _audio_stub.Play(SoundType.GAME_OVER);

                _scene_main_menu.Play();
                _scene_game.SceneState = SceneState.GAME_STOPPED;

                ToggleHudVisibility(Visibility.Collapsed);
                GenerateTitleScreen("Game Over");

                _game_controller.DeactivateGyrometerReading();
            }
        }

        private void RecycleLogicalConstructs()
        {
            foreach (var construct in _scene_game.Children.OfType<Construct>()
                .Where(x => x.ConstructType is
                ConstructType.VEHICLE_ENEMY_LARGE or
                ConstructType.VEHICLE_ENEMY_SMALL or
                ConstructType.VEHICLE_BOSS or
                ConstructType.UFO_BOSS or
                ConstructType.HONK or
                ConstructType.PLAYER_ROCKET or
                ConstructType.PLAYER_ROCKET_SEEKING or
                ConstructType.PLAYER_FIRE_CRACKER or
                ConstructType.UFO_BOSS_ROCKET or
                ConstructType.UFO_BOSS_ROCKET_SEEKING or
                ConstructType.UFO_ENEMY or
                ConstructType.UFO_ENEMY_ROCKET or
                ConstructType.VEHICLE_BOSS_ROCKET or
                ConstructType.POWERUP_PICKUP or
                ConstructType.HEALTH_PICKUP))
            {
                construct.IsAnimating = false;

                construct.SetPosition(
                     left: -3000,
                     top: -3000);

                if (construct is UfoBoss ufoBoss)
                {
                    ufoBoss.IsAttacking = false;
                    ufoBoss.Health = 0;
                }

                if (construct is VehicleBoss vehicleboss)
                {
                    vehicleboss.IsAttacking = false;
                    vehicleboss.Health = 0;
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
                        if (ScreenExtensions.RequiredScreenOrientation == ScreenExtensions.GetScreenOrienation())
                        {
                            RecycleTitleScreen(titleScreen);
                            GeneratePlayerSelectionScreen();
                            ScreenExtensions.EnterFullScreen(true);
                        }
                        else
                        {
                            ScreenExtensions.SetScreenOrientation(ScreenExtensions.RequiredScreenOrientation);
                        }
                    }
                    else
                    {
                        if (!_scene_game.IsAnimating)
                        {
                            if (ScreenExtensions.RequiredScreenOrientation == ScreenExtensions.GetScreenOrienation())
                            {
                                ResumeGame();
                                RecycleTitleScreen(titleScreen);
                            }
                            else
                            {
                                ScreenExtensions.SetScreenOrientation(ScreenExtensions.RequiredScreenOrientation);
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

            _scene_main_menu.AddToScene(interimScreen);

            return true;
        }

        private bool GenerateInterimScreen(string title)
        {
            if (_scene_main_menu.Children.OfType<InterimScreen>().FirstOrDefault(x => x.IsAnimating == false) is InterimScreen interimScreen)
            {
                interimScreen.IsAnimating = true;
                interimScreen.SetTitle(title);
                interimScreen.Reposition();
                interimScreen.Reset();

                _scene_main_menu.Play();

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

                _scene_main_menu.Pause();

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

            SpawnDropShadow(source: _player);

            _scene_game.AddToScene(_player);

            return true;
        }

        private bool GeneratePlayerBalloon()
        {
            _player.IsAnimating = true;
            _player.Reset();
            _player.Reposition();
            _player.SetPlayerTemplate(_selected_player_template);

            switch (_selected_player_template)
            {
                case 1:
                    {
                        _game_controller.SetAttackButtonColor(App.Current.Resources["Player1AccentColor"] as SolidColorBrush);
                        _game_controller.SetThumbstickThumbColor(App.Current.Resources["Player1AccentColor"] as SolidColorBrush);
                    }
                    break;
                case 2:
                    {
                        _game_controller.SetAttackButtonColor(App.Current.Resources["Player2AccentColor"] as SolidColorBrush);
                        _game_controller.SetThumbstickThumbColor(App.Current.Resources["Player2AccentColor"] as SolidColorBrush);
                    }
                    break;
                default:
                    break;
            }

            GenerateDropShadow(source: _player);
            SetPlayerHealthBar();

            return true;
        }

        private void SetPlayerHealthBar()
        {
            _player_health_bar.SetMaxiumHealth(_player.Health);
            _player_health_bar.SetValue(_player.Health);

            _player_health_bar.SetIcon(Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.HEALTH_PICKUP).Uri);
            _player_health_bar.SetBarColor(color: Colors.Crimson);
        }

        private bool AnimatePlayerBalloon(Construct player)
        {
            _player.Pop();
            _player.Hover();
            _player.DepleteAttackStance();
            _player.DepleteWinStance();
            _player.DepleteHitStance();

            if (_scene_game.SceneState == SceneState.GAME_RUNNING)
            {
                var count = _scene_game.Children.OfType<VehicleEnemy>().Count(x => x.IsAnimating && x.WillHonk) + _scene_game.Children.OfType<UfoEnemy>().Count(x => x.IsAnimating && x.WillHonk);
                _sound_pollution_health_bar.SetValue(count * 2);

                if (_sound_pollution_health_bar.GetValue() >= _sound_pollution_health_bar.GetMaxiumHealth()) // loose score slowly if sound pollution has reached the limit
                {
                    _game_score_bar.LooseScore(0.01);
                }

                var scaling = ScreenExtensions.GetScreenSpaceScaling();
                var speed = _player.GetMovementSpeed();

                _player.Move(
                    speed: speed,
                    sceneWidth: Constants.DEFAULT_SCENE_WIDTH * scaling,
                    sceneHeight: Constants.DEFAULT_SCENE_HEIGHT * scaling,
                    controller: _game_controller);

                ProcessPlayerAttack();
            }

            return true;
        }

        private void ProcessPlayerAttack()
        {
            if (_game_controller.IsAttacking)
            {
                if (UfoEnemyExists() || UfoBossExists() || ZombieBossExists())
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

                if (_scene_game.Children.OfType<UfoBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is UfoBoss UfoBoss)
                    UfoBoss.SetWinStance();

                GameOver();
            }
        }

        #endregion

        #region PlayerFireCracker

        private bool SpawnPlayerFireCrackers()
        {
            for (int i = 0; i < 3; i++)
            {
                PlayerFireCracker playerFireCracker = new(
                    animateAction: AnimatePlayerFireCracker,
                    recycleAction: RecyclePlayerFireCracker);

                playerFireCracker.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 7);

                _scene_game.AddToScene(playerFireCracker);

                SpawnDropShadow(source: playerFireCracker);
            }

            return true;
        }

        private bool GeneratePlayerFireCracker()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !_scene_game.IsSlowMotionActivated)
            {
                if ((VehicleBossExists() || _scene_game.Children.OfType<VehicleEnemy>().Any(x => x.IsAnimating)) &&
                    _scene_game.Children.OfType<PlayerFireCracker>().FirstOrDefault(x => x.IsAnimating == false) is PlayerFireCracker playerFireCracker)
                {
                    _player.SetAttackStance();

                    playerFireCracker.Reset();
                    playerFireCracker.IsAnimating = true;
                    playerFireCracker.IsGravitatingDownwards = true;
                    playerFireCracker.SetPopping();

                    playerFireCracker.SetRotation(_random.Next(-30, 30));

                    playerFireCracker.Reposition(
                        player: _player);

                    GenerateDropShadow(source: playerFireCracker);

                    LoggerExtensions.Log("Player firecracker dropped.");

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

            var speed = playerFireCracker1.GetMovementSpeed();

            if (playerFireCracker1.IsBlasting)
            {
                playerFireCracker.Expand();
                playerFireCracker.Fade(0.02);
                playerFireCracker1.MoveDownRight(speed);
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
                    var fireCrackerHitBox = playerFireCracker.GetCloseHitBox();

                    if (drpShdwHitBox.IntersectsWith(fireCrackerHitBox) && playerFireCracker.GetBottom() > dropShadow.GetBottom())  // start blast animation when the bomb touches it's shadow
                    {
                        if (_scene_game.Children.OfType<VehicleEnemy>()
                            .Where(x => x.IsAnimating && x.WillHonk)
                            .FirstOrDefault(x => x.GetCloseHitBox().IntersectsWith(fireCrackerHitBox)) is VehicleEnemy vehicle) // while in blast check if it intersects with any vehicle, if it does then the vehicle stops honking and slows down
                        {
                            vehicle.SetBlast();
                            _game_score_bar.GainScore(3);
                        }

                        if (_scene_game.Children.OfType<VehicleBoss>()
                            .FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is VehicleBoss vehicleBoss && vehicleBoss.GetCloseHitBox().IntersectsWith(fireCrackerHitBox)) // if a vechile boss is in place then boss looses health
                        {
                            LooseVehicleBossHealth(vehicleBoss);
                        }

                        playerFireCracker1.SetBlast();

                        dropShadow.IsAnimating = false;
                        dropShadow.SetPosition(-3000, -3000);
                    }
                }
            }

            return true;
        }

        private bool RecyclePlayerFireCracker(Construct playerFireCracker)
        {
            if (playerFireCracker.IsFadingComplete)
            {
                playerFireCracker.IsAnimating = false;
                playerFireCracker.IsGravitatingDownwards = false;

                playerFireCracker.SetPosition(
                    left: -3000,
                    top: -3000);

                return true;
            }

            return false;
        }

        #endregion

        #region PlayerRocket

        private bool SpawnPlayerRockets()
        {
            for (int i = 0; i < 4; i++)
            {
                PlayerRocket playerRocket = new(
                    animateAction: AnimatePlayerRocket,
                    recycleAction: RecyclePlayerRocket);

                playerRocket.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 7);

                _scene_game.AddToScene(playerRocket);

                SpawnDropShadow(source: playerRocket);
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
                playerRocket.Reposition(Player: _player);

                GenerateDropShadow(source: playerRocket);

                var playerDistantHitBox = _player.GetDistantHitBox();

                // get closest possible target
                UfoBossRocketSeeking ufoBossRocketSeeking = _scene_game.Children.OfType<UfoBossRocketSeeking>()?.FirstOrDefault(x => x.IsAnimating && !x.IsBlasting && x.GetHitBox().IntersectsWith(playerDistantHitBox));
                UfoBoss ufoBoss = _scene_game.Children.OfType<UfoBoss>()?.FirstOrDefault(x => x.IsAnimating && x.IsAttacking && x.GetHitBox().IntersectsWith(playerDistantHitBox));
                ZombieBoss zombieBoss = _scene_game.Children.OfType<ZombieBoss>()?.FirstOrDefault(x => x.IsAnimating && x.IsAttacking && x.GetHitBox().IntersectsWith(playerDistantHitBox));

                UfoEnemy ufoEnemy = _scene_game.Children.OfType<UfoEnemy>()?.FirstOrDefault(x => x.IsAnimating && !x.IsFadingComplete && x.GetHitBox().IntersectsWith(playerDistantHitBox));

                // if not found then find random target
                ufoBossRocketSeeking ??= _scene_game.Children.OfType<UfoBossRocketSeeking>().FirstOrDefault(x => x.IsAnimating && !x.IsBlasting);
                ufoBoss ??= _scene_game.Children.OfType<UfoBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking);
                zombieBoss ??= _scene_game.Children.OfType<ZombieBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking);
                ufoEnemy ??= _scene_game.Children.OfType<UfoEnemy>().FirstOrDefault(x => x.IsAnimating && !x.IsFadingComplete);

                LoggerExtensions.Log("Player Bomb dropped.");

                if (ufoEnemy is not null)
                {
                    SetPlayerRocketDirection(source: _player, rocket: playerRocket, rocketTarget: ufoEnemy);
                }
                else if (ufoBoss is not null)
                {
                    SetPlayerRocketDirection(source: _player, rocket: playerRocket, rocketTarget: ufoBoss);
                }
                else if (ufoBossRocketSeeking is not null)
                {
                    SetPlayerRocketDirection(source: _player, rocket: playerRocket, rocketTarget: ufoBossRocketSeeking);
                }
                else if (zombieBoss is not null)
                {
                    SetPlayerRocketDirection(source: _player, rocket: playerRocket, rocketTarget: zombieBoss);
                }

                return true;
            }

            return false;
        }

        private bool AnimatePlayerRocket(Construct playerRocket)
        {
            PlayerRocket playerRocket1 = playerRocket as PlayerRocket;

            var hitBox = playerRocket.GetCloseHitBox();

            var speed = playerRocket1.GetMovementSpeed();

            if (playerRocket1.AwaitMoveDownLeft)
            {
                playerRocket1.MoveDownLeft(speed);
            }
            else if (playerRocket1.AwaitMoveUpRight)
            {
                playerRocket1.MoveUpRight(speed);
            }
            else if (playerRocket1.AwaitMoveUpLeft)
            {
                playerRocket1.MoveUpLeft(speed);
            }
            else if (playerRocket1.AwaitMoveDownRight)
            {
                playerRocket1.MoveDownRight(speed);
            }

            if (playerRocket1.IsBlasting)
            {
                playerRocket.Expand();
                playerRocket.Fade(0.02);
            }
            else
            {
                playerRocket.Pop();
                playerRocket1.Hover();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    // if player bomb touches UfoBoss, it blasts, UfoBoss looses health
                    if (_scene_game.Children.OfType<UfoBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking && x.GetCloseHitBox().IntersectsWith(hitBox)) is UfoBoss ufoBoss)
                    {
                        playerRocket1.SetBlast();
                        LooseUfoBossHealth(ufoBoss);
                    }

                    // if player bomb touches ZombieBoss, it blasts, ZombieBoss looses health
                    if (_scene_game.Children.OfType<ZombieBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking && x.GetCloseHitBox().IntersectsWith(hitBox)) is ZombieBoss zombieBoss)
                    {
                        playerRocket1.SetBlast();
                        LooseZombieBossHealth(zombieBoss);
                    }

                    // if player bomb touches UfoBoss's seeking bomb, it blasts
                    if (_scene_game.Children.OfType<UfoBossRocketSeeking>().FirstOrDefault(x => x.IsAnimating && !x.IsBlasting && x.GetCloseHitBox().IntersectsWith(hitBox)) is UfoBossRocketSeeking ufoBossRocketSeeking)
                    {
                        playerRocket1.SetBlast();
                        ufoBossRocketSeeking.SetBlast();
                    }

                    // if player bomb touches ZombieBoss's seeking bomb, it blasts
                    if (_scene_game.Children.OfType<ZombieBossRocket>().FirstOrDefault(x => x.IsAnimating && !x.IsBlasting && x.GetCloseHitBox().IntersectsWith(hitBox)) is ZombieBossRocket zombieBossRocket)
                    {
                        playerRocket1.SetBlast();
                        zombieBossRocket.SetBlast();
                    }

                    // if player bomb touches enemy, it blasts, enemy looses health
                    if (_scene_game.Children.OfType<UfoEnemy>().FirstOrDefault(x => x.IsAnimating && !x.IsDead && x.GetCloseHitBox().IntersectsWith(hitBox)) is UfoEnemy ufoEnemy)
                    {
                        playerRocket1.SetBlast();
                        LooseUfoEnemyHealth(ufoEnemy);
                    }

                    if (playerRocket1.AutoBlast())
                        playerRocket1.SetBlast();
                }
            }

            return true;
        }

        private bool RecyclePlayerRocket(Construct playerRocket)
        {
            var hitbox = playerRocket.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (playerRocket.IsFadingComplete || hitbox.Left > Constants.DEFAULT_SCENE_WIDTH || hitbox.Right < 0 /*|| hitbox.Top < 0 || hitbox.Top > Constants.DEFAULT_SCENE_HEIGHT*/)
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

        #region PlayerRocketSeeking

        private bool SpawnPlayerRocketSeekings()
        {
            for (int i = 0; i < 3; i++)
            {
                PlayerRocketSeeking playerRocketSeeking = new(
                    animateAction: AnimatePlayerRocketSeeking,
                    recycleAction: RecyclePlayerRocketSeeking);

                playerRocketSeeking.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 7);

                _scene_game.AddToScene(playerRocketSeeking);

                SpawnDropShadow(source: playerRocketSeeking);
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
                playerRocketSeeking.Reposition(player: _player);

                GenerateDropShadow(source: playerRocketSeeking);

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
                var speed = playerRocketSeeking1.GetMovementSpeed();
                playerRocketSeeking1.MoveDownRight(speed);
                playerRocketSeeking.Expand();
                playerRocketSeeking.Fade(0.02);
            }
            else
            {
                playerRocketSeeking.Pop();
                playerRocketSeeking.Rotate(rotationSpeed: 3.5);

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (_scene_game.Children.OfType<UfoBossRocketSeeking>().FirstOrDefault(x => x.IsAnimating && !x.IsBlasting) is UfoBossRocketSeeking UfoBossRocketSeeking) // target UfoBoss bomb seeking
                    {
                        playerRocketSeeking1.Seek(UfoBossRocketSeeking.GetCloseHitBox());

                        if (playerRocketSeeking1.GetCloseHitBox().IntersectsWith(UfoBossRocketSeeking.GetCloseHitBox()))
                        {
                            playerRocketSeeking1.SetBlast();
                            UfoBossRocketSeeking.SetBlast();
                        }
                    }
                    else if (_scene_game.Children.OfType<UfoBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is UfoBoss UfoBoss) // target UfoBoss
                    {
                        playerRocketSeeking1.Seek(UfoBoss.GetCloseHitBox());

                        if (playerRocketSeeking1.GetCloseHitBox().IntersectsWith(UfoBoss.GetCloseHitBox()))
                        {
                            playerRocketSeeking1.SetBlast();
                            LooseUfoBossHealth(UfoBoss);
                        }
                    }
                    else if (_scene_game.Children.OfType<UfoEnemy>().FirstOrDefault(x => x.IsAnimating && !x.IsFadingComplete) is UfoEnemy enemy) // target enemy
                    {
                        playerRocketSeeking1.Seek(enemy.GetCloseHitBox());

                        if (playerRocketSeeking1.GetCloseHitBox().IntersectsWith(enemy.GetCloseHitBox()))
                        {
                            playerRocketSeeking1.SetBlast();
                            LooseUfoEnemyHealth(enemy);
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
            if (playerRocketSeeking.IsFadingComplete || hitbox.Left > Constants.DEFAULT_SCENE_WIDTH || hitbox.Right < 0 || hitbox.Top < 0 || hitbox.Bottom > Constants.DEFAULT_SCENE_HEIGHT)
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

        #region RoadSideWalkSlope

        private bool SpawnRoadSideWalkSlopes()
        {
            for (int i = 0; i < 10; i++)
            {
                RoadSideWalkSlope roadSideStripe = new(
                    animateAction: AnimateRoadSideWalkSlope,
                    recycleAction: RecycleRoadSideWalkSlope);

                roadSideStripe.SetPosition(
                    left: -3000,
                    top: -3000);

                _scene_game.AddToScene(roadSideStripe);
            }

            return true;
        }

        private bool GenerateRoadSideWalkSlopeTop()
        {
            if (_scene_game.Children.OfType<RoadSideWalkSlope>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideWalkSlope roadSideStripe)
            {
                roadSideStripe.IsAnimating = true;

                roadSideStripe.SetPosition(
                    left: (Constants.DEFAULT_SCENE_WIDTH / 5.4),
                    top: (roadSideStripe.Height * -1) - 16.5,
                    z: 0);

                return true;
            }

            return false;
        }

        private bool GenerateRoadSideWalkSlopeBottom()
        {
            if (_scene_game.Children.OfType<RoadSideWalkSlope>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideWalkSlope roadSideWalkSlope)
            {
                roadSideWalkSlope.IsAnimating = true;

                roadSideWalkSlope.SetPosition(
                    left: (roadSideWalkSlope.Height * -1),
                    top: (Constants.DEFAULT_SCENE_HEIGHT / 2.1) - 4.5,
                    z: 0);

                return true;
            }

            return false;
        }

        private bool AnimateRoadSideWalkSlope(Construct roadSideWalkSlope)
        {
            RoadSideWalkSlope roadSideStripe1 = roadSideWalkSlope as RoadSideWalkSlope;
            var speed = roadSideStripe1.GetMovementSpeed();
            roadSideStripe1.MoveDownRight(speed);
            return true;
        }

        private bool RecycleRoadSideWalkSlope(Construct roadSideWalkSlope)
        {
            var hitBox = roadSideWalkSlope.GetHitBox();

            if (hitBox.Top - roadSideWalkSlope.Height > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left - roadSideWalkSlope.Height > Constants.DEFAULT_SCENE_WIDTH)
            {
                roadSideWalkSlope.IsAnimating = false;

                roadSideWalkSlope.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        #endregion

        #region RoadSideWalk

        private bool SpawnRoadSideWalks()
        {
            for (int i = 0; i < 8; i++)
            {
                RoadSideWalk roadSideWalk = new(
                animateAction: AnimateRoadSideWalk,
                recycleAction: RecycleRoadSideWalk);

                roadSideWalk.SetPosition(
                    left: -3000,
                    top: -3000);

                _scene_game.AddToScene(roadSideWalk);
            }

            return true;
        }

        private bool GenerateRoadSideWalkTop()
        {
            if (_scene_game.Children.OfType<RoadSideWalk>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideWalk roadSideWalk)
            {
                roadSideWalk.IsAnimating = true;

                roadSideWalk.SetPosition(
                    left: (Constants.DEFAULT_SCENE_WIDTH / 2.25 - roadSideWalk.Width),
                    top: roadSideWalk.Height * -1,
                    z: 0);

                return true;
            }

            return false;
        }

        private bool GenerateRoadSideWalkBottom()
        {
            if (_scene_game.Children.OfType<RoadSideWalk>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideWalk roadSideWalk)
            {
                roadSideWalk.IsAnimating = true;

                roadSideWalk.SetPosition(
                    left: (roadSideWalk.Height * -1.5) - 30,
                    top: (Constants.DEFAULT_SCENE_HEIGHT / 5 + roadSideWalk.Height / 2) - 50,
                    z: 0);

                return true;
            }

            return false;
        }

        private bool AnimateRoadSideWalk(Construct roadSideWalk)
        {
            RoadSideWalk roadSideWalk1 = roadSideWalk as RoadSideWalk;
            var speed = roadSideWalk1.GetMovementSpeed();
            roadSideWalk1.MoveDownRight(speed);
            return true;
        }

        private bool RecycleRoadSideWalk(Construct roadSideWalk)
        {
            var hitBox = roadSideWalk.GetHitBox();

            if (hitBox.Top > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left - roadSideWalk.Width > Constants.DEFAULT_SCENE_WIDTH)
            {
                roadSideWalk.IsAnimating = false;

                roadSideWalk.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        #endregion

        #region RoadSideTree

        private bool SpawnRoadSideTrees()
        {
            for (int i = 0; i < 6; i++)
            {
                RoadSideTree roadSideTree = new(
                    animateAction: AnimateRoadSideTree,
                    recycleAction: RecycleRoadSideTree);

                roadSideTree.SetPosition(
                    left: -3000,
                    top: -3000);

                _scene_game.AddToScene(roadSideTree);

                SpawnDropShadow(source: roadSideTree);
            }

            return true;
        }

        private bool GenerateRoadSideTreeTop()
        {
            if (_scene_game.Children.OfType<RoadSideTree>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideTree tree)
            {
                tree.IsAnimating = true;

                tree.SetPosition(
                  left: (Constants.DEFAULT_SCENE_WIDTH / 2 - tree.Width),
                  top: (tree.Height * -1.1),
                  z: 3);

                GenerateDropShadow(source: tree);
            }

            return true;
        }

        private bool GenerateRoadSideTreeBottom()
        {
            if (_scene_game.Children.OfType<RoadSideTree>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideTree tree)
            {
                tree.IsAnimating = true;

                tree.SetPosition(
                  left: (-1 * tree.Width),
                  top: (Constants.DEFAULT_SCENE_HEIGHT / 3),
                  z: 4);

                GenerateDropShadow(tree);
            }

            return true;
        }

        private bool AnimateRoadSideTree(Construct roadSideTree)
        {
            RoadSideTree roadSideTree1 = roadSideTree as RoadSideTree;
            var speed = roadSideTree1.GetMovementSpeed();
            roadSideTree1.MoveDownRight(speed);
            return true;
        }

        private bool RecycleRoadSideTree(Construct roadSideTree)
        {
            var hitBox = roadSideTree.GetHitBox();

            if (hitBox.Top > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left - roadSideTree.Width > Constants.DEFAULT_SCENE_WIDTH)
            {
                roadSideTree.IsAnimating = false;

                roadSideTree.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        #endregion

        #region RoadSideHedge

        private bool SpawnRoadSideHedges()
        {
            for (int i = 0; i < 11; i++)
            {
                RoadSideHedge roadSideHedge = new(
                    animateAction: AnimateRoadSideHedge,
                    recycleAction: RecycleRoadSideHedge);

                roadSideHedge.SetPosition(
                    left: -3000,
                    top: -3000);

                _scene_game.AddToScene(roadSideHedge);
            }

            return true;
        }

        private bool GenerateRoadSideHedgeTop()
        {
            if (_scene_game.Children.OfType<RoadSideHedge>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideHedge roadSideHedge)
            {
                roadSideHedge.IsAnimating = true;

                roadSideHedge.SetPosition(
                  left: (Constants.DEFAULT_SCENE_WIDTH / 3.8) - 30,
                  top: (roadSideHedge.Height * -1) - 30,
                  z: 2);

                return true;
            }

            return false;
        }

        private bool GenerateRoadSideHedgeBottom()
        {
            if (_scene_game.Children.OfType<RoadSideHedge>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideHedge roadSideHedge)
            {
                roadSideHedge.IsAnimating = true;

                roadSideHedge.SetPosition(
                  left: (-1.1 * roadSideHedge.Width) - 10,
                  top: (Constants.DEFAULT_SCENE_HEIGHT / 3.1) - 10,
                  z: 3);

                return true;
            }

            return false;
        }

        private bool AnimateRoadSideHedge(Construct roadSideHedge)
        {
            RoadSideHedge roadSideHedge1 = roadSideHedge as RoadSideHedge;
            var speed = roadSideHedge1.GetMovementSpeed();
            roadSideHedge1.MoveDownRight(speed);
            return true;
        }

        private bool RecycleRoadSideHedge(Construct roadSideHedge)
        {
            var hitBox = roadSideHedge.GetHitBox();

            if (hitBox.Top > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left - roadSideHedge.Width > Constants.DEFAULT_SCENE_WIDTH)
            {
                roadSideHedge.IsAnimating = false;

                roadSideHedge.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        #endregion

        #region RoadSideLamp

        private bool SpawnRoadSideLamps()
        {
            for (int i = 0; i < 4; i++)
            {
                RoadSideLamp roadSideLamp = new(
                    animateAction: AnimateRoadSideLamp,
                    recycleAction: RecycleRoadSideLamp);

                roadSideLamp.SetPosition(
                    left: -3000,
                    top: -3000);

                _scene_game.AddToScene(roadSideLamp);

                SpawnDropShadow(source: roadSideLamp);
            }

            return true;
        }

        private bool GenerateRoadSideLampTop()
        {
            if (_scene_game.Children.OfType<RoadSideLamp>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideLamp roadSideLamp)
            {
                roadSideLamp.IsAnimating = true;

                roadSideLamp.SetPosition(
                  left: (Constants.DEFAULT_SCENE_WIDTH / 2.40 - roadSideLamp.Width) + 10,
                  top: ((roadSideLamp.Height * 1.5) * -1) + 5,
                  z: 3);

                GenerateDropShadow(roadSideLamp);

                LoggerExtensions.Log("RoadSideLamp generated.");

                return true;
            }

            return false;
        }

        private bool GenerateRoadSideLampBottom()
        {
            if (_scene_game.Children.OfType<RoadSideLamp>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideLamp tree)
            {
                tree.IsAnimating = true;

                tree.SetPosition(
                  left: (-1.9 * tree.Width),
                  top: (Constants.DEFAULT_SCENE_HEIGHT / 3),
                  z: 4);

                GenerateDropShadow(tree);

                return true;
            }

            return false;
        }

        private bool AnimateRoadSideLamp(Construct roadSideLamp)
        {
            RoadSideLamp roadSideLamp1 = roadSideLamp as RoadSideLamp;
            var speed = roadSideLamp1.GetMovementSpeed();
            roadSideLamp1.MoveDownRight(speed);
            return true;
        }

        private bool RecycleRoadSideLamp(Construct roadSideLamp)
        {
            var hitBox = roadSideLamp.GetHitBox();

            if (hitBox.Top > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left - roadSideLamp.Width > Constants.DEFAULT_SCENE_WIDTH)
            {
                roadSideLamp.IsAnimating = false;

                roadSideLamp.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        #endregion

        #region RoadSideBillboard

        private bool SpawnRoadSideBillboards()
        {
            for (int i = 0; i < 3; i++)
            {
                RoadSideBillboard roadSideBillboard = new(
                    animateAction: AnimateRoadSideBillboard,
                    recycleAction: RecycleRoadSideBillboard);

                roadSideBillboard.SetPosition(
                    left: -3000,
                    top: -3000);

                _scene_game.AddToScene(roadSideBillboard);

                SpawnDropShadow(source: roadSideBillboard);
            }

            return true;
        }

        private bool GenerateRoadSideBillboardTop()
        {
            if (_scene_game.Children.OfType<RoadSideBillboard>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideBillboard roadSideBillboard)
            {
                roadSideBillboard.IsAnimating = true;

                roadSideBillboard.SetPosition(
                  left: (Constants.DEFAULT_SCENE_WIDTH / 2.5 - roadSideBillboard.Width) + 48,
                  top: ((roadSideBillboard.Height * 1.5) * -1) - 10,
                  z: 4);

                GenerateDropShadow(roadSideBillboard);

                LoggerExtensions.Log("RoadSideBillboard generated.");

                return true;
            }

            return false;
        }

        private bool GenerateRoadSideBillboardBottom()
        {
            if (_scene_game.Children.OfType<RoadSideBillboard>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideBillboard tree)
            {
                tree.IsAnimating = true;

                tree.SetPosition(
                  left: (-1.9 * tree.Width),
                  top: (Constants.DEFAULT_SCENE_HEIGHT / 3),
                  z: 4);

                GenerateDropShadow(tree);

                return true;
            }

            return false;
        }

        private bool AnimateRoadSideBillboard(Construct roadSideBillboard)
        {
            RoadSideBillboard roadSideBillboard1 = roadSideBillboard as RoadSideBillboard;
            var speed = roadSideBillboard1.GetMovementSpeed();
            roadSideBillboard1.MoveDownRight(speed);
            return true;
        }

        private bool RecycleRoadSideBillboard(Construct roadSideBillboard)
        {
            var hitBox = roadSideBillboard.GetHitBox();

            if (hitBox.Top > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left - roadSideBillboard.Width > Constants.DEFAULT_SCENE_WIDTH)
            {
                roadSideBillboard.IsAnimating = false;

                roadSideBillboard.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        #endregion      

        #region UfoBoss

        private bool SpawnUfoBosses()
        {
            UfoBoss ufoBoss = new(
                animateAction: AnimateUfoBoss,
                recycleAction: RecycleUfoBoss);

            ufoBoss.SetPosition(
                left: -3000,
                top: -3000,
                z: 8);

            _scene_game.AddToScene(ufoBoss);

            SpawnDropShadow(source: ufoBoss);

            return true;
        }

        private bool GenerateUfoBoss()
        {
            // if scene doesn't contain a UfoBoss then pick a UfoBoss and add to scene

            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _ufo_boss_threashold.ShouldRelease(_game_score_bar.GetScore()) && !UfoBossExists() &&
                _scene_game.Children.OfType<UfoBoss>().FirstOrDefault(x => x.IsAnimating == false) is UfoBoss ufoBoss)
            {
                _audio_stub.Stop(SoundType.GAME_BACKGROUND_MUSIC);
                //_audio_stub.Play(SoundType.UFO_BOSS_BACKGROUND_MUSIC);
                _audio_stub.SetVolume(SoundType.AMBIENCE, 0.2);

                ufoBoss.IsAnimating = true;

                ufoBoss.Reset();
                ufoBoss.SetPosition(
                    left: 0,
                    top: ufoBoss.Height * -1);

                GenerateDropShadow(ufoBoss);

                // set UfoBoss health
                ufoBoss.Health = _ufo_boss_threashold.GetReleasePointDifference() * 1.5;

                _ufo_boss_threashold.IncreaseThreasholdLimit(increment: _ufo_boss_threashold_limit_increase, currentPoint: _game_score_bar.GetScore());

                _ufo_boss_health_bar.SetMaxiumHealth(ufoBoss.Health);
                _ufo_boss_health_bar.SetValue(ufoBoss.Health);
                _ufo_boss_health_bar.SetIcon(ufoBoss.GetContentUri());
                _ufo_boss_health_bar.SetBarColor(color: Colors.Crimson);

                _scene_game.ActivateSlowMotion();

                GenerateInterimScreen("Beware of Cyber Psycho");

                return true;
            }

            return false;
        }

        private bool AnimateUfoBoss(Construct ufoBoss)
        {
            UfoBoss ufoBoss1 = ufoBoss as UfoBoss;

            if (ufoBoss1.IsDead)
            {
                ufoBoss.Shrink();
            }
            else
            {
                ufoBoss.Pop();

                ufoBoss1.Hover();
                ufoBoss1.DepleteHitStance();
                ufoBoss1.DepleteWinStance();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    var speed = ufoBoss1.GetMovementSpeed();
                    var scaling = ScreenExtensions.GetScreenSpaceScaling();

                    if (ufoBoss1.IsAttacking)
                    {
                        ufoBoss1.Move(
                            speed: speed,
                            sceneWidth: Constants.DEFAULT_SCENE_WIDTH * scaling,
                            sceneHeight: Constants.DEFAULT_SCENE_HEIGHT * scaling,
                            playerPoint: _player.GetCloseHitBox());
                    }
                    else
                    {
                        ufoBoss1.MoveDownRight(speed);

                        if (ufoBoss.GetLeft() > (Constants.DEFAULT_SCENE_WIDTH * scaling / 3)) // bring UfoBoss to a suitable distance from player and then start attacking
                        {
                            ufoBoss1.IsAttacking = true;
                        }
                    }
                }
            }

            return true;
        }

        private bool RecycleUfoBoss(Construct ufoBoss)
        {
            if (ufoBoss.IsShrinkingComplete)
            {
                ufoBoss.IsAnimating = false;

                ufoBoss.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        private void LooseUfoBossHealth(UfoBoss ufoBoss)
        {
            ufoBoss.SetPopping();
            ufoBoss.LooseHealth();
            ufoBoss.SetHitStance();

            _ufo_boss_health_bar.SetValue(ufoBoss.Health);

            if (ufoBoss.IsDead)
            {
                //_audio_stub.Stop(SoundType.UFO_BOSS_BACKGROUND_MUSIC);
                _audio_stub.Play(SoundType.GAME_BACKGROUND_MUSIC);
                _audio_stub.SetVolume(SoundType.AMBIENCE, 0.6);

                _player.SetWinStance();
                _game_score_bar.GainScore(5);

                GenerateInterimScreen("Cyber Psycho Will Return");

                _scene_game.ActivateSlowMotion();
            }
        }

        private bool UfoBossExists()
        {
            return _scene_game.Children.OfType<UfoBoss>().Any(x => x.IsAnimating);
        }

        #endregion

        #region UfoBossRocket

        private bool SpawnUfoBossRockets()
        {
            for (int i = 0; i < 4; i++)
            {
                UfoBossRocket ufoBossRocket = new(
                    animateAction: AnimateUfoBossRocket,
                    recycleAction: RecycleUfoBossRocket);

                ufoBossRocket.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 7);

                _scene_game.AddToScene(ufoBossRocket);

                SpawnDropShadow(source: ufoBossRocket);
            }

            return true;
        }

        private bool GenerateUfoBossRocket()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<UfoBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is UfoBoss ufoBoss &&
                _scene_game.Children.OfType<UfoBossRocket>().FirstOrDefault(x => x.IsAnimating == false) is UfoBossRocket ufoBossRocket)
            {
                ufoBossRocket.Reset();
                ufoBossRocket.IsAnimating = true;
                ufoBossRocket.SetPopping();
                ufoBossRocket.Reposition(UfoBoss: ufoBoss);

                GenerateDropShadow(ufoBossRocket);
                SetUfoBossRocketDirection(source: ufoBoss, rocket: ufoBossRocket, rocketTarget: _player);

                LoggerExtensions.Log("UfoBoss Bomb dropped.");

                return true;
            }

            return false;
        }

        private bool AnimateUfoBossRocket(Construct ufoBossRocket)
        {
            UfoBossRocket ufoBossRocket1 = ufoBossRocket as UfoBossRocket;

            var speed = ufoBossRocket1.GetMovementSpeed();

            if (ufoBossRocket1.AwaitMoveDownLeft)
            {
                ufoBossRocket1.MoveDownLeft(speed);
            }
            else if (ufoBossRocket1.AwaitMoveUpRight)
            {
                ufoBossRocket1.MoveUpRight(speed);
            }
            else if (ufoBossRocket1.AwaitMoveUpLeft)
            {
                ufoBossRocket1.MoveUpLeft(speed);
            }
            else if (ufoBossRocket1.AwaitMoveDownRight)
            {
                ufoBossRocket1.MoveDownRight(speed);
            }

            if (ufoBossRocket1.IsBlasting)
            {
                ufoBossRocket.Expand();
                ufoBossRocket.Fade(0.02);
            }
            else
            {
                ufoBossRocket.Pop();
                ufoBossRocket1.Hover();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (ufoBossRocket.GetCloseHitBox().IntersectsWith(_player.GetCloseHitBox()))
                    {
                        ufoBossRocket1.SetBlast();
                        LoosePlayerHealth();
                    }

                    if (ufoBossRocket1.AutoBlast())
                        ufoBossRocket1.SetBlast();
                }
            }

            return true;
        }

        private bool RecycleUfoBossRocket(Construct ufoBossRocket)
        {
            //var hitbox = bomb.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (ufoBossRocket.IsFadingComplete /*|| hitbox.Left > Constants.DEFAULT_SCENE_WIDTH || hitbox.Right < 0 || hitbox.Top < 0 || hitbox.Top > Constants.DEFAULT_SCENE_HEIGHT*/)
            {
                ufoBossRocket.IsAnimating = false;

                ufoBossRocket.SetPosition(
                    left: -3000,
                    top: -3000);

                return true;
            }

            return false;
        }

        #endregion                

        #region UfoBossRocketSeeking

        private bool SpawnUfoBossRocketSeekings()
        {
            for (int i = 0; i < 2; i++)
            {
                UfoBossRocketSeeking ufoBossRocketSeeking = new(
                    animateAction: AnimateUfoBossRocketSeeking,
                    recycleAction: RecycleUfoBossRocketSeeking);

                ufoBossRocketSeeking.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 7);

                _scene_game.AddToScene(ufoBossRocketSeeking);

                SpawnDropShadow(source: ufoBossRocketSeeking);
            }

            return true;
        }

        private bool GenerateUfoBossRocketSeeking()
        {
            // generate a seeking bomb if one is not in scene
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<UfoBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is UfoBoss ufoBoss &&
                !_scene_game.Children.OfType<UfoBossRocketSeeking>().Any(x => x.IsAnimating) &&
                _scene_game.Children.OfType<UfoBossRocketSeeking>().FirstOrDefault(x => x.IsAnimating == false) is UfoBossRocketSeeking ufoBossRocketSeeking)
            {
                ufoBossRocketSeeking.Reset();
                ufoBossRocketSeeking.IsAnimating = true;
                ufoBossRocketSeeking.SetPopping();

                ufoBossRocketSeeking.Reposition(
                    UfoBoss: ufoBoss);

                GenerateDropShadow(ufoBossRocketSeeking);

                LoggerExtensions.Log("UfoBoss Seeking Bomb dropped.");

                return true;
            }

            return false;
        }

        private bool AnimateUfoBossRocketSeeking(Construct ufoBossRocketSeeking)
        {
            UfoBossRocketSeeking ufoBossRocketSeeking1 = ufoBossRocketSeeking as UfoBossRocketSeeking;

            var speed = ufoBossRocketSeeking1.GetMovementSpeed();

            if (ufoBossRocketSeeking1.IsBlasting)
            {
                ufoBossRocketSeeking.Expand();
                ufoBossRocketSeeking.Fade(0.02);
                ufoBossRocketSeeking1.MoveDownRight(speed);
            }
            else
            {
                ufoBossRocketSeeking.Pop();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (_scene_game.Children.OfType<UfoBoss>().Any(x => x.IsAnimating && x.IsAttacking))
                    {
                        ufoBossRocketSeeking1.Seek(_player.GetCloseHitBox());

                        if (ufoBossRocketSeeking1.GetCloseHitBox().IntersectsWith(_player.GetCloseHitBox()))
                        {
                            ufoBossRocketSeeking1.SetBlast();
                            LoosePlayerHealth();
                        }
                        else
                        {
                            if (ufoBossRocketSeeking1.RunOutOfTimeToBlast())
                                ufoBossRocketSeeking1.SetBlast();
                        }
                    }
                    else
                    {
                        ufoBossRocketSeeking1.SetBlast();
                    }
                }
            }

            return true;
        }

        private bool RecycleUfoBossRocketSeeking(Construct ufoBossRocketSeeking)
        {
            var hitbox = ufoBossRocketSeeking.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (ufoBossRocketSeeking.IsFadingComplete || hitbox.Left > Constants.DEFAULT_SCENE_WIDTH || hitbox.Right < 0 || hitbox.Top < 0 || hitbox.Bottom > Constants.DEFAULT_SCENE_HEIGHT)
            {
                ufoBossRocketSeeking.IsAnimating = false;

                ufoBossRocketSeeking.SetPosition(
                    left: -3000,
                    top: -3000);

                return true;
            }

            return false;
        }

        #endregion

        #region UfoEnemy

        private bool SpawnUfoEnemys()
        {
            for (int i = 0; i < 7; i++)
            {
                UfoEnemy ufoEnemy = new(
                    animateAction: AnimateUfoEnemy,
                    recycleAction: RecycleUfoEnemy);

                _scene_game.AddToScene(ufoEnemy);

                ufoEnemy.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 8);

                SpawnDropShadow(source: ufoEnemy);
            }

            return true;
        }

        private bool GenerateUfoEnemy()
        {
            if (!UfoBossExists() &&
                _enemy_threashold.ShouldRelease(_game_score_bar.GetScore()) &&
                _scene_game.Children.OfType<UfoEnemy>().FirstOrDefault(x => x.IsAnimating == false) is UfoEnemy ufoEnemy)
            {
                ufoEnemy.IsAnimating = true;
                ufoEnemy.Reset();
                ufoEnemy.Reposition();

                GenerateDropShadow(ufoEnemy);

                if (!_enemy_fleet_appeared)
                {
                    _audio_stub.Play(SoundType.UFO_ENEMY_ENTRY);

                    GenerateInterimScreen("Beware of Aliens");
                    _scene_game.ActivateSlowMotion();
                    _enemy_fleet_appeared = true;
                }

                return true;
            }

            return false;
        }

        private bool AnimateUfoEnemy(Construct ufoEnemy)
        {
            UfoEnemy ufoEnemy1 = ufoEnemy as UfoEnemy;

            if (ufoEnemy1.IsDead)
            {
                ufoEnemy1.Shrink();
            }
            else
            {
                ufoEnemy1.Hover();
                ufoEnemy1.Pop();

                var speed = ufoEnemy1.GetMovementSpeed();

                ufoEnemy1.MoveDownRight(speed);

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (ufoEnemy1.Honk())
                        GenerateUfoEnemyHonk(ufoEnemy1);

                    if (ufoEnemy1.Attack())
                        GenerateUfoEnemyRocket(ufoEnemy1);
                }
            }

            return true;
        }

        private bool RecycleUfoEnemy(Construct ufoEnemy)
        {
            var hitbox = ufoEnemy.GetHitBox();

            // enemy is dead or goes out of bounds
            if (ufoEnemy.IsShrinkingComplete ||
                hitbox.Left > Constants.DEFAULT_SCENE_WIDTH || hitbox.Top > Constants.DEFAULT_SCENE_HEIGHT ||
                hitbox.Right < 0 || hitbox.Bottom < 0)
            {
                ufoEnemy.IsAnimating = false;

                ufoEnemy.SetPosition(
                    left: -3000,
                    top: -3000);

                LoggerExtensions.Log("UfoEnemy Recycled");
            }

            return true;
        }

        private void LooseUfoEnemyHealth(UfoEnemy ufoEnemy)
        {
            ufoEnemy.SetPopping();
            ufoEnemy.LooseHealth();

            if (ufoEnemy.IsDead)
            {
                _game_score_bar.GainScore(3);

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

                LoggerExtensions.Log("UfoEnemy dead");
            }
        }

        private bool UfoEnemyExists()
        {
            return _scene_game.Children.OfType<UfoEnemy>().Any(x => x.IsAnimating);
        }

        #endregion

        #region UfoEnemyRocket

        private bool SpawnUfoEnemyRockets()
        {
            for (int i = 0; i < 8; i++)
            {
                UfoEnemyRocket ufoEnemyRocket = new(
                    animateAction: AnimateUfoEnemyRocket,
                    recycleAction: RecycleUfoEnemyRocket);

                ufoEnemyRocket.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 8);

                _scene_game.AddToScene(ufoEnemyRocket);

                SpawnDropShadow(source: ufoEnemyRocket);
            }

            return true;
        }

        private bool GenerateUfoEnemyRocket(UfoEnemy ufoEnemy)
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<UfoEnemyRocket>().FirstOrDefault(x => x.IsAnimating == false) is UfoEnemyRocket enemyRocket)
            {
                enemyRocket.Reset();
                enemyRocket.IsAnimating = true;
                enemyRocket.SetPopping();

                enemyRocket.Reposition(ufoEnemy: ufoEnemy);

                GenerateDropShadow(enemyRocket);

                LoggerExtensions.Log("UfoEnemy Bomb dropped.");

                return true;
            }

            return false;
        }

        private bool AnimateUfoEnemyRocket(Construct ufoEnemyRocket)
        {
            UfoEnemyRocket ufoEnemyRocket1 = ufoEnemyRocket as UfoEnemyRocket;

            var speed = ufoEnemyRocket1.GetMovementSpeed();
            ufoEnemyRocket1.MoveDownRight(speed);

            if (ufoEnemyRocket1.IsBlasting)
            {
                ufoEnemyRocket.Expand();
                ufoEnemyRocket.Fade(0.02);
            }
            else
            {
                ufoEnemyRocket.Pop();
                ufoEnemyRocket1.Hover();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (ufoEnemyRocket1.GetCloseHitBox().IntersectsWith(_player.GetCloseHitBox()))
                    {
                        ufoEnemyRocket1.SetBlast();
                        LoosePlayerHealth();
                    }

                    if (ufoEnemyRocket1.AutoBlast())
                        ufoEnemyRocket1.SetBlast();
                }
            }

            return true;
        }

        private bool RecycleUfoEnemyRocket(Construct ufoEnemyRocket)
        {
            var hitbox = ufoEnemyRocket.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (ufoEnemyRocket.IsFadingComplete || hitbox.Left > Constants.DEFAULT_SCENE_WIDTH || hitbox.Right < 0 || hitbox.Top < 0 || hitbox.Bottom > Constants.DEFAULT_SCENE_HEIGHT)
            {
                ufoEnemyRocket.IsAnimating = false;

                ufoEnemyRocket.SetPosition(
                    left: -3000,
                    top: -3000);

                return true;
            }

            return false;
        }

        #endregion

        #region VehicleEnemy

        private bool SpawnVehicleEnemys()
        {
            for (int i = 0; i < 8; i++)
            {
                VehicleEnemy vehicleEnemy = new(
                    animateAction: AnimateVehicleEnemy,
                    recycleAction: RecycleVehicleEnemy);

                _scene_game.AddToScene(vehicleEnemy);

                vehicleEnemy.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        private bool GenerateVehicleEnemy()
        {
            if (!UfoBossExists() && !VehicleBossExists() && !ZombieBossExists() && _scene_game.Children.OfType<VehicleEnemy>().FirstOrDefault(x => x.IsAnimating == false) is VehicleEnemy vehicleEnemy)
            {
                vehicleEnemy.IsAnimating = true;
                vehicleEnemy.Reset();
                vehicleEnemy.Reposition();

                return true;
            }
            return false;
        }

        private bool AnimateVehicleEnemy(Construct vehicleEnemy)
        {
            VehicleEnemy vehicleEnemy1 = vehicleEnemy as VehicleEnemy;

            vehicleEnemy.Pop();
            vehicleEnemy1.Vibrate();

            var speed = vehicleEnemy1.GetMovementSpeed();
            vehicleEnemy1.MoveDownRight(speed);

            if (_scene_game.SceneState == SceneState.GAME_RUNNING)
            {
                if (vehicleEnemy1.Honk())
                    GenerateVehicleEnemyHonk(vehicleEnemy1);
            }

            PreventVehicleEnemyOverlapping(vehicleEnemy);

            return true;
        }

        private bool RecycleVehicleEnemy(Construct vehicleEnemy)
        {
            var hitBox = vehicleEnemy.GetHitBox();

            if (hitBox.Top > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left > Constants.DEFAULT_SCENE_WIDTH)
            {
                vehicleEnemy.IsAnimating = false;

                vehicleEnemy.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        private void PreventVehicleEnemyOverlapping(Construct vehicleEnemy)
        {
            //var vehicleEnemy_distantHitBox = vehicleEnemy.GetDistantHitBox();

            if (_scene_game.Children.OfType<VehicleEnemy>()
                .FirstOrDefault(x => x.IsAnimating && x.GetHitBox().IntersectsWith(vehicleEnemy.GetHitBox())) is Construct collidingVehicleEnemy)
            {
                var hitBox = vehicleEnemy.GetHitBox();

                //if (vehicleEnemy.SpeedOffset == collidingVehicleEnemy.SpeedOffset)
                //{
                //    if (vehicleEnemy.SpeedOffset > -1)
                //        vehicleEnemy.SpeedOffset--;
                //}
                //else
                //{

                if (collidingVehicleEnemy.SpeedOffset > vehicleEnemy.SpeedOffset) // colliding vehicleEnemy is faster
                {
                    vehicleEnemy.SpeedOffset = collidingVehicleEnemy.SpeedOffset;
                }
                else if (vehicleEnemy.SpeedOffset > collidingVehicleEnemy.SpeedOffset) // vehicleEnemy is faster
                {
                    collidingVehicleEnemy.SpeedOffset = vehicleEnemy.SpeedOffset;
                }
                //}
            }
        }

        #endregion        

        #region VehicleBoss

        private bool SpawnVehicleBosses()
        {
            VehicleBoss vehicleBoss = new(
                animateAction: AnimateVehicleBoss,
                recycleAction: RecycleVehicleBoss);

            vehicleBoss.SetPosition(
                left: -3000,
                top: -3000,
                z: 3);

            _scene_game.AddToScene(vehicleBoss);

            return true;
        }

        private bool GenerateVehicleBoss()
        {
            // if scene doesn't contain a VehicleBoss then pick a random VehicleBoss and add to scene

            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _vehicle_boss_threashold.ShouldRelease(_game_score_bar.GetScore()) && !VehicleBossExists() &&
                _scene_game.Children.OfType<VehicleBoss>().FirstOrDefault(x => x.IsAnimating == false) is VehicleBoss vehicleBoss)
            {
                _audio_stub.Stop(SoundType.GAME_BACKGROUND_MUSIC);
                _audio_stub.Play(SoundType.UFO_BOSS_BACKGROUND_MUSIC);
                _audio_stub.SetVolume(SoundType.AMBIENCE, 0.4);

                vehicleBoss.IsAnimating = true;

                vehicleBoss.Reset();
                vehicleBoss.Reposition();
                vehicleBoss.SetZ(3);

                // set VehicleBoss health
                vehicleBoss.Health = _vehicle_boss_threashold.GetReleasePointDifference() * 1.5;

                _vehicle_boss_threashold.IncreaseThreasholdLimit(increment: _vehicle_boss_threashold_limit_increase, currentPoint: _game_score_bar.GetScore());

                _vehicle_boss_health_bar.SetMaxiumHealth(vehicleBoss.Health);
                _vehicle_boss_health_bar.SetValue(vehicleBoss.Health);
                _vehicle_boss_health_bar.SetIcon(vehicleBoss.GetContentUri());
                _vehicle_boss_health_bar.SetBarColor(color: Colors.Crimson);

                GenerateInterimScreen("Crazy Honker Arrived");
                _scene_game.ActivateSlowMotion();

                LoggerExtensions.Log("Vehicle boss generated.");

                return true;
            }

            return false;
        }

        private bool AnimateVehicleBoss(Construct vehicleBoss)
        {
            VehicleBoss vehicleBoss1 = vehicleBoss as VehicleBoss;

            var speed = vehicleBoss1.GetMovementSpeed();

            if (vehicleBoss1.IsDead)
            {
                vehicleBoss1.MoveDownRight(speed);
            }
            else
            {
                vehicleBoss.Pop();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    var scaling = ScreenExtensions.GetScreenSpaceScaling();

                    if (vehicleBoss1.IsAttacking)
                    {
                        vehicleBoss1.Move(
                            speed: speed,
                            sceneWidth: Constants.DEFAULT_SCENE_WIDTH * scaling,
                            sceneHeight: Constants.DEFAULT_SCENE_HEIGHT * scaling);

                        if (vehicleBoss1.Honk())
                            GenerateVehicleBossHonk(vehicleBoss1);
                    }
                    else
                    {
                        if (_scene_game.Children.OfType<VehicleEnemy>().All(x => !x.IsAnimating)
                            || _scene_game.Children.OfType<VehicleEnemy>().Where(x => x.IsAnimating).All(x => x.GetLeft() > Constants.DEFAULT_SCENE_WIDTH * scaling / 2)) // only bring the boss in view when all other vechiles are gone
                        {
                            vehicleBoss1.MoveDownRight(speed);

                            if (vehicleBoss1.GetLeft() > (Constants.DEFAULT_SCENE_WIDTH * scaling / 3)) // bring boss to a suitable distance from player and then start attacking
                            {
                                vehicleBoss1.IsAttacking = true;
                            }
                        }
                    }
                }
            }

            LoggerExtensions.Log($"Vehicle boss at: x: {vehicleBoss1.GetLeft()} y: {vehicleBoss1.GetTop()}.");

            return true;
        }

        private bool RecycleVehicleBoss(Construct vehicleBoss)
        {
            var hitBox = vehicleBoss.GetHitBox();

            VehicleBoss vehicleBoss1 = vehicleBoss as VehicleBoss;

            if (vehicleBoss1.IsDead && hitBox.Top > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left > Constants.DEFAULT_SCENE_WIDTH)
            {
                vehicleBoss.IsAnimating = false;

                vehicleBoss.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        private void LooseVehicleBossHealth(VehicleBoss vehicleBoss)
        {
            vehicleBoss.SetPopping();
            vehicleBoss.LooseHealth();

            _vehicle_boss_health_bar.SetValue(vehicleBoss.Health);

            if (vehicleBoss.IsDead)
            {
                _audio_stub.Stop(SoundType.UFO_BOSS_BACKGROUND_MUSIC);
                _audio_stub.Play(SoundType.GAME_BACKGROUND_MUSIC);
                _audio_stub.SetVolume(SoundType.AMBIENCE, 0.6);

                _player.SetWinStance();
                _game_score_bar.GainScore(5);

                GenerateInterimScreen("Crazy Honker Busted");

                _scene_game.ActivateSlowMotion();
            }
        }

        private bool VehicleBossExists()
        {
            return _scene_game.Children.OfType<VehicleBoss>().Any(x => x.IsAnimating);
        }

        #endregion

        #region VehicleBossRocket

        private bool SpawnVehicleBossRockets()
        {
            for (int i = 0; i < 4; i++)
            {
                VehicleBossRocket vehicleBossRocket = new(
                    animateAction: AnimateVehicleBossRocket,
                    recycleAction: RecycleVehicleBossRocket);

                vehicleBossRocket.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 7);

                _scene_game.AddToScene(vehicleBossRocket);

                SpawnDropShadow(source: vehicleBossRocket);
            }

            return true;
        }

        private bool GenerateVehicleBossRocket()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<VehicleBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is VehicleBoss vehicleBoss &&
                _scene_game.Children.OfType<VehicleBossRocket>().FirstOrDefault(x => x.IsAnimating == false) is VehicleBossRocket vehicleBossRocket)
            {
                vehicleBossRocket.Reset();
                vehicleBossRocket.IsAnimating = true;
                vehicleBossRocket.IsGravitatingUpwards = true;
                vehicleBossRocket.SetPopping();

                vehicleBossRocket.Reposition(vehicleBoss: vehicleBoss);
                vehicleBossRocket.AwaitMoveUpRight = true;

                GenerateDropShadow(source: vehicleBossRocket);

                LoggerExtensions.Log("VehicleBoss rocket dropped.");

                return true;
            }

            return false;
        }

        private bool AnimateVehicleBossRocket(Construct vehicleBossRocket)
        {
            VehicleBossRocket vehicleBossRocket1 = vehicleBossRocket as VehicleBossRocket;

            var speed = vehicleBossRocket1.GetMovementSpeed();

            if (vehicleBossRocket1.AwaitMoveUpRight)
            {
                vehicleBossRocket1.MoveUpRight(speed);
            }

            if (vehicleBossRocket1.IsBlasting)
            {
                vehicleBossRocket.Expand();
                vehicleBossRocket.Fade(0.02);
            }
            else
            {
                vehicleBossRocket.Pop();
                vehicleBossRocket1.DillyDally();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (vehicleBossRocket.GetCloseHitBox().IntersectsWith(_player.GetCloseHitBox()))
                    {
                        vehicleBossRocket1.SetBlast();
                        LoosePlayerHealth();
                    }

                    if (vehicleBossRocket1.AutoBlast())
                        vehicleBossRocket1.SetBlast();
                }
            }

            return true;
        }

        private bool RecycleVehicleBossRocket(Construct vehicleBossRocket)
        {
            //var hitbox = bomb.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (vehicleBossRocket.IsFadingComplete /*|| hitbox.Left > Constants.DEFAULT_SCENE_WIDTH || hitbox.Right < 0 || hitbox.Top < 0 || hitbox.Top > Constants.DEFAULT_SCENE_HEIGHT*/)
            {
                vehicleBossRocket.IsAnimating = false;
                vehicleBossRocket.IsGravitatingUpwards = false;

                vehicleBossRocket.SetPosition(
                    left: -3000,
                    top: -3000);

                return true;
            }

            return false;
        }

        #endregion

        #region ZombieBoss

        private bool SpawnZombieBosses()
        {
            ZombieBoss zombieBoss = new(
                animateAction: AnimateZombieBoss,
                recycleAction: RecycleZombieBoss);

            zombieBoss.SetPosition(
                left: -3000,
                top: -3000,
                z: 8);

            _scene_game.AddToScene(zombieBoss);

            SpawnDropShadow(source: zombieBoss);

            return true;
        }

        private bool GenerateZombieBoss()
        {
            // if scene doesn't contain a ZombieBoss then pick a ZombieBoss and add to scene

            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _zombie_boss_threashold.ShouldRelease(_game_score_bar.GetScore()) && !ZombieBossExists() &&
                _scene_game.Children.OfType<ZombieBoss>().FirstOrDefault(x => x.IsAnimating == false) is ZombieBoss zombieBoss)
            {
                _audio_stub.Stop(SoundType.GAME_BACKGROUND_MUSIC);
                //_audio_stub.Play(SoundType.UFO_BOSS_BACKGROUND_MUSIC);
                _audio_stub.SetVolume(SoundType.AMBIENCE, 0.2);

                zombieBoss.IsAnimating = true;

                zombieBoss.Reset();
                zombieBoss.SetPosition(
                    left: 0,
                    top: zombieBoss.Height * -1);

                GenerateDropShadow(zombieBoss);

                // set ZombieBoss health
                zombieBoss.Health = _zombie_boss_threashold.GetReleasePointDifference() * 1.5;

                _zombie_boss_threashold.IncreaseThreasholdLimit(increment: _zombie_boss_threashold_limit_increase, currentPoint: _game_score_bar.GetScore());

                _zombie_boss_health_bar.SetMaxiumHealth(zombieBoss.Health);
                _zombie_boss_health_bar.SetValue(zombieBoss.Health);
                _zombie_boss_health_bar.SetIcon(zombieBoss.GetContentUri());
                _zombie_boss_health_bar.SetBarColor(color: Colors.Crimson);

                _scene_game.ActivateSlowMotion();

                GenerateInterimScreen("Beware of Blocks Zombie");

                return true;
            }

            return false;
        }

        private bool AnimateZombieBoss(Construct zombieBoss)
        {
            ZombieBoss zombieBoss1 = zombieBoss as ZombieBoss;

            if (zombieBoss1.IsDead)
            {
                zombieBoss.Shrink();
            }
            else
            {
                zombieBoss.Pop();

                zombieBoss1.Hover();
                zombieBoss1.DepleteHitStance();
                zombieBoss1.DepleteWinStance();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    var speed = zombieBoss1.GetMovementSpeed();
                    var scaling = ScreenExtensions.GetScreenSpaceScaling();

                    if (zombieBoss1.IsAttacking)
                    {
                        zombieBoss1.Move(
                            speed: speed,
                            sceneWidth: Constants.DEFAULT_SCENE_WIDTH * scaling,
                            sceneHeight: Constants.DEFAULT_SCENE_HEIGHT * scaling);
                    }
                    else
                    {
                        zombieBoss1.MoveDownRight(speed);

                        if (zombieBoss.GetLeft() > (Constants.DEFAULT_SCENE_WIDTH * scaling / 3)) // bring ZombieBoss to a suitable distance from player and then start attacking
                        {
                            zombieBoss1.IsAttacking = true;
                        }
                    }
                }
            }

            return true;
        }

        private bool RecycleZombieBoss(Construct zombieBoss)
        {
            if (zombieBoss.IsShrinkingComplete)
            {
                zombieBoss.IsAnimating = false;

                zombieBoss.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
        }

        private void LooseZombieBossHealth(ZombieBoss zombieBoss)
        {
            zombieBoss.SetPopping();
            zombieBoss.LooseHealth();
            zombieBoss.SetHitStance();

            _zombie_boss_health_bar.SetValue(zombieBoss.Health);

            if (zombieBoss.IsDead)
            {
                //_audio_stub.Stop(SoundType.UFO_BOSS_BACKGROUND_MUSIC);
                _audio_stub.Play(SoundType.GAME_BACKGROUND_MUSIC);
                _audio_stub.SetVolume(SoundType.AMBIENCE, 0.6);

                _player.SetWinStance();
                _game_score_bar.GainScore(5);

                GenerateInterimScreen("Blocks Zombie Will Return");

                _scene_game.ActivateSlowMotion();
            }
        }

        private bool ZombieBossExists()
        {
            return _scene_game.Children.OfType<ZombieBoss>().Any(x => x.IsAnimating);
        }

        #endregion

        #region ZombieBossRocket

        private bool SpawnZombieBossRockets()
        {
            for (int i = 0; i < 6; i++)
            {
                ZombieBossRocket zombieBossRocket = new(
                    animateAction: AnimateZombieBossRocket,
                    recycleAction: RecycleZombieBossRocket);

                zombieBossRocket.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 7);

                _scene_game.AddToScene(zombieBossRocket);

                SpawnDropShadow(source: zombieBossRocket);
            }

            return true;
        }

        private bool GenerateZombieBossRocket()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<ZombieBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is ZombieBoss zombieBoss &&
                _scene_game.Children.OfType<ZombieBossRocket>().FirstOrDefault(x => x.IsAnimating == false) is ZombieBossRocket zombieBossRocket)
            {
                zombieBossRocket.Reset();
                zombieBossRocket.IsAnimating = true;
                zombieBossRocket.SetPopping();
                zombieBossRocket.Reposition();

                GenerateDropShadow(zombieBossRocket);

                LoggerExtensions.Log("ZombieBoss Bomb dropped.");

                return true;
            }

            return false;
        }

        private bool AnimateZombieBossRocket(Construct zombieBossRocket)
        {
            ZombieBossRocket zombieBossRocket1 = zombieBossRocket as ZombieBossRocket;

            var speed = zombieBossRocket1.GetMovementSpeed();

            zombieBossRocket1.MoveDownRight(speed);

            if (zombieBossRocket1.IsBlasting)
            {
                zombieBossRocket.Expand();
                zombieBossRocket.Fade(0.02);
            }
            else
            {
                zombieBossRocket.Pop();
                zombieBossRocket1.Hover();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (zombieBossRocket.GetCloseHitBox().IntersectsWith(_player.GetCloseHitBox()))
                    {
                        zombieBossRocket1.SetBlast();
                        LoosePlayerHealth();
                    }

                    if (zombieBossRocket1.AutoBlast())
                        zombieBossRocket1.SetBlast();
                }
            }

            return true;
        }

        private bool RecycleZombieBossRocket(Construct zombieBossRocket)
        {
            var hitbox = zombieBossRocket.GetHitBox();

            var scaling = ScreenExtensions.GetScreenSpaceScaling();

            // if bomb is blasted and faed or goes out of scene bounds
            if (zombieBossRocket.IsFadingComplete || hitbox.Left > Constants.DEFAULT_SCENE_WIDTH * scaling || hitbox.Top > Constants.DEFAULT_SCENE_HEIGHT * scaling)
            {
                zombieBossRocket.IsAnimating = false;

                zombieBossRocket.SetPosition(
                    left: -3000,
                    top: -3000);

                return true;
            }

            return false;
        }

        #endregion                

        #region Rocket

        private void SetPlayerRocketDirection(Construct source, AnimableConstruct rocket, Construct rocketTarget)
        {
            // rocket target is on the bottom right side of the UfoBoss
            if (rocketTarget.GetTop() > source.GetTop() && rocketTarget.GetLeft() > source.GetLeft())
            {
                rocket.AwaitMoveDownRight = true;
                rocket.SetRotation(33);
            }
            // rocket target is on the bottom left side of the UfoBoss
            else if (rocketTarget.GetTop() > source.GetTop() && rocketTarget.GetLeft() < source.GetLeft())
            {
                rocket.AwaitMoveDownLeft = true;
                rocket.SetRotation(-213);
            }
            // if rocket target is on the top left side of the UfoBoss
            else if (rocketTarget.GetTop() < source.GetTop() && rocketTarget.GetLeft() < source.GetLeft())
            {
                rocket.AwaitMoveUpLeft = true;
                rocket.SetRotation(213);
            }
            // if rocket target is on the top right side of the UfoBoss
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

        private void SetUfoBossRocketDirection(Construct source, AnimableConstruct rocket, Construct rocketTarget)
        {
            // rocket target is on the bottom right side of the UfoBoss
            if (rocketTarget.GetTop() > source.GetTop() && rocketTarget.GetLeft() > source.GetLeft())
            {
                rocket.AwaitMoveDownRight = true;
                rocket.SetRotation(33);
            }
            // rocket target is on the bottom left side of the UfoBoss
            else if (rocketTarget.GetTop() > source.GetTop() && rocketTarget.GetLeft() < source.GetLeft())
            {
                rocket.AwaitMoveDownLeft = true;
                rocket.SetRotation(-213);
            }
            // if rocket target is on the top left side of the UfoBoss
            else if (rocketTarget.GetTop() < source.GetTop() && rocketTarget.GetLeft() < source.GetLeft())
            {
                rocket.AwaitMoveUpLeft = true;
                rocket.SetRotation(213);
            }
            // if rocket target is on the top right side of the UfoBoss
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

        private bool GenerateVehicleBossHonk(VehicleBoss source)
        {
            // if there are no UfoBosses or enemies in the scene the vehicles will honk

            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !UfoBossExists())
            {
                return GenerateHonk(source);
            }

            return true;
        }

        private bool GenerateVehicleEnemyHonk(VehicleEnemy source)
        {
            // if there are no UfoBosses or enemies in the scene the vehicles will honk

            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !UfoBossExists() && !UfoEnemyExists() && !VehicleBossExists())
            {
                return GenerateHonk(source);
            }

            return true;
        }

        private bool GenerateUfoEnemyHonk(UfoEnemy source)
        {
            // if there are no UfoBosses in the scene the vehicles will honk

            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !UfoBossExists())
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
                            var xLaneWidth = Constants.DEFAULT_SCENE_WIDTH / 4;
                            cloud.SetPosition(
                                left: _random.Next(Convert.ToInt32(xLaneWidth - cloud.Width)),
                                top: cloud.Height * -1);
                        }
                        break;
                    case 1:
                        {
                            var yLaneWidth = (Constants.DEFAULT_SCENE_HEIGHT / 2) / 2;
                            cloud.SetPosition(
                                left: cloud.Width * -1,
                                top: _random.Next(Convert.ToInt32(yLaneWidth)));
                        }
                        break;
                    default:
                        break;
                }

                return true;
            }

            return false;
        }

        private bool AnimateCloud(Construct cloud)
        {
            Cloud cloud1 = cloud as Cloud;
            cloud1.Hover();

            var speed = cloud1.GetMovementSpeed();
            cloud1.MoveDownRight(speed);
            return true;
        }

        private bool RecycleCloud(Construct cloud)
        {
            var hitBox = cloud.GetHitBox();

            if (hitBox.Top > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left > Constants.DEFAULT_SCENE_WIDTH)
            {
                cloud.IsAnimating = false;

                cloud.SetPosition(
                    left: -3000,
                    top: -3000);

            }

            return true;
        }

        #endregion

        #region RoadMark

        private bool SpawnRoadMarks()
        {
            for (int i = 0; i < 10; i++)
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
                  left: roadMark.Height * -1,
                  top: roadMark.Height * -1,
                  z: 0);

                return true;
            }

            return false;
        }

        private bool AnimateRoadMark(Construct roadMark)
        {
            RoadMark roadMark1 = roadMark as RoadMark;
            var speed = roadMark1.GetMovementSpeed();
            roadMark1.MoveDownRight(speed);
            return true;
        }

        private bool RecycleRoadMark(Construct roadMark)
        {
            var hitBox = roadMark.GetHitBox();

            if (hitBox.Top > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left - roadMark.Height > Constants.DEFAULT_SCENE_WIDTH)
            {
                roadMark.IsAnimating = false;

                roadMark.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
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
            dropShadow.Move();
            return true;
        }

        private bool RecycleDropShadow(Construct dropShadow)
        {
            DropShadow dropShadow1 = dropShadow as DropShadow;

            if (!dropShadow1.IsParentConstructAnimating())
            {
                dropShadow.IsAnimating = false;
                dropShadow.SetPosition(
                    left: -3000,
                    top: -3000);

                return true;
            }

            return false;
        }

        private void GenerateDropShadow(Construct source)
        {
            if (_scene_game.Children.OfType<DropShadow>().FirstOrDefault(x => x.Id == source.Id) is DropShadow dropShadow)
            {
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
                HealthPickup healthPickup = new(
                    animateAction: AnimateHealthPickup,
                    recycleAction: RecycleHealthPickup);

                healthPickup.SetPosition(
                    left: -3000,
                    top: -3000,
                    z: 6);

                _scene_game.AddToScene(healthPickup);
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
                            var xLaneWidth = Constants.DEFAULT_SCENE_WIDTH / 4;
                            healthPickup.SetPosition(
                                left: _random.Next(Convert.ToInt32(xLaneWidth - healthPickup.Width)),
                                top: healthPickup.Height * -1);
                        }
                        break;
                    case 1:
                        {
                            var yLaneWidth = (Constants.DEFAULT_SCENE_HEIGHT / 2) / 2;
                            healthPickup.SetPosition(
                                left: healthPickup.Width * -1,
                                top: _random.Next(Convert.ToInt32(yLaneWidth)));
                        }
                        break;
                    default:
                        break;
                }

                GenerateDropShadow(healthPickup);

                return true;
            }

            return false;
        }

        private bool AnimateHealthPickup(Construct healthPickup)
        {
            HealthPickup healthPickup1 = healthPickup as HealthPickup;

            var speed = healthPickup1.GetMovementSpeed();

            if (healthPickup1.IsPickedUp)
            {
                healthPickup1.Shrink();
            }
            else
            {
                healthPickup1.MoveDownRight(speed);

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

            if (hitBox.Top > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left > Constants.DEFAULT_SCENE_WIDTH || healthPickup.IsShrinkingComplete)
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
                if ((UfoBossExists() || UfoEnemyExists()) && !_powerUp_health_bar.HasHealth) // if a UfoBoss or enemy exists and currently player has no other power up
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
                                    var xLaneWidth = Constants.DEFAULT_SCENE_WIDTH / 4;
                                    powerUpPickup.SetPosition(
                                        left: _random.Next(Convert.ToInt32(xLaneWidth - powerUpPickup.Width)),
                                        top: powerUpPickup.Height * -1);
                                }
                                break;
                            case 1:
                                {
                                    var yLaneWidth = (Constants.DEFAULT_SCENE_HEIGHT / 2) / 2;
                                    powerUpPickup.SetPosition(
                                        left: powerUpPickup.Width * -1,
                                        top: _random.Next(Convert.ToInt32(yLaneWidth)));
                                }
                                break;
                            default:
                                break;
                        }

                        GenerateDropShadow(powerUpPickup);

                        return true;
                    }
                }
            }

            return false;
        }

        private bool AnimatePowerUpPickup(Construct powerUpPickup)
        {
            PowerUpPickup powerUpPickup1 = powerUpPickup as PowerUpPickup;

            var speed = powerUpPickup1.GetMovementSpeed();

            if (powerUpPickup1.IsPickedUp)
            {
                powerUpPickup1.Shrink();
            }
            else
            {
                powerUpPickup1.MoveDownRight(speed);

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
                        _powerUp_health_bar.SetBarColor(color: Colors.Green);
                    }
                }
            }

            return true;
        }

        private bool RecyclePowerUpPickup(Construct powerUpPickup)
        {
            var hitBox = powerUpPickup.GetHitBox();

            if (hitBox.Top > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left > Constants.DEFAULT_SCENE_WIDTH || powerUpPickup.IsShrinkingComplete)
            {
                powerUpPickup.IsAnimating = false;

                powerUpPickup.SetPosition(
                    left: -3000,
                    top: -3000);
            }

            return true;
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
            _ufo_boss_health_bar.Reset();
            _game_score_bar.Reset();

            AddGeneratorsToScene();

            if (ScreenExtensions.GetScreenOrienation() == ScreenExtensions.RequiredScreenOrientation)
                _scene_game.Play();

            _scene_main_menu.Play();
        }

        private void AddGeneratorsToScene()
        {
            _scene_game.AddToScene(

            // add road marks
            new Generator(
                generationDelay: 20,
                generationAction: GenerateRoadMark,
                startUpAction: SpawnRoadMarks),

            new Generator(
                generationDelay: 72,
                generationAction: GenerateRoadSideBillboardTop,
                startUpAction: SpawnRoadSideBillboards),

            new Generator(
                generationDelay: 36,
                generationAction: GenerateRoadSideLampTop,
                startUpAction: SpawnRoadSideLamps),

            new Generator(
                generationDelay: 36,
                generationAction: GenerateRoadSideLampBottom,
                startUpAction: SpawnRoadSideLamps),

            // add road side walks
            new Generator(
                generationDelay: 18,
                generationAction: GenerateRoadSideWalkTop,
                startUpAction: SpawnRoadSideWalks),

            new Generator(
                generationDelay: 18,
                generationAction: GenerateRoadSideWalkBottom,
                startUpAction: SpawnRoadSideWalks),

            // then add the top trees
            new Generator(
                generationDelay: 18,
                generationAction: GenerateRoadSideTreeTop,
                startUpAction: SpawnRoadSideTrees),

            // then add the bottom trees which will appear forward in z wrt to the vehicles
            new Generator(
                generationDelay: 18,
                generationAction: GenerateRoadSideTreeBottom,
                startUpAction: SpawnRoadSideTrees),

            // then add the top RoadSideHedges
            new Generator(
                generationDelay: 9,
                generationAction: GenerateRoadSideHedgeTop,
                startUpAction: SpawnRoadSideHedges),

            // then add the bottom RoadSideHedges which will appear forward in z wrt to the vehicles
            new Generator(
                generationDelay: 9,
                generationAction: GenerateRoadSideHedgeBottom,
                startUpAction: SpawnRoadSideHedges),

            // then add the vehicles which will appear forward in z wrt the top trees
            new Generator(
                generationDelay: 100,
                generationAction: GenerateVehicleEnemy,
                startUpAction: SpawnVehicleEnemys),

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
                generationDelay: 10,
                generationAction: GenerateUfoBoss,
                startUpAction: SpawnUfoBosses),

             new Generator(
                generationDelay: 10,
                generationAction: GenerateZombieBoss,
                startUpAction: SpawnZombieBosses),

            new Generator(
                generationDelay: 10,
                generationAction: GenerateVehicleBoss,
                startUpAction: SpawnVehicleBosses),

            new Generator(
                generationDelay: 50,
                generationAction: GenerateVehicleBossRocket,
                startUpAction: SpawnVehicleBossRockets,
                randomizeGenerationDelay: true),

            new Generator(
                generationDelay: 40,
                generationAction: GenerateUfoBossRocket,
                startUpAction: SpawnUfoBossRockets,
                randomizeGenerationDelay: true),

            new Generator(
                generationDelay: 30,
                generationAction: GenerateZombieBossRocket,
                startUpAction: SpawnZombieBossRockets),

            new Generator(
                generationDelay: 200,
                generationAction: GenerateUfoBossRocketSeeking,
                startUpAction: SpawnUfoBossRocketSeekings,
                randomizeGenerationDelay: true),

            new Generator(
                generationDelay: 0,
                generationAction: () => { return true; },
                startUpAction: SpawnPlayerRocketSeekings),

            new Generator(
                generationDelay: 180,
                generationAction: GenerateUfoEnemy,
                startUpAction: SpawnUfoEnemys,
                randomizeGenerationDelay: true),

                new Generator(
                generationDelay: 0,
                generationAction: () => { return true; },
                startUpAction: SpawnUfoEnemyRockets),

                new Generator(
                generationDelay: 500,
                generationAction: GenerateHealthPickups,
                startUpAction: SpawnHealthPickups,
                randomizeGenerationDelay: true),

            new Generator(
                generationDelay: 600,
                generationAction: GeneratePowerUpPickups,
                startUpAction: SpawnPowerUpPickups,
                randomizeGenerationDelay: true)
                );

            _scene_main_menu.AddToScene(

            new Generator(
                generationDelay: 0,
                generationAction: () => { return true; },
                startUpAction: SpawnInterimScreen),

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

        private void SetScreenScaling()
        {
            var scaling = ScreenExtensions.GetScreenSpaceScaling();

            LoggerExtensions.Log($"ScreenSpaceScaling: {scaling}");

            // resize the game scene
            _scene_game.Width = ScreenExtensions.Width;
            _scene_game.Height = ScreenExtensions.Height;

            // resize the main menu
            _scene_main_menu.Width = ScreenExtensions.Width;
            _scene_main_menu.Height = ScreenExtensions.Height;


            // scale the scenes
            _scene_game.SetScaleTransform(scaling);
            _scene_main_menu.SetScaleTransform(scaling);
        }

        #endregion

        #endregion        
    }
}
