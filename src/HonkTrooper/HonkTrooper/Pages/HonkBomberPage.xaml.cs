﻿using Microsoft.UI;
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

        private readonly Random _random;

        private readonly Scene _scene_game;
        private readonly Scene _scene_main_menu;
        private readonly Controller _game_controller;

        private readonly HealthBar _player_health_bar;

        private readonly HealthBar _ufo_boss_health_bar;
        private readonly HealthBar _vehicle_boss_health_bar;
        private readonly HealthBar _zombie_boss_health_bar;
        private readonly HealthBar _mafia_boss_health_bar;

        private readonly HealthBar _powerUp_health_bar;
        private readonly HealthBar _sound_pollution_health_bar;

        private readonly ScoreBar _game_score_bar;
        private readonly StackPanel _health_bars;

        private readonly Threashold _ufo_boss_threashold;
        private readonly Threashold _vehicle_boss_threashold;
        private readonly Threashold _zombie_boss_threashold;
        private readonly Threashold _mafia_boss_threashold;

        private readonly Threashold _ufo_enemy_threashold;

        private readonly double _sound_pollution_max_limit = 6; // max 3 vehicles or ufos honking to trigger sound pollution limit

        //TODO: set defaults _vehicle_boss_threashold_limit = 25
        private readonly double _vehicle_boss_threashold_limit = 25; // first appearance
        private readonly double _vehicle_boss_threashold_limit_increase = 15;

        //TODO: set defaults _ufo_boss_threashold_limit = 50
        private readonly double _ufo_boss_threashold_limit = 50; // first appearance
        private readonly double _ufo_boss_threashold_limit_increase = 15;

        //TODO: set defaults _zombie_boss_threashold_limit = 75
        private readonly double _zombie_boss_threashold_limit = 75; // first appearance
        private readonly double _zombie_boss_threashold_limit_increase = 15;

        //TODO: set defaults _mafia_boss_threashold_limit = 100
        private readonly double _mafia_boss_threashold_limit = 100; // first appearance
        private readonly double _mafia_boss_threashold_limit_increase = 15;

        //TODO: set defaults _enemy_threashold_limit = 35
        private readonly double _ufo_enemy_threashold_limit = 35; // first appearance
        private readonly double _ufo_enemy_threashold_limit_increase = 5;

        private double _ufo_enemy_kill_count;
        private readonly double _ufo_enemy_kill_count_limit = 20;

        private bool _ufo_enemy_fleet_appeared;

        private PlayerBalloon _player;
        private PlayerBalloonTemplate _selected_player_template;
        private PlayerHonkBombTemplate _selected_player_honk_bomb_template;
        private int _game_level;

        private readonly AudioStub _audioStub;

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
            _mafia_boss_health_bar = this.MafiaBossHealthBar;

            _powerUp_health_bar = this.PowerUpHealthBar;
            _sound_pollution_health_bar = this.SoundPollutionBar;

            _game_controller = this.GameController;

            _game_score_bar = this.GameScoreBar;
            _health_bars = this.HealthBars;

            _ufo_boss_threashold = new Threashold(_ufo_boss_threashold_limit);
            _zombie_boss_threashold = new Threashold(_zombie_boss_threashold_limit);
            _vehicle_boss_threashold = new Threashold(_vehicle_boss_threashold_limit);
            _mafia_boss_threashold = new Threashold(_mafia_boss_threashold_limit);

            _ufo_enemy_threashold = new Threashold(_ufo_enemy_threashold_limit);

            ToggleHudVisibility(Visibility.Collapsed);

            _random = new Random();

            _audioStub = new AudioStub(
                (SoundType.GAME_BACKGROUND_MUSIC, 0.5, true),
                (SoundType.BOSS_BACKGROUND_MUSIC, 0.5, true),
                (SoundType.AMBIENCE, 0.6, true),
                (SoundType.GAME_START, 1, false),
                (SoundType.GAME_PAUSE, 1, false),
                (SoundType.GAME_OVER, 1, false),
                (SoundType.UFO_ENEMY_ENTRY, 1, false));

            ScreenExtensions.Width = Constants.DEFAULT_SCENE_WIDTH;
            ScreenExtensions.Height = Constants.DEFAULT_SCENE_HEIGHT;

            _scene_main_menu.SetRenderTransformOrigin(0.5);

            SetSceneScaling();

            Loaded += HonkBomberPage_Loaded;
            Unloaded += HonkBomberPage_Unloaded;
        }

        #endregion

        #region Methods

        #region Game

        private bool PauseGame()
        {
            _audioStub.Play(SoundType.GAME_PAUSE);

            _audioStub.Pause(SoundType.AMBIENCE);

            if (AnyBossExists())
            {
                _audioStub.Pause(SoundType.BOSS_BACKGROUND_MUSIC);
            }
            else
            {
                _audioStub.Pause(SoundType.GAME_BACKGROUND_MUSIC);
            }

            ToggleHudVisibility(Visibility.Collapsed);

            _scene_game.Pause();
            _scene_main_menu.Play();

            _game_controller.DeactivateGyrometerReading();
            _game_controller.SetDefaultThumbstickPosition();

            GenerateGameStartScreen(title: "Game Paused", subTitle: "-Taking a break-");

            return true;
        }

        private void ResumeGame()
        {
            _audioStub.Resume(SoundType.AMBIENCE);

            if (AnyBossExists())
            {
                _audioStub.Resume(SoundType.BOSS_BACKGROUND_MUSIC);
            }
            else
            {
                _audioStub.Resume(SoundType.GAME_BACKGROUND_MUSIC);
            }

            ToggleHudVisibility(Visibility.Visible);

            _scene_game.Play();
            _scene_main_menu.Pause();

            _game_controller.ActivateGyrometerReading();
            _game_controller.FocusController();
        }

        private void NewGame()
        {
            LoggingExtensions.Log("New game dtarted.");

            if (_scene_game.IsInNightMode)
                ToggleNightMode(false);

            _game_level = 0;

            _audioStub.Play(SoundType.AMBIENCE, SoundType.GAME_BACKGROUND_MUSIC);

            _game_controller.Reset();

            _game_score_bar.Reset();
            _powerUp_health_bar.Reset();

            _ufo_boss_health_bar.Reset();
            _vehicle_boss_health_bar.Reset();
            _zombie_boss_health_bar.Reset();
            _mafia_boss_health_bar.Reset();

            _sound_pollution_health_bar.Reset();
            _sound_pollution_health_bar.SetMaxiumHealth(_sound_pollution_max_limit);
            _sound_pollution_health_bar.SetIcon(Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.HONK).Uri);
            _sound_pollution_health_bar.SetBarColor(color: Colors.Purple);

            _ufo_boss_threashold.Reset(_ufo_boss_threashold_limit);
            _vehicle_boss_threashold.Reset(_vehicle_boss_threashold_limit);
            _zombie_boss_threashold.Reset(_zombie_boss_threashold_limit);
            _mafia_boss_threashold.Reset(_mafia_boss_threashold_limit);

            _ufo_enemy_threashold.Reset(_ufo_enemy_threashold_limit);
            _ufo_enemy_kill_count = 0;
            _ufo_enemy_fleet_appeared = false;

            SetupSetPlayerBalloon();

            GeneratePlayerBalloon();
            RepositionConstructs();

            _scene_game.SceneState = SceneState.GAME_RUNNING;
            _scene_game.Play();

            _scene_main_menu.Pause();

            ToggleHudVisibility(Visibility.Visible);

            _game_controller.FocusController();
            _game_controller.SetDefaultThumbstickPosition();
            _game_controller.ActivateGyrometerReading();
        }

        private void SetupSetPlayerBalloon()
        {
            _player.SetPlayerTemplate(_selected_player_template); // change player template

            foreach (var honkBomb in _scene_game.Children.OfType<PlayerHonkBomb>()) // change player honk bomb template
            {
                honkBomb.SetHonkBombTemplate(_selected_player_honk_bomb_template);
            }
        }

        private void GameOver()
        {
            // if player is dead game keeps playing in the background but scene state goes to game over
            if (_player.IsDead)
            {
                _audioStub.Stop(SoundType.AMBIENCE, SoundType.GAME_BACKGROUND_MUSIC, SoundType.BOSS_BACKGROUND_MUSIC);

                if (_scene_game.Children.OfType<UfoBoss>().FirstOrDefault(x => x.IsAnimating) is UfoBoss ufoBoss)
                {
                    ufoBoss.SetWinStance();
                    ufoBoss.StopSoundLoop();
                }

                _audioStub.Play(SoundType.GAME_OVER);

                _scene_main_menu.Play();
                _scene_game.SceneState = SceneState.GAME_STOPPED;

                ToggleHudVisibility(Visibility.Collapsed);
                GenerateGameStartScreen(title: "Game Over", subTitle: $"-Score: {_game_score_bar.GetScore():0000} Level: {_game_level}-");

                _game_controller.DeactivateGyrometerReading();
            }
        }

        private void RepositionConstructs()
        {
            foreach (var construct in _scene_game.Children.OfType<Construct>()
                .Where(x => x.ConstructType is
                ConstructType.VEHICLE_ENEMY_LARGE or
                ConstructType.VEHICLE_ENEMY_SMALL or
                ConstructType.VEHICLE_BOSS or
                ConstructType.UFO_BOSS or
                ConstructType.ZOMBIE_BOSS or
                ConstructType.MAFIA_BOSS or
                ConstructType.HONK or
                ConstructType.PLAYER_ROCKET or
                ConstructType.PLAYER_ROCKET_SEEKING or
                ConstructType.PLAYER_HONK_BOMB or
                ConstructType.UFO_BOSS_ROCKET or
                ConstructType.UFO_BOSS_ROCKET_SEEKING or
                ConstructType.UFO_ENEMY or
                ConstructType.UFO_ENEMY_ROCKET or
                ConstructType.VEHICLE_BOSS_ROCKET or
                ConstructType.MAFIA_BOSS_ROCKET or
                ConstructType.MAFIA_BOSS_ROCKET_BULLS_EYE or
                ConstructType.POWERUP_PICKUP or
                ConstructType.HEALTH_PICKUP or
                ConstructType.FLOATING_NUMBER))
            {
                construct.IsAnimating = false;

                if (construct is VehicleBoss vehicleboss)
                {
                    vehicleboss.IsAttacking = false;
                    vehicleboss.Health = 0;
                }

                if (construct is UfoBoss ufoBoss)
                {
                    ufoBoss.IsAttacking = false;
                    ufoBoss.Health = 0;
                }

                if (construct is ZombieBoss zombieBoss)
                {
                    zombieBoss.IsAttacking = false;
                    zombieBoss.Health = 0;
                }

                if (construct is MafiaBoss mafiaBoss)
                {
                    mafiaBoss.IsAttacking = false;
                    mafiaBoss.Health = 0;
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

        private void LevelUp()
        {
            _game_level++;
            GenerateInterimScreen($"LEVEL {_game_level} COMPLETE");
        }

        private void ToggleNightMode(bool isNightMode)
        {
            _scene_game.ToggleNightMode(isNightMode);

            if (_scene_game.IsInNightMode)
            {
                this.NightToDayStoryboard.Stop();
                this.DayToNightStoryboard.Begin();
            }
            else
            {
                this.DayToNightStoryboard.Stop();
                this.NightToDayStoryboard.Begin();
            }
        }

        private async Task OpenGame()
        {
            _scene_game.Play();
            _scene_main_menu.Play();

            ToggleNightMode(false);

            await Task.Delay(500);

            GenerateGameStartScreen(title: "Honk Trooper", subTitle: "-Stop Honkers, Save The City-");

            _audioStub.Play(SoundType.GAME_BACKGROUND_MUSIC);
        }

        #endregion

        #region Screen

        #region PromptOrientationChangeScreen

        private void SpawnPromptOrientationChangeScreen()
        {
            PromptOrientationChangeScreen promptOrientationChangeScreen = null;

            promptOrientationChangeScreen = new(
                animateAction: AnimatePromptOrientationChangeScreen,
                recycleAction: (se) => { });

            promptOrientationChangeScreen.SetZ(z: 10);

            _scene_main_menu.AddToScene(promptOrientationChangeScreen);
        }

        private void GeneratePromptOrientationChangeScreen()
        {
            if (_scene_main_menu.Children.OfType<PromptOrientationChangeScreen>().FirstOrDefault(x => x.IsAnimating == false) is PromptOrientationChangeScreen promptOrientationChangeScreen)
            {
                promptOrientationChangeScreen.Reposition();
                promptOrientationChangeScreen.IsAnimating = true;                
            }
        }

        private void AnimatePromptOrientationChangeScreen(Construct promptOrientationChangeScreen)
        {
            PromptOrientationChangeScreen screen1 = promptOrientationChangeScreen as PromptOrientationChangeScreen;
            screen1.Hover();
        }

        private void RecyclePromptOrientationChangeScreen(PromptOrientationChangeScreen promptOrientationChangeScreen)
        {
            promptOrientationChangeScreen.IsAnimating = false;
        }

        #endregion

        #region AssetsLoadingScreen

        private void SpawnAssetsLoadingScreen()
        {
            AssetsLoadingScreen assetsLoadingScreen = null;

            assetsLoadingScreen = new(
                animateAction: AnimateAssetsLoadingScreen,
                recycleAction: (se) => { });

            assetsLoadingScreen.SetZ(z: 10);

            _scene_main_menu.AddToScene(assetsLoadingScreen);
        }

        private void GenerateAssetsLoadingScreen()
        {
            if (_scene_main_menu.Children.OfType<AssetsLoadingScreen>().FirstOrDefault(x => x.IsAnimating == false) is AssetsLoadingScreen assetsLoadingScreen)
            {
                assetsLoadingScreen.Reposition();                
                assetsLoadingScreen.SetSubTitle($"... Loading Assets ...");
                assetsLoadingScreen.IsAnimating = true;                

                _ = assetsLoadingScreen.PreloadAssets(async () =>
                {
                    RecycleAssetsLoadingScreen(assetsLoadingScreen);

                    if (ScreenExtensions.IsScreenInRequiredOrientation())
                    {
                        if (!_scene_game.GeneratorsExist)
                        {
                            PrepareGameScene();
                            await Task.Delay(500);
                            await OpenGame();
                        }
                        else
                        {
                            await OpenGame();
                        }
                    }
                    else
                    {
                        GeneratePromptOrientationChangeScreen();
                    }
                });
            }
        }

        private void AnimateAssetsLoadingScreen(Construct assetsLoadingScreen)
        {
            AssetsLoadingScreen screen1 = assetsLoadingScreen as AssetsLoadingScreen;
            screen1.Hover();
        }

        private void RecycleAssetsLoadingScreen(AssetsLoadingScreen assetsLoadingScreen)
        {
            assetsLoadingScreen.IsAnimating = false;
        }

        #endregion

        #region GameStartScreen

        private void SpawnGameStartScreen()
        {
            GameStartScreen gameStartScreen = null;

            gameStartScreen = new(
                animateAction: AnimateGameStartScreen,
                recycleAction: (se) => { },
                playAction: () =>
                {
                    if (_scene_game.SceneState == SceneState.GAME_STOPPED)
                    {
                        if (ScreenExtensions.IsScreenInRequiredOrientation())
                        {
                            RecycleGameStartScreen(gameStartScreen);
                            GeneratePlayerCharacterSelectionScreen();
                            ScreenExtensions.EnterFullScreen(true);
                        }
                        else
                        {
                            ScreenExtensions.ChangeDisplayOrientationAsRequired();
                        }
                    }
                    else
                    {
                        if (!_scene_game.IsAnimating)
                        {
                            if (ScreenExtensions.IsScreenInRequiredOrientation())
                            {
                                ResumeGame();
                                RecycleGameStartScreen(gameStartScreen);
                            }
                            else
                            {
                                ScreenExtensions.ChangeDisplayOrientationAsRequired();
                            }
                        }
                    }
                });

            gameStartScreen.SetZ(z: 10);

            _scene_main_menu.AddToScene(gameStartScreen);
        }

        private void GenerateGameStartScreen(string title, string subTitle = "")
        {
            if (_scene_main_menu.Children.OfType<GameStartScreen>().FirstOrDefault(x => x.IsAnimating == false) is GameStartScreen gameStartScreen)
            {
                gameStartScreen.SetTitle(title);
                gameStartScreen.SetSubTitle(subTitle);
                gameStartScreen.Reposition();
                gameStartScreen.Reset();
                gameStartScreen.IsAnimating = true;
                gameStartScreen.SetContent(ConstructExtensions.GetRandomContentUri(Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.PLAYER_BALLOON).Select(x => x.Uri).ToArray()));
            }
        }

        private void AnimateGameStartScreen(Construct gameStartScreen)
        {
            GameStartScreen screen1 = gameStartScreen as GameStartScreen;
            screen1.Hover();
        }

        private void RecycleGameStartScreen(GameStartScreen gameStartScreen)
        {
            gameStartScreen.IsAnimating = false;
        }

        #endregion

        #region PlayerCharacterSelectionScreen

        private void SpawnPlayerCharacterSelectionScreen()
        {
            PlayerCharacterSelectionScreen playerCharacterSelectionScreen = null;

            playerCharacterSelectionScreen = new(
                animateAction: AnimatePlayerCharacterSelectionScreen,
                recycleAction: (se) => { },
                playAction: (int playerTemplate) =>
                {
                    _selected_player_template = (PlayerBalloonTemplate)playerTemplate;

                    RecyclePlayerCharacterSelectionScreen(playerCharacterSelectionScreen);
                    GeneratePlayerHonkBombSelectionScreen();
                },
                backAction: () =>
                {
                    RecyclePlayerCharacterSelectionScreen(playerCharacterSelectionScreen);
                    GenerateGameStartScreen(title: "Honk Trooper", subTitle: "-Stop Honkers, Save The City-");
                });

            playerCharacterSelectionScreen.SetZ(z: 10);

            _scene_main_menu.AddToScene(playerCharacterSelectionScreen);
        }

        private void GeneratePlayerCharacterSelectionScreen()
        {
            if (_scene_main_menu.Children.OfType<PlayerCharacterSelectionScreen>().FirstOrDefault(x => x.IsAnimating == false) is PlayerCharacterSelectionScreen playerCharacterSelectionScreen)
            {
                //playerCharacterSelectionScreen.Reset();
                playerCharacterSelectionScreen.Reposition();
                playerCharacterSelectionScreen.IsAnimating = true;
            }
        }

        private void AnimatePlayerCharacterSelectionScreen(Construct playerCharacterSelectionScreen)
        {
            PlayerCharacterSelectionScreen screen1 = playerCharacterSelectionScreen as PlayerCharacterSelectionScreen;
            screen1.Hover();
        }

        private void RecyclePlayerCharacterSelectionScreen(PlayerCharacterSelectionScreen playerCharacterSelectionScreen)
        {
            playerCharacterSelectionScreen.IsAnimating = false;
        }

        #endregion

        #region PlayerHonkBombSelectionScreen

        private void SpawnPlayerHonkBombSelectionScreen()
        {
            PlayerHonkBombSelectionScreen playerHonkBombSelectionScreen = null;

            playerHonkBombSelectionScreen = new(
                animateAction: AnimatePlayerHonkBombSelectionScreen,
                recycleAction: (se) => { },
                playAction: (int playerTemplate) =>
                {
                    _selected_player_honk_bomb_template = (PlayerHonkBombTemplate)playerTemplate;

                    if (_scene_game.SceneState == SceneState.GAME_STOPPED)
                    {
                        RecyclePlayerHonkBombSelectionScreen(playerHonkBombSelectionScreen);
                        NewGame();
                    }
                },
                backAction: () =>
                {
                    RecyclePlayerHonkBombSelectionScreen(playerHonkBombSelectionScreen);
                    GeneratePlayerCharacterSelectionScreen();
                });

            playerHonkBombSelectionScreen.SetZ(z: 10);

            _scene_main_menu.AddToScene(playerHonkBombSelectionScreen);
        }

        private void GeneratePlayerHonkBombSelectionScreen()
        {
            if (_scene_main_menu.Children.OfType<PlayerHonkBombSelectionScreen>().FirstOrDefault(x => x.IsAnimating == false) is PlayerHonkBombSelectionScreen playerHonkBombSelectionScreen)
            {
                //playerHonkBombSelectionScreen.Reset();
                playerHonkBombSelectionScreen.Reposition();
                playerHonkBombSelectionScreen.IsAnimating = true;
            }
        }

        private void AnimatePlayerHonkBombSelectionScreen(Construct playerHonkBombSelectionScreen)
        {
            PlayerHonkBombSelectionScreen screen1 = playerHonkBombSelectionScreen as PlayerHonkBombSelectionScreen;
            screen1.Hover();
        }

        private void RecyclePlayerHonkBombSelectionScreen(PlayerHonkBombSelectionScreen playerHonkBombSelectionScreen)
        {
            playerHonkBombSelectionScreen.IsAnimating = false;
        }

        #endregion

        #region InterimScreen

        private void SpawnInterimScreen()
        {
            InterimScreen interimScreen = null;

            interimScreen = new(
                animateAction: AnimateInterimScreen,
                recycleAction: RecycleInterimScreen);

            interimScreen.SetZ(z: 10);

            _scene_main_menu.AddToScene(interimScreen);
        }

        private void GenerateInterimScreen(string title)
        {
            if (_scene_main_menu.Children.OfType<InterimScreen>().FirstOrDefault(x => x.IsAnimating == false) is InterimScreen interimScreen)
            {
                interimScreen.SetTitle(title);
                interimScreen.Reposition();
                interimScreen.Reset();
                interimScreen.IsAnimating = true;

                _scene_main_menu.Play();
            }
        }

        private void AnimateInterimScreen(Construct interimScreen)
        {
            InterimScreen screen1 = interimScreen as InterimScreen;
            screen1.Hover();
            screen1.DepleteOnScreenDelay();
        }

        private void RecycleInterimScreen(Construct interimScreen)
        {
            if (interimScreen is InterimScreen interimScreen1 && interimScreen1.IsDepleted)
            {
                interimScreen.IsAnimating = false;

                _scene_main_menu.Pause();
            }
        }

        #endregion 

        #endregion

        #region Player

        #region PlayerBalloon

        private void SpawnPlayerBalloon()
        {
            var playerTemplate = _random.Next(1, 3);

            _player = new(
                animateAction: AnimatePlayerBalloon,
                recycleAction: (_player) => { });

            _player.SetZ(z: 7);

            SpawnDropShadow(source: _player);

            _scene_game.AddToScene(_player);

            LoggingExtensions.Log($"Player Template: {playerTemplate}");
        }

        private void GeneratePlayerBalloon()
        {
            _player.Reset();
            _player.Reposition();
            _player.IsAnimating = true;

            switch (_selected_player_template)
            {
                case PlayerBalloonTemplate.Blue:
                    {
                        _game_controller.SetAttackButtonColor(Application.Current.Resources["PlayerBlueAccentColor"] as SolidColorBrush);
                        _game_controller.SetThumbstickThumbColor(Application.Current.Resources["PlayerBlueAccentColor"] as SolidColorBrush);
                    }
                    break;
                case PlayerBalloonTemplate.Red:
                    {
                        _game_controller.SetAttackButtonColor(Application.Current.Resources["PlayerRedAccentColor"] as SolidColorBrush);
                        _game_controller.SetThumbstickThumbColor(Application.Current.Resources["PlayerRedAccentColor"] as SolidColorBrush);
                    }
                    break;
                default:
                    break;
            }

            GenerateDropShadow(source: _player);
            SetPlayerHealthBar();
        }

        private void SetPlayerHealthBar()
        {
            _player_health_bar.SetMaxiumHealth(_player.Health);
            _player_health_bar.SetValue(_player.Health);

            _player_health_bar.SetIcon(Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.HEALTH_PICKUP).Uri);
            _player_health_bar.SetBarColor(color: Colors.Crimson);
        }

        private void AnimatePlayerBalloon(Construct player)
        {
            _player.Pop();
            _player.Hover();
            _player.DepleteAttackStance();
            _player.DepleteWinStance();
            _player.DepleteHitStance();
            _player.RecoverFromHealthLoss();

            if (_scene_game.SceneState == SceneState.GAME_RUNNING)
            {
                if (_game_controller.IsPausing)
                {
                    PauseGame();
                    _game_controller.IsPausing = false;
                }
                else
                {
                    var count = _scene_game.Children.OfType<VehicleEnemy>().Count(x => x.IsAnimating && x.WillHonk) + _scene_game.Children.OfType<UfoEnemy>().Count(x => x.IsAnimating && x.WillHonk);
                    _sound_pollution_health_bar.SetValue(count * 2);

                    if (_sound_pollution_health_bar.GetValue() >= _sound_pollution_health_bar.GetMaxiumHealth()) // loose score slowly if sound pollution has reached the limit
                    {
                        _game_score_bar.LooseScore(0.01);
                    }

                    var scaling = ScreenExtensions.GetScreenSpaceScaling();
                    var speed = _player.Speed;

                    _player.Move(
                        speed: speed,
                        sceneWidth: Constants.DEFAULT_SCENE_WIDTH * scaling,
                        sceneHeight: Constants.DEFAULT_SCENE_HEIGHT * scaling,
                        controller: _game_controller);

                    if (_game_controller.IsAttacking)
                    {
                        if (UfoEnemyExists() || AnyInAirBossExists())
                        {
                            if (_powerUp_health_bar.HasHealth)
                            {
                                switch ((PowerUpType)_powerUp_health_bar.Tag)
                                {
                                    case PowerUpType.SEEKING_SNITCH:
                                        {
                                            GeneratePlayerRocketSeeking();
                                        }
                                        break;
                                    case PowerUpType.BULLS_EYE:
                                        {
                                            GeneratePlayerRocketBullsEye();
                                        }
                                        break;
                                    case PowerUpType.ARMOR:
                                        {
                                            GeneratePlayerRocket();
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                GeneratePlayerRocket();
                            }
                        }
                        else
                        {
                            GeneratePlayerHonkBomb();
                        }

                        _game_controller.DeactivateAttack();
                    }
                }
            }
        }

        private void LoosePlayerHealth()
        {
            _player.SetPopping();

            if (_powerUp_health_bar.HasHealth && (PowerUpType)_powerUp_health_bar.Tag == PowerUpType.ARMOR)
            {
                DepletePowerUp();
                GenerateFloatingNumber(_player);
            }
            else
            {
                _player.LooseHealth();
                _player.SetHitStance();

                GenerateFloatingNumber(_player);

                _player_health_bar.SetValue(_player.Health);

                if (_scene_game.Children.OfType<Construct>().FirstOrDefault(x => x.IsAnimating &&
                    (x.ConstructType == ConstructType.UFO_BOSS || x.ConstructType == ConstructType.ZOMBIE_BOSS || x.ConstructType == ConstructType.MAFIA_BOSS)) is Construct boss)
                {
                    if (boss is UfoBoss ufo)
                    {
                        ufo.SetWinStance();
                    }
                    else if (boss is ZombieBoss zombie)
                    {
                        zombie.SetWinStance();
                    }
                    else if (boss is MafiaBoss mafia)
                    {
                        mafia.SetWinStance();
                    }
                }

                GameOver();
            }
        }

        #endregion

        #region PlayerHonkBomb

        private void SpawnPlayerHonkBombs()
        {
            for (int i = 0; i < 3; i++)
            {
                PlayerHonkBomb playerHonkBomb = new(
                    animateAction: AnimatePlayerHonkBomb,
                    recycleAction: RecyclePlayerHonkBomb);

                playerHonkBomb.SetZ(z: 7);

                _scene_game.AddToScene(playerHonkBomb);

                SpawnDropShadow(source: playerHonkBomb);
            }
        }

        private void GeneratePlayerHonkBomb()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !_scene_game.IsSlowMotionActivated)
            {
                if ((VehicleBossExists() || _scene_game.Children.OfType<VehicleEnemy>().Any(x => x.IsAnimating)) &&
                    _scene_game.Children.OfType<PlayerHonkBomb>().FirstOrDefault(x => x.IsAnimating == false) is PlayerHonkBomb playerHonkBomb)
                {
                    _player.SetAttackStance();

                    playerHonkBomb.Reset();
                    playerHonkBomb.IsGravitatingDownwards = true;
                    playerHonkBomb.Reposition(player: _player);
                    playerHonkBomb.IsAnimating = true;

                    GenerateDropShadow(source: playerHonkBomb);
                }
                else
                {
                    _player.SetWinStance();
                }
            }
        }

        private void AnimatePlayerHonkBomb(Construct playerHonkBomb)
        {
            PlayerHonkBomb playerHonkBomb1 = playerHonkBomb as PlayerHonkBomb;

            var speed = playerHonkBomb1.Speed;

            if (playerHonkBomb1.IsBlasting)
            {
                playerHonkBomb.Expand();
                playerHonkBomb.Fade(Constants.DEFAULT_BLAST_FADE_SCALE);
                playerHonkBomb1.MoveDownRight(speed);
            }
            else
            {
                //playerHonkBomb.Pop();
                playerHonkBomb.SetLeft(playerHonkBomb.GetLeft() + speed);
                playerHonkBomb.SetTop(playerHonkBomb.GetTop() + speed * 1.2);

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    DropShadow dropShadow = _scene_game.Children.OfType<DropShadow>().First(x => x.Id == playerHonkBomb.Id);

                    var drpShdwHitBox = dropShadow.GetCloseHitBox();
                    var fireCrackerHitBox = playerHonkBomb.GetCloseHitBox();

                    if (drpShdwHitBox.IntersectsWith(fireCrackerHitBox) && playerHonkBomb.GetBottom() > dropShadow.GetBottom())  // start blast animation when the bomb touches it's shadow
                    {
                        if (_scene_game.Children.OfType<VehicleEnemy>()
                            .Where(x => x.IsAnimating /*&& x.WillHonk*/)
                            .FirstOrDefault(x => x.GetCloseHitBox().IntersectsWith(fireCrackerHitBox)) is VehicleEnemy vehicleEnemy) // while in blast check if it intersects with any vehicle, if it does then the vehicle stops honking and slows down
                        {
                            LooseVehicleEnemyHealth(vehicleEnemy);
                        }

                        if (_scene_game.Children.OfType<VehicleBoss>()
                            .FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is VehicleBoss vehicleBoss && vehicleBoss.GetCloseHitBox().IntersectsWith(fireCrackerHitBox)) // if a vechile boss is in place then boss looses health
                        {
                            LooseVehicleBossHealth(vehicleBoss);
                        }

                        playerHonkBomb1.SetBlast();

                        dropShadow.IsAnimating = false;
                        dropShadow.SetPosition(-3000, -3000);
                    }
                }
            }
        }

        private void RecyclePlayerHonkBomb(Construct playerHonkBomb)
        {
            if (playerHonkBomb.IsFadingComplete)
            {
                playerHonkBomb.IsAnimating = false;
                playerHonkBomb.IsGravitatingDownwards = false;
                playerHonkBomb.SetPosition(left: -3000, top: -3000);
            }
        }

        #endregion

        #region PlayerRocket

        private void SpawnPlayerRockets()
        {
            for (int i = 0; i < 3; i++)
            {
                PlayerRocket playerRocket = new(
                    animateAction: AnimatePlayerRocket,
                    recycleAction: RecyclePlayerRocket);

                playerRocket.SetZ(z: 8);

                _scene_game.AddToScene(playerRocket);

                SpawnDropShadow(source: playerRocket);
            }
        }

        private void GeneratePlayerRocket()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !_scene_game.IsSlowMotionActivated &&
                _scene_game.Children.OfType<PlayerRocket>().FirstOrDefault(x => x.IsAnimating == false) is PlayerRocket playerRocket)
            {
                _player.SetAttackStance();

                playerRocket.Reset();
                playerRocket.SetPopping();
                playerRocket.Reposition(player: _player);
                playerRocket.IsAnimating = true;

                GenerateDropShadow(source: playerRocket);

                var playerDistantHitBox = _player.GetDistantHitBox();

                // get closest possible target
                UfoBossRocketSeeking ufoBossRocketSeeking = _scene_game.Children.OfType<UfoBossRocketSeeking>()?.FirstOrDefault(x => x.IsAnimating && !x.IsBlasting && x.GetHitBox().IntersectsWith(playerDistantHitBox));

                UfoBoss ufoBoss = _scene_game.Children.OfType<UfoBoss>()?.FirstOrDefault(x => x.IsAnimating && x.IsAttacking && x.GetHitBox().IntersectsWith(playerDistantHitBox));
                ZombieBoss zombieBoss = _scene_game.Children.OfType<ZombieBoss>()?.FirstOrDefault(x => x.IsAnimating && x.IsAttacking && x.GetHitBox().IntersectsWith(playerDistantHitBox));
                MafiaBoss mafiaBoss = _scene_game.Children.OfType<MafiaBoss>()?.FirstOrDefault(x => x.IsAnimating && x.IsAttacking && x.GetHitBox().IntersectsWith(playerDistantHitBox));

                UfoEnemy ufoEnemy = _scene_game.Children.OfType<UfoEnemy>()?.FirstOrDefault(x => x.IsAnimating && !x.IsFadingComplete && x.GetHitBox().IntersectsWith(playerDistantHitBox));

                // if not found then find random target
                ufoBossRocketSeeking ??= _scene_game.Children.OfType<UfoBossRocketSeeking>().FirstOrDefault(x => x.IsAnimating && !x.IsBlasting);
                ufoBoss ??= _scene_game.Children.OfType<UfoBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking);
                zombieBoss ??= _scene_game.Children.OfType<ZombieBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking);
                mafiaBoss ??= _scene_game.Children.OfType<MafiaBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking);

                ufoEnemy ??= _scene_game.Children.OfType<UfoEnemy>().FirstOrDefault(x => x.IsAnimating && !x.IsFadingComplete);

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
                else if (mafiaBoss is not null)
                {
                    SetPlayerRocketDirection(source: _player, rocket: playerRocket, rocketTarget: mafiaBoss);
                }
            }
        }

        private void AnimatePlayerRocket(Construct playerRocket)
        {
            PlayerRocket playerRocket1 = playerRocket as PlayerRocket;

            var speed = playerRocket1.Speed;

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
                playerRocket.Fade(Constants.DEFAULT_BLAST_FADE_SCALE);
            }
            else
            {
                playerRocket.Pop();
                playerRocket1.Hover();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    var hitBox = playerRocket.GetCloseHitBox();

                    if (_scene_game.Children.OfType<UfoBossRocketSeeking>().FirstOrDefault(x => x.IsAnimating && !x.IsBlasting && x.GetCloseHitBox().IntersectsWith(hitBox)) is UfoBossRocketSeeking ufoBossRocketSeeking) // if player bomb touches UfoBoss's seeking bomb, it blasts
                    {
                        playerRocket1.SetBlast();
                        ufoBossRocketSeeking.SetBlast();
                    }
                    else if (_scene_game.Children.OfType<ZombieBossRocketBlock>().FirstOrDefault(x => x.IsAnimating && !x.IsBlasting && x.GetCloseHitBox().IntersectsWith(hitBox)) is ZombieBossRocketBlock zombieBossRocket) // if player bomb touches ZombieBoss's seeking bomb, it blasts
                    {
                        playerRocket1.SetBlast();
                        LooseZombieBossRocketBlockHealth(zombieBossRocket);
                    }
                    else if (_scene_game.Children.OfType<UfoBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking && x.GetCloseHitBox().IntersectsWith(hitBox)) is UfoBoss ufoBoss) // if player bomb touches UfoBoss, it blasts, UfoBoss looses health
                    {
                        playerRocket1.SetBlast();
                        LooseUfoBossHealth(ufoBoss);
                    }
                    else if (_scene_game.Children.OfType<ZombieBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking && x.GetCloseHitBox().IntersectsWith(hitBox)) is ZombieBoss zombieBoss) // if player bomb touches ZombieBoss, it blasts, ZombieBoss looses health
                    {
                        playerRocket1.SetBlast();
                        LooseZombieBossHealth(zombieBoss);
                    }
                    else if (_scene_game.Children.OfType<MafiaBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking && x.GetCloseHitBox().IntersectsWith(hitBox)) is MafiaBoss mafiaBoss) // if player bomb touches MafiaBoss, it blasts, MafiaBoss looses health
                    {
                        playerRocket1.SetBlast();
                        LooseMafiaBossHealth(mafiaBoss);
                    }
                    else if (_scene_game.Children.OfType<UfoEnemy>().FirstOrDefault(x => x.IsAnimating && !x.IsDead && x.GetCloseHitBox().IntersectsWith(hitBox)) is UfoEnemy ufoEnemy) // if player bomb touches enemy, it blasts, enemy looses health
                    {
                        playerRocket1.SetBlast();
                        LooseUfoEnemyHealth(ufoEnemy);
                    }

                    if (playerRocket1.AutoBlast())
                        playerRocket1.SetBlast();
                }
            }
        }

        private void RecyclePlayerRocket(Construct playerRocket)
        {
            var hitbox = playerRocket.GetHitBox();

            // if bomb is blasted or goes out of scene bounds
            if (playerRocket.IsFadingComplete || hitbox.Left > Constants.DEFAULT_SCENE_WIDTH || hitbox.Right < 0 || hitbox.Bottom < 0 || hitbox.Top > Constants.DEFAULT_SCENE_HEIGHT)
            {
                playerRocket.IsAnimating = false;
            }
        }

        #endregion

        #region PlayerRocketSeeking

        private void SpawnPlayerRocketSeekings()
        {
            for (int i = 0; i < 3; i++)
            {
                PlayerRocketSeeking playerRocketSeeking = new(
                    animateAction: AnimatePlayerRocketSeeking,
                    recycleAction: RecyclePlayerRocketSeeking);

                playerRocketSeeking.SetZ(z: 7);

                _scene_game.AddToScene(playerRocketSeeking);

                SpawnDropShadow(source: playerRocketSeeking);
            }
        }

        private void GeneratePlayerRocketSeeking()
        {
            // generate a seeking bomb if one is not in scene

            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !_scene_game.IsSlowMotionActivated &&
                _scene_game.Children.OfType<PlayerRocketSeeking>().FirstOrDefault(x => x.IsAnimating == false) is PlayerRocketSeeking playerRocketSeeking)
            {
                _player.SetAttackStance();

                playerRocketSeeking.Reset();
                playerRocketSeeking.SetPopping();
                playerRocketSeeking.Reposition(player: _player);
                playerRocketSeeking.IsAnimating = true;

                GenerateDropShadow(source: playerRocketSeeking);

                if (_powerUp_health_bar.HasHealth && (PowerUpType)_powerUp_health_bar.Tag == PowerUpType.SEEKING_SNITCH)
                    DepletePowerUp();
            }
        }

        private void AnimatePlayerRocketSeeking(Construct playerRocketSeeking)
        {
            PlayerRocketSeeking playerRocketSeeking1 = playerRocketSeeking as PlayerRocketSeeking;

            if (playerRocketSeeking1.IsBlasting)
            {
                var speed = playerRocketSeeking1.Speed;
                playerRocketSeeking1.MoveDownRight(speed);
                playerRocketSeeking.Expand();
                playerRocketSeeking.Fade(Constants.DEFAULT_BLAST_FADE_SCALE);
            }
            else
            {
                playerRocketSeeking.Pop();
                playerRocketSeeking.Rotate(rotationSpeed: 3.5);

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (_scene_game.Children.OfType<UfoBossRocketSeeking>().FirstOrDefault(x => x.IsAnimating && !x.IsBlasting) is UfoBossRocketSeeking ufoBossRocketSeeking) // target UfoBossRocketSeeking
                    {
                        playerRocketSeeking1.Seek(ufoBossRocketSeeking.GetCloseHitBox());

                        if (playerRocketSeeking1.GetCloseHitBox().IntersectsWith(ufoBossRocketSeeking.GetCloseHitBox()))
                        {
                            playerRocketSeeking1.SetBlast();
                            ufoBossRocketSeeking.SetBlast();
                        }
                    }
                    else if (_scene_game.Children.OfType<ZombieBossRocketBlock>().FirstOrDefault(x => x.IsAnimating) is ZombieBossRocketBlock zombieBossRocket) // target ZombieBossRocketBlock
                    {
                        playerRocketSeeking1.Seek(zombieBossRocket.GetCloseHitBox());

                        if (playerRocketSeeking1.GetCloseHitBox().IntersectsWith(zombieBossRocket.GetCloseHitBox()))
                        {
                            playerRocketSeeking1.SetBlast();
                            zombieBossRocket.LooseHealth();
                        }
                    }
                    else if (_scene_game.Children.OfType<UfoBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is UfoBoss ufoBoss) // target UfoBoss
                    {
                        playerRocketSeeking1.Seek(ufoBoss.GetCloseHitBox());

                        if (playerRocketSeeking1.GetCloseHitBox().IntersectsWith(ufoBoss.GetCloseHitBox()))
                        {
                            playerRocketSeeking1.SetBlast();
                            LooseUfoBossHealth(ufoBoss);
                        }
                    }
                    else if (_scene_game.Children.OfType<ZombieBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is ZombieBoss ZombieBoss) // target ZombieBoss
                    {
                        playerRocketSeeking1.Seek(ZombieBoss.GetCloseHitBox());

                        if (playerRocketSeeking1.GetCloseHitBox().IntersectsWith(ZombieBoss.GetCloseHitBox()))
                        {
                            playerRocketSeeking1.SetBlast();
                            LooseZombieBossHealth(ZombieBoss);
                        }
                    }
                    else if (_scene_game.Children.OfType<MafiaBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is MafiaBoss MafiaBoss) // target MafiaBoss
                    {
                        playerRocketSeeking1.Seek(MafiaBoss.GetCloseHitBox());

                        if (playerRocketSeeking1.GetCloseHitBox().IntersectsWith(MafiaBoss.GetCloseHitBox()))
                        {
                            playerRocketSeeking1.SetBlast();
                            LooseMafiaBossHealth(MafiaBoss);
                        }
                    }
                    else if (_scene_game.Children.OfType<UfoEnemy>().FirstOrDefault(x => x.IsAnimating && !x.IsFadingComplete) is UfoEnemy enemy) // target UfoEnemy
                    {
                        playerRocketSeeking1.Seek(enemy.GetCloseHitBox());

                        if (playerRocketSeeking1.GetCloseHitBox().IntersectsWith(enemy.GetCloseHitBox()))
                        {
                            playerRocketSeeking1.SetBlast();
                            LooseUfoEnemyHealth(enemy);
                        }
                    }

                    if (playerRocketSeeking1.AutoBlast())
                        playerRocketSeeking1.SetBlast();
                }
            }
        }

        private void RecyclePlayerRocketSeeking(Construct playerRocketSeeking)
        {
            var hitbox = playerRocketSeeking.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (playerRocketSeeking.IsFadingComplete || hitbox.Left > Constants.DEFAULT_SCENE_WIDTH || hitbox.Right < 0 || hitbox.Bottom < 0 || hitbox.Bottom > Constants.DEFAULT_SCENE_HEIGHT)
            {
                playerRocketSeeking.IsAnimating = false;
            }
        }

        #endregion

        #region PlayerRocketBullsEye

        private void SpawnPlayerRocketBullsEyes()
        {
            for (int i = 0; i < 3; i++)
            {
                PlayerRocketBullsEye playerRocketBullsEye = new(
                    animateAction: AnimatePlayerRocketBullsEye,
                    recycleAction: RecyclePlayerRocketBullsEye);

                playerRocketBullsEye.SetZ(z: 7);

                _scene_game.AddToScene(playerRocketBullsEye);

                SpawnDropShadow(source: playerRocketBullsEye);
            }
        }

        private void GeneratePlayerRocketBullsEye()
        {
            // generate a bulls eye bomb if one is not in scene

            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !_scene_game.IsSlowMotionActivated &&
                _scene_game.Children.OfType<PlayerRocketBullsEye>().FirstOrDefault(x => x.IsAnimating == false) is PlayerRocketBullsEye playerRocketBullsEye)
            {
                _player.SetAttackStance();

                playerRocketBullsEye.Reset();
                playerRocketBullsEye.SetPopping();
                playerRocketBullsEye.Reposition(player: _player);
                playerRocketBullsEye.IsAnimating = true;

                GenerateDropShadow(source: playerRocketBullsEye);

                var playerDistantHitBox = _player.GetDistantHitBox();

                // get closest possible target
                UfoBossRocketSeeking ufoBossRocketSeeking = _scene_game.Children.OfType<UfoBossRocketSeeking>()?.FirstOrDefault(x => x.IsAnimating && !x.IsBlasting && x.GetHitBox().IntersectsWith(playerDistantHitBox));

                UfoBoss ufoBoss = _scene_game.Children.OfType<UfoBoss>()?.FirstOrDefault(x => x.IsAnimating && x.IsAttacking && x.GetHitBox().IntersectsWith(playerDistantHitBox));
                ZombieBoss zombieBoss = _scene_game.Children.OfType<ZombieBoss>()?.FirstOrDefault(x => x.IsAnimating && x.IsAttacking && x.GetHitBox().IntersectsWith(playerDistantHitBox));
                MafiaBoss mafiaBoss = _scene_game.Children.OfType<MafiaBoss>()?.FirstOrDefault(x => x.IsAnimating && x.IsAttacking && x.GetHitBox().IntersectsWith(playerDistantHitBox));

                UfoEnemy ufoEnemy = _scene_game.Children.OfType<UfoEnemy>()?.FirstOrDefault(x => x.IsAnimating && !x.IsFadingComplete && x.GetHitBox().IntersectsWith(playerDistantHitBox));

                // if not found then find random target
                ufoBossRocketSeeking ??= _scene_game.Children.OfType<UfoBossRocketSeeking>().FirstOrDefault(x => x.IsAnimating && !x.IsBlasting);

                ufoBoss ??= _scene_game.Children.OfType<UfoBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking);
                zombieBoss ??= _scene_game.Children.OfType<ZombieBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking);
                mafiaBoss ??= _scene_game.Children.OfType<MafiaBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking);

                ufoEnemy ??= _scene_game.Children.OfType<UfoEnemy>().FirstOrDefault(x => x.IsAnimating && !x.IsFadingComplete);

                if (ufoEnemy is not null)
                {
                    playerRocketBullsEye.SetTarget(ufoEnemy.GetCloseHitBox());
                }
                else if (ufoBoss is not null)
                {
                    playerRocketBullsEye.SetTarget(ufoBoss.GetCloseHitBox());
                }
                else if (ufoBossRocketSeeking is not null)
                {
                    playerRocketBullsEye.SetTarget(ufoBossRocketSeeking.GetCloseHitBox());
                }
                else if (zombieBoss is not null)
                {
                    playerRocketBullsEye.SetTarget(zombieBoss.GetCloseHitBox());
                }
                else if (mafiaBoss is not null)
                {
                    playerRocketBullsEye.SetTarget(mafiaBoss.GetCloseHitBox());
                }

                if (_powerUp_health_bar.HasHealth && (PowerUpType)_powerUp_health_bar.Tag == PowerUpType.BULLS_EYE)
                    DepletePowerUp();
            }
        }

        private void AnimatePlayerRocketBullsEye(Construct playerRocketBullsEye)
        {
            PlayerRocketBullsEye playerRocketBullsEye1 = playerRocketBullsEye as PlayerRocketBullsEye;

            if (playerRocketBullsEye1.IsBlasting)
            {
                var speed = playerRocketBullsEye1.Speed;
                playerRocketBullsEye1.MoveDownRight(speed);
                playerRocketBullsEye.Expand();
                playerRocketBullsEye.Fade(Constants.DEFAULT_BLAST_FADE_SCALE);
            }
            else
            {
                playerRocketBullsEye.Pop();
                playerRocketBullsEye.Rotate(rotationSpeed: 3.5);

                if (_scene_game.SceneState == SceneState.GAME_RUNNING) // check if the rocket intersects with any target on its path
                {
                    var speed = playerRocketBullsEye1.Speed;
                    playerRocketBullsEye1.Move();

                    var hitbox = playerRocketBullsEye1.GetCloseHitBox();

                    if (_scene_game.Children.OfType<UfoBossRocketSeeking>().FirstOrDefault(x => x.IsAnimating && !x.IsBlasting && x.GetCloseHitBox().IntersectsWith(hitbox)) is UfoBossRocketSeeking ufoBossRocketSeeking) // target UfoBossRocketSeeking
                    {
                        playerRocketBullsEye1.SetBlast();
                        ufoBossRocketSeeking.SetBlast();

                    }
                    else if (_scene_game.Children.OfType<ZombieBossRocketBlock>().FirstOrDefault(x => x.IsAnimating && x.GetCloseHitBox().IntersectsWith(hitbox)) is ZombieBossRocketBlock zombieBossRocket) // target ZombieBossRocketBlock
                    {
                        playerRocketBullsEye1.SetBlast();
                        zombieBossRocket.LooseHealth();
                    }
                    else if (_scene_game.Children.OfType<UfoBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking && x.GetCloseHitBox().IntersectsWith(hitbox)) is UfoBoss ufoBoss) // target UfoBoss
                    {
                        playerRocketBullsEye1.SetBlast();
                        LooseUfoBossHealth(ufoBoss);
                    }
                    else if (_scene_game.Children.OfType<ZombieBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking && x.GetCloseHitBox().IntersectsWith(hitbox)) is ZombieBoss zombieBoss) // target ZombieBoss
                    {
                        playerRocketBullsEye1.SetBlast();
                        LooseZombieBossHealth(zombieBoss);
                    }
                    else if (_scene_game.Children.OfType<MafiaBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking && x.GetCloseHitBox().IntersectsWith(hitbox)) is MafiaBoss mafiaBoss) // target MafiaBoss
                    {
                        playerRocketBullsEye1.SetBlast();
                        LooseMafiaBossHealth(mafiaBoss);
                    }
                    else if (_scene_game.Children.OfType<UfoEnemy>().FirstOrDefault(x => x.IsAnimating && !x.IsFadingComplete && x.GetCloseHitBox().IntersectsWith(hitbox)) is UfoEnemy enemy) // target UfoEnemy
                    {
                        playerRocketBullsEye1.SetBlast();
                        LooseUfoEnemyHealth(enemy);
                    }

                    if (playerRocketBullsEye1.AutoBlast())
                        playerRocketBullsEye1.SetBlast();
                }
            }
        }

        private void RecyclePlayerRocketBullsEye(Construct playerRocketBullsEye)
        {
            var hitbox = playerRocketBullsEye.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (playerRocketBullsEye.IsFadingComplete || hitbox.Left > Constants.DEFAULT_SCENE_WIDTH || hitbox.Right < 0 || hitbox.Bottom < 0 || hitbox.Bottom > Constants.DEFAULT_SCENE_HEIGHT)
            {
                playerRocketBullsEye.IsAnimating = false;
            }
        }

        #endregion

        #endregion

        #region Road

        #region RoadSideWalk

        private void SpawnRoadSideWalks()
        {
            for (int i = 0; i < 15; i++)
            {
                RoadSideWalk roadSideWalk = new(
                animateAction: AnimateRoadSideWalk,
                recycleAction: RecycleRoadSideWalk);

                _scene_game.AddToScene(roadSideWalk);
            }
        }

        private void GenerateRoadSideWalk()
        {
            if (/*!_scene_game.IsSlowMotionActivated &&*/ _scene_game.Children.OfType<RoadSideWalk>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideWalk roadSideWalkTop)
            {
                roadSideWalkTop.Reset();
                roadSideWalkTop.SetPosition(
                    left: (Constants.DEFAULT_SCENE_WIDTH / 2.25 - roadSideWalkTop.Width),
                    top: roadSideWalkTop.Height * -1);
                roadSideWalkTop.IsAnimating = true;
            }

            if (/*!_scene_game.IsSlowMotionActivated &&*/ _scene_game.Children.OfType<RoadSideWalk>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideWalk roadSideWalkBottom)
            {
                roadSideWalkBottom.Reset();
                roadSideWalkBottom.SetPosition(
                    left: (roadSideWalkBottom.Height * -1.5) - 30,
                    top: (Constants.DEFAULT_SCENE_HEIGHT / 5 + roadSideWalkBottom.Height / 2) - 90);
                roadSideWalkBottom.IsAnimating = true;
            }
        }

        private void AnimateRoadSideWalk(Construct roadSideWalk)
        {
            RoadSideWalk roadSideWalk1 = roadSideWalk as RoadSideWalk;
            var speed = roadSideWalk1.Speed;
            roadSideWalk1.MoveDownRight(speed: speed);
        }

        private void RecycleRoadSideWalk(Construct roadSideWalk)
        {
            var hitBox = roadSideWalk.GetHitBox();

            if (hitBox.Top - 45 > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left - roadSideWalk.Width > Constants.DEFAULT_SCENE_WIDTH)
            {
                roadSideWalk.IsAnimating = false;
            }
        }

        #endregion

        #region RoadSideTree

        private void SpawnRoadSideTrees()
        {
            for (int i = 0; i < 11; i++)
            {
                RoadSideTree roadSideTree = new(
                    animateAction: AnimateRoadSideTree,
                    recycleAction: RecycleRoadSideTree);              

                _scene_game.AddToScene(roadSideTree);

                //SpawnDropShadow(source: roadSideTree);
            }
        }

        private void GenerateRoadSideTree()
        {
            if (!_scene_game.IsSlowMotionActivated && _scene_game.Children.OfType<RoadSideTree>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideTree roadSideTreeTop)
            {
                roadSideTreeTop.SetPosition(
                  left: (Constants.DEFAULT_SCENE_WIDTH / 5),
                  top: (roadSideTreeTop.Height * -1.1),
                  z: 3);
                roadSideTreeTop.IsAnimating = true;


                //GenerateDropShadow(source: roadSideTreeTop);
            }

            if (!_scene_game.IsSlowMotionActivated && _scene_game.Children.OfType<RoadSideTree>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideTree roadSideTreeBottom)
            {
                roadSideTreeBottom.SetPosition(
                  left: (roadSideTreeBottom.Width * -1.1),
                  top: (Constants.DEFAULT_SCENE_HEIGHT / 7.8),
                  z: 5);
                roadSideTreeBottom.IsAnimating = true;

                //GenerateDropShadow(source: roadSideTreeBottom);
            }
        }

        private void AnimateRoadSideTree(Construct roadSideTree)
        {
            RoadSideTree roadSideTree1 = roadSideTree as RoadSideTree;
            var speed = roadSideTree1.Speed;
            roadSideTree1.MoveDownRight(speed);
        }

        private void RecycleRoadSideTree(Construct roadSideTree)
        {
            var hitBox = roadSideTree.GetHitBox();

            if (hitBox.Top - 45 > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left - roadSideTree.Width > Constants.DEFAULT_SCENE_WIDTH)
            {
                roadSideTree.IsAnimating = false;
            }
        }

        #endregion

        #region RoadSideHedge

        private void SpawnRoadSideHedges()
        {
            for (int i = 0; i < 10; i++)
            {
                RoadSideHedge roadSideHedge = new(
                    animateAction: AnimateRoadSideHedge,
                    recycleAction: RecycleRoadSideHedge);

                _scene_game.AddToScene(roadSideHedge);
            }
        }

        private void GenerateRoadSideHedge()
        {
            if (!_scene_game.IsSlowMotionActivated && _scene_game.Children.OfType<RoadSideHedge>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideHedge roadSideHedgeTop)
            {
                roadSideHedgeTop.SetPosition(
                  left: (Constants.DEFAULT_SCENE_WIDTH / 20),
                  top: (roadSideHedgeTop.Height * -1.1),
                  z: 3);
                roadSideHedgeTop.IsAnimating = true;
            }

            if (!_scene_game.IsSlowMotionActivated && _scene_game.Children.OfType<RoadSideHedge>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideHedge roadSideHedgeBottom)
            {
                roadSideHedgeBottom.SetPosition(
                  left: (roadSideHedgeBottom.Width * -1.1),
                  top: (Constants.DEFAULT_SCENE_HEIGHT / 7.9),
                  z: 3);
                roadSideHedgeBottom.IsAnimating = true;
            }
        }

        private void AnimateRoadSideHedge(Construct roadSideHedge)
        {
            RoadSideHedge roadSideHedge1 = roadSideHedge as RoadSideHedge;
            var speed = roadSideHedge1.Speed;
            roadSideHedge1.MoveDownRight(speed);
        }

        private void RecycleRoadSideHedge(Construct roadSideHedge)
        {
            var hitBox = roadSideHedge.GetHitBox();

            if (hitBox.Top - 45 > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left - roadSideHedge.Width > Constants.DEFAULT_SCENE_WIDTH)
            {
                roadSideHedge.IsAnimating = false;
            }
        }

        #endregion

        #region RoadSideLamp

        private void SpawnRoadSideLamps()
        {
            for (int i = 0; i < 7; i++)
            {
                RoadSideLamp roadSideLamp = new(
                    animateAction: AnimateRoadSideLamp,
                    recycleAction: RecycleRoadSideLamp);

                _scene_game.AddToScene(roadSideLamp);
            }
        }

        private void GenerateRoadSideLamp()
        {
            if (!_scene_game.IsSlowMotionActivated && _scene_game.Children.OfType<RoadSideLamp>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideLamp roadSideLampTop)
            {
                roadSideLampTop.SetPosition(
                  left: (Constants.DEFAULT_SCENE_WIDTH / 3 - roadSideLampTop.Width) - 100,
                  top: ((roadSideLampTop.Height * 1.5) * -1) - 50,
                  z: 4);
                roadSideLampTop.IsAnimating = true;
            }

            if (!_scene_game.IsSlowMotionActivated && _scene_game.Children.OfType<RoadSideLamp>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideLamp roadSideLampBottom)
            {
                roadSideLampBottom.SetPosition(
                  left: (-1.9 * roadSideLampBottom.Width),
                  top: (Constants.DEFAULT_SCENE_HEIGHT / 4.3),
                  z: 5);
                roadSideLampBottom.IsAnimating = true;
            }
        }

        private void AnimateRoadSideLamp(Construct roadSideLamp)
        {
            RoadSideLamp roadSideLamp1 = roadSideLamp as RoadSideLamp;
            var speed = roadSideLamp1.Speed;
            roadSideLamp1.MoveDownRight(speed);
        }

        private void RecycleRoadSideLamp(Construct roadSideLamp)
        {
            var hitBox = roadSideLamp.GetHitBox();

            if (hitBox.Top - 45 > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left - roadSideLamp.Width > Constants.DEFAULT_SCENE_WIDTH)
            {
                roadSideLamp.IsAnimating = false;
            }
        }

        #endregion

        #region RoadSideBillboard

        private void SpawnRoadSideBillboards()
        {
            for (int i = 0; i < 3; i++)
            {
                RoadSideBillboard roadSideBillboard = new(
                    animateAction: AnimateRoadSideBillboard,
                    recycleAction: RecycleRoadSideBillboard);

                roadSideBillboard.SetZ(z: 4);

                _scene_game.AddToScene(roadSideBillboard);

                //SpawnDropShadow(source: roadSideBillboard);
            }
        }

        private void GenerateRoadSideBillboard()
        {
            if (!_scene_game.IsSlowMotionActivated && _scene_game.Children.OfType<RoadSideBillboard>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideBillboard roadSideBillboardTop)
            {
                roadSideBillboardTop.SetPosition(
                  left: (Constants.DEFAULT_SCENE_WIDTH / 3.1),
                  top: (roadSideBillboardTop.Height * -1.1));
                roadSideBillboardTop.IsAnimating = true;
            }
        }

        private void AnimateRoadSideBillboard(Construct roadSideBillboard)
        {
            RoadSideBillboard roadSideBillboard1 = roadSideBillboard as RoadSideBillboard;
            var speed = roadSideBillboard1.Speed;
            roadSideBillboard1.MoveDownRight(speed);
        }

        private void RecycleRoadSideBillboard(Construct roadSideBillboard)
        {
            var hitBox = roadSideBillboard.GetHitBox();

            if (hitBox.Top - 45 > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left - roadSideBillboard.Width > Constants.DEFAULT_SCENE_WIDTH)
            {
                roadSideBillboard.IsAnimating = false;
            }
        }

        #endregion      

        #region RoadSideLightBillboard

        private void SpawnRoadSideLightBillboards()
        {
            for (int i = 0; i < 4; i++)
            {
                RoadSideLightBillboard roadSideLight = new(
                    animateAction: AnimateRoadSideLightBillboard,
                    recycleAction: RecycleRoadSideLightBillboard);

                _scene_game.AddToScene(roadSideLight);

                //SpawnDropShadow(source: roadSideLight);
            }
        }

        private void GenerateRoadSideLightBillboard()
        {
            if (!_scene_game.IsSlowMotionActivated && _scene_game.Children.OfType<RoadSideLightBillboard>().FirstOrDefault(x => x.IsAnimating == false) is RoadSideLightBillboard roadSideLight)
            {
                roadSideLight.SetPosition(
                  left: (-3.5 * roadSideLight.Width) + 10,
                  top: (Constants.DEFAULT_SCENE_HEIGHT / 5.2) + 10,
                  z: 4);
                roadSideLight.IsAnimating = true;
            }
        }

        private void AnimateRoadSideLightBillboard(Construct roadSideLight)
        {
            RoadSideLightBillboard roadSideLight1 = roadSideLight as RoadSideLightBillboard;
            var speed = roadSideLight1.Speed;
            roadSideLight1.MoveDownRight(speed);
        }

        private void RecycleRoadSideLightBillboard(Construct roadSideLight)
        {
            var hitBox = roadSideLight.GetHitBox();

            if (hitBox.Top - 45 > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left - roadSideLight.Width > Constants.DEFAULT_SCENE_WIDTH)
            {
                roadSideLight.IsAnimating = false;
            }
        }

        #endregion        

        #region RoadMark

        private void SpawnRoadMarks()
        {
            for (int i = 0; i < 6; i++)
            {
                RoadMark roadMark = new(
                    animateAction: AnimateRoadMark,
                    recycleAction: RecycleRoadMark);

                roadMark.SetZ(z: 0);

                _scene_game.AddToScene(roadMark);
            }
        }

        private void GenerateRoadMark()
        {
            if (_scene_game.Children.OfType<RoadMark>().FirstOrDefault(x => x.IsAnimating == false) is RoadMark roadMark)
            {
                roadMark.SetPosition(
                  left: roadMark.Height * -1.5,
                  top: roadMark.Height * -1);
                roadMark.IsAnimating = true;
            }
        }

        private void AnimateRoadMark(Construct roadMark)
        {
            RoadMark roadMark1 = roadMark as RoadMark;
            var speed = roadMark1.Speed;
            roadMark1.MoveDownRight(speed);
        }

        private void RecycleRoadMark(Construct roadMark)
        {
            var hitBox = roadMark.GetHitBox();

            if (hitBox.Top > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left > Constants.DEFAULT_SCENE_WIDTH)
            {
                roadMark.IsAnimating = false;
            }
        }

        #endregion

        #endregion

        #region UfoBoss

        #region UfoBoss

        private void SpawnUfoBosses()
        {
            UfoBoss ufoBoss = new(
                animateAction: AnimateUfoBoss,
                recycleAction: RecycleUfoBoss);

            ufoBoss.SetZ(z: 8);

            _scene_game.AddToScene(ufoBoss);

            SpawnDropShadow(source: ufoBoss);
        }

        private void GenerateUfoBoss()
        {
            // if scene doesn't contain a UfoBoss then pick a UfoBoss and add to scene

            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _ufo_boss_threashold.ShouldRelease(_game_score_bar.GetScore()) && !UfoBossExists() &&
                _scene_game.Children.OfType<UfoBoss>().FirstOrDefault(x => x.IsAnimating == false) is UfoBoss ufoBoss)
            {
                _audioStub.Stop(SoundType.GAME_BACKGROUND_MUSIC);
                _audioStub.Play(SoundType.BOSS_BACKGROUND_MUSIC);
                _audioStub.SetVolume(SoundType.AMBIENCE, 0.2);

                ufoBoss.Reset();
                ufoBoss.SetPosition(
                    left: 0,
                    top: ufoBoss.Height * -1);
                ufoBoss.IsAnimating = true;

                GenerateDropShadow(source: ufoBoss);

                // set UfoBoss health
                ufoBoss.Health = _ufo_boss_threashold.GetReleasePointDifference() * 1.5;

                _ufo_boss_threashold.IncreaseThreasholdLimit(increment: _ufo_boss_threashold_limit_increase, currentPoint: _game_score_bar.GetScore());

                _ufo_boss_health_bar.SetMaxiumHealth(ufoBoss.Health);
                _ufo_boss_health_bar.SetValue(ufoBoss.Health);
                _ufo_boss_health_bar.SetIcon(ufoBoss.GetContentUri());
                _ufo_boss_health_bar.SetBarColor(color: Colors.Crimson);

                _scene_game.ActivateSlowMotion();

                GenerateInterimScreen("Beware of Scarlet Saucer");

                ToggleNightMode(true);
            }
        }

        private void AnimateUfoBoss(Construct ufoBoss)
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
                    var speed = ufoBoss1.Speed;
                    var scaling = ScreenExtensions.GetScreenSpaceScaling();

                    if (ufoBoss1.IsAttacking)
                    {
                        ufoBoss1.Move(
                            speed: speed,
                            sceneWidth: Constants.DEFAULT_SCENE_WIDTH * scaling,
                            sceneHeight: Constants.DEFAULT_SCENE_HEIGHT * scaling,
                            playerPoint: _player.GetCloseHitBox());


                        if (ufoBoss1.GetCloseHitBox().IntersectsWith(_player.GetCloseHitBox()))
                        {
                            LoosePlayerHealth();
                        }
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
        }

        private void RecycleUfoBoss(Construct ufoBoss)
        {
            if (ufoBoss.IsShrinkingComplete)
            {
                ufoBoss.IsAnimating = false;
            }
        }

        private void LooseUfoBossHealth(UfoBoss ufoBoss)
        {
            ufoBoss.SetPopping();
            ufoBoss.LooseHealth();
            ufoBoss.SetHitStance();

            GenerateFloatingNumber(ufoBoss);

            _ufo_boss_health_bar.SetValue(ufoBoss.Health);

            if (ufoBoss.IsDead)
            {
                _audioStub.Stop(SoundType.BOSS_BACKGROUND_MUSIC);
                _audioStub.Play(SoundType.GAME_BACKGROUND_MUSIC);
                _audioStub.SetVolume(SoundType.AMBIENCE, 0.6);

                _player.SetWinStance();
                _game_score_bar.GainScore(3);

                LevelUp();

                _scene_game.ActivateSlowMotion();
                ToggleNightMode(false);
            }
        }

        private bool UfoBossExists()
        {
            return _scene_game.Children.OfType<UfoBoss>().Any(x => x.IsAnimating);
        }

        #endregion

        #region UfoBossRocket

        private void SpawnUfoBossRockets()
        {
            for (int i = 0; i < 4; i++)
            {
                UfoBossRocket ufoBossRocket = new(
                    animateAction: AnimateUfoBossRocket,
                    recycleAction: RecycleUfoBossRocket);

                ufoBossRocket.SetZ(z: 7);

                _scene_game.AddToScene(ufoBossRocket);

                SpawnDropShadow(source: ufoBossRocket);
            }
        }

        private void GenerateUfoBossRocket()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<UfoBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is UfoBoss ufoBoss &&
                _scene_game.Children.OfType<UfoBossRocket>().FirstOrDefault(x => x.IsAnimating == false) is UfoBossRocket ufoBossRocket)
            {
                ufoBossRocket.Reset();
                ufoBossRocket.SetPopping();
                ufoBossRocket.Reposition(ufoBoss: ufoBoss);
                ufoBossRocket.IsAnimating = true;

                GenerateDropShadow(source: ufoBossRocket);
                SetBossRocketDirection(source: ufoBoss, rocket: ufoBossRocket, rocketTarget: _player);
            }
        }

        private void AnimateUfoBossRocket(Construct ufoBossRocket)
        {
            UfoBossRocket ufoBossRocket1 = ufoBossRocket as UfoBossRocket;

            var speed = ufoBossRocket1.Speed;

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
                ufoBossRocket.Fade(Constants.DEFAULT_BLAST_FADE_SCALE);
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
        }

        private void RecycleUfoBossRocket(Construct ufoBossRocket)
        {
            var hitbox = ufoBossRocket.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (ufoBossRocket.IsFadingComplete || hitbox.Left > Constants.DEFAULT_SCENE_WIDTH || hitbox.Right < 0 || hitbox.Bottom < 0 || hitbox.Top > Constants.DEFAULT_SCENE_HEIGHT)
            {
                ufoBossRocket.IsAnimating = false;
            }
        }

        #endregion                

        #region UfoBossRocketSeeking

        private void SpawnUfoBossRocketSeekings()
        {
            for (int i = 0; i < 2; i++)
            {
                UfoBossRocketSeeking ufoBossRocketSeeking = new(
                    animateAction: AnimateUfoBossRocketSeeking,
                    recycleAction: RecycleUfoBossRocketSeeking);

                ufoBossRocketSeeking.SetZ(z: 7);

                _scene_game.AddToScene(ufoBossRocketSeeking);

                SpawnDropShadow(source: ufoBossRocketSeeking);
            }
        }

        private void GenerateUfoBossRocketSeeking()
        {
            // generate a seeking bomb if one is not in scene
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<UfoBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is UfoBoss ufoBoss &&
                !_scene_game.Children.OfType<UfoBossRocketSeeking>().Any(x => x.IsAnimating) &&
                _scene_game.Children.OfType<UfoBossRocketSeeking>().FirstOrDefault(x => x.IsAnimating == false) is UfoBossRocketSeeking ufoBossRocketSeeking)
            {
                ufoBossRocketSeeking.Reset();
                ufoBossRocketSeeking.SetPopping();
                ufoBossRocketSeeking.Reposition(UfoBoss: ufoBoss);
                ufoBossRocketSeeking.IsAnimating = true;

                GenerateDropShadow(source: ufoBossRocketSeeking);
            }
        }

        private void AnimateUfoBossRocketSeeking(Construct ufoBossRocketSeeking)
        {
            UfoBossRocketSeeking ufoBossRocketSeeking1 = ufoBossRocketSeeking as UfoBossRocketSeeking;

            var speed = ufoBossRocketSeeking1.Speed;

            if (ufoBossRocketSeeking1.IsBlasting)
            {
                ufoBossRocketSeeking.Expand();
                ufoBossRocketSeeking.Fade(Constants.DEFAULT_BLAST_FADE_SCALE);
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
                            if (ufoBossRocketSeeking1.AutoBlast())
                                ufoBossRocketSeeking1.SetBlast();
                        }
                    }
                    else
                    {
                        ufoBossRocketSeeking1.SetBlast();
                    }
                }
            }
        }

        private void RecycleUfoBossRocketSeeking(Construct ufoBossRocketSeeking)
        {
            var hitbox = ufoBossRocketSeeking.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (ufoBossRocketSeeking.IsFadingComplete || hitbox.Left > Constants.DEFAULT_SCENE_WIDTH || hitbox.Right < 0 || hitbox.Bottom < 0 || hitbox.Bottom > Constants.DEFAULT_SCENE_HEIGHT)
            {
                ufoBossRocketSeeking.IsAnimating = false;
            }
        }

        #endregion

        #endregion

        #region UfoEnemy

        #region UfoEnemy

        private void SpawnUfoEnemys()
        {
            for (int i = 0; i < 7; i++)
            {
                UfoEnemy ufoEnemy = new(
                    animateAction: AnimateUfoEnemy,
                    recycleAction: RecycleUfoEnemy);

                _scene_game.AddToScene(ufoEnemy);

                ufoEnemy.SetZ(z: 8);

                SpawnDropShadow(source: ufoEnemy);
            }
        }

        private void GenerateUfoEnemy()
        {
            if (!AnyBossExists() &&
                _ufo_enemy_threashold.ShouldRelease(_game_score_bar.GetScore()) &&
                _scene_game.Children.OfType<UfoEnemy>().FirstOrDefault(x => x.IsAnimating == false) is UfoEnemy ufoEnemy)
            {
                ufoEnemy.Reset();
                ufoEnemy.Reposition();
                ufoEnemy.IsAnimating = true;

                GenerateDropShadow(source: ufoEnemy);

                if (!_ufo_enemy_fleet_appeared)
                {
                    _audioStub.Play(SoundType.UFO_ENEMY_ENTRY);

                    GenerateInterimScreen("Beware of UFO Fleet");
                    _scene_game.ActivateSlowMotion();
                    _ufo_enemy_fleet_appeared = true;
                }
            }
        }

        private void AnimateUfoEnemy(Construct ufoEnemy)
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

                var speed = ufoEnemy1.Speed;

                ufoEnemy1.MoveDownRight(speed);

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (ufoEnemy1.Honk())
                        GenerateUfoEnemyHonk(ufoEnemy1);

                    if (ufoEnemy1.Attack())
                        GenerateUfoEnemyRocket(ufoEnemy1);
                }
            }
        }

        private void RecycleUfoEnemy(Construct ufoEnemy)
        {
            var hitbox = ufoEnemy.GetHitBox();

            if (ufoEnemy.IsShrinkingComplete || hitbox.Left > Constants.DEFAULT_SCENE_WIDTH || hitbox.Top > Constants.DEFAULT_SCENE_HEIGHT) // enemy is dead or goes out of bottom right corner
            {
                ufoEnemy.IsAnimating = false;
            }
        }

        private void LooseUfoEnemyHealth(UfoEnemy ufoEnemy)
        {
            ufoEnemy.SetPopping();
            ufoEnemy.LooseHealth();

            GenerateFloatingNumber(ufoEnemy);

            if (ufoEnemy.IsDead)
            {
                _game_score_bar.GainScore(2);

                _ufo_enemy_kill_count++;

                if (_ufo_enemy_kill_count > _ufo_enemy_kill_count_limit) // after killing limited enemies increase the threadhold limit
                {
                    _ufo_enemy_threashold.IncreaseThreasholdLimit(increment: _ufo_enemy_threashold_limit_increase, currentPoint: _game_score_bar.GetScore());
                    _ufo_enemy_kill_count = 0;
                    _ufo_enemy_fleet_appeared = false;

                    LevelUp();

                    _scene_game.ActivateSlowMotion();
                }
            }
        }

        private bool UfoEnemyExists()
        {
            return _scene_game.Children.OfType<UfoEnemy>().Any(x => x.IsAnimating);
        }

        #endregion

        #region UfoEnemyRocket

        private void SpawnUfoEnemyRockets()
        {
            for (int i = 0; i < 7; i++)
            {
                UfoEnemyRocket ufoEnemyRocket = new(
                    animateAction: AnimateUfoEnemyRocket,
                    recycleAction: RecycleUfoEnemyRocket);

                ufoEnemyRocket.SetZ(z: 8);

                _scene_game.AddToScene(ufoEnemyRocket);

                SpawnDropShadow(source: ufoEnemyRocket);
            }
        }

        private void GenerateUfoEnemyRocket(UfoEnemy ufoEnemy)
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<UfoEnemyRocket>().FirstOrDefault(x => x.IsAnimating == false) is UfoEnemyRocket ufoEnemyRocket)
            {
                ufoEnemyRocket.Reset();
                ufoEnemyRocket.SetPopping();
                ufoEnemyRocket.Reposition(ufoEnemy: ufoEnemy);
                ufoEnemyRocket.IsAnimating = true;

                GenerateDropShadow(source: ufoEnemyRocket);
            }
        }

        private void AnimateUfoEnemyRocket(Construct ufoEnemyRocket)
        {
            UfoEnemyRocket ufoEnemyRocket1 = ufoEnemyRocket as UfoEnemyRocket;

            var speed = ufoEnemyRocket1.Speed;
            ufoEnemyRocket1.MoveDownRight(speed);

            if (ufoEnemyRocket1.IsBlasting)
            {
                ufoEnemyRocket.Expand();
                ufoEnemyRocket.Fade(Constants.DEFAULT_BLAST_FADE_SCALE);
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
        }

        private void RecycleUfoEnemyRocket(Construct ufoEnemyRocket)
        {
            var hitbox = ufoEnemyRocket.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (ufoEnemyRocket.IsFadingComplete || hitbox.Left > Constants.DEFAULT_SCENE_WIDTH || hitbox.Right < 0 || hitbox.Bottom < 0 || hitbox.Bottom > Constants.DEFAULT_SCENE_HEIGHT)
            {
                ufoEnemyRocket.IsAnimating = false;
            }
        }

        #endregion 

        #endregion

        #region VehicleEnemy

        private void SpawnVehicleEnemys()
        {
            for (int i = 0; i < 7; i++)
            {
                VehicleEnemy vehicleEnemy = new(
                    animateAction: AnimateVehicleEnemy,
                    recycleAction: RecycleVehicleEnemy);

                _scene_game.AddToScene(vehicleEnemy);

                vehicleEnemy.SetZ(z: 4);
            }
        }

        private void GenerateVehicleEnemy()
        {
            if (!AnyBossExists() && !_scene_game.IsSlowMotionActivated && _scene_game.Children.OfType<VehicleEnemy>().FirstOrDefault(x => x.IsAnimating == false) is VehicleEnemy vehicleEnemy)
            {
                vehicleEnemy.Reset();
                vehicleEnemy.Reposition();
                vehicleEnemy.IsAnimating = true;
            }
        }

        private void AnimateVehicleEnemy(Construct vehicleEnemy)
        {
            VehicleEnemy vehicleEnemy1 = vehicleEnemy as VehicleEnemy;

            vehicleEnemy.Pop();
            vehicleEnemy1.Vibrate();

            var speed = vehicleEnemy1.Speed;
            vehicleEnemy1.MoveDownRight(speed);

            if (_scene_game.SceneState == SceneState.GAME_RUNNING)
            {
                if (vehicleEnemy1.Honk())
                    GenerateVehicleEnemyHonk(vehicleEnemy1);
            }

            PreventVehicleEnemyOverlapping(vehicleEnemy);
        }

        private void RecycleVehicleEnemy(Construct vehicleEnemy)
        {
            var hitBox = vehicleEnemy.GetHitBox();

            if (hitBox.Top > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left > Constants.DEFAULT_SCENE_WIDTH)
            {
                vehicleEnemy.IsAnimating = false;
            }
        }

        private void PreventVehicleEnemyOverlapping(Construct vehicleEnemy)
        {
            //var vehicleEnemy_distantHitBox = vehicleEnemy.GetDistantHitBox();

            if (_scene_game.Children.OfType<VehicleEnemy>()
                .FirstOrDefault(x => x.IsAnimating && x.GetHitBox().IntersectsWith(vehicleEnemy.GetHitBox())) is Construct collidingVehicleEnemy)
            {
                var hitBox = vehicleEnemy.GetHitBox();

                if (collidingVehicleEnemy.Speed > vehicleEnemy.Speed) // colliding vehicleEnemy is faster
                {
                    vehicleEnemy.Speed = collidingVehicleEnemy.Speed;
                }
                else if (vehicleEnemy.Speed > collidingVehicleEnemy.Speed) // vehicleEnemy is faster
                {
                    collidingVehicleEnemy.Speed = vehicleEnemy.Speed;
                }
            }
        }

        private void LooseVehicleEnemyHealth(VehicleEnemy vehicleEnemy)
        {
            vehicleEnemy.SetPopping();
            vehicleEnemy.LooseHealth();

            if (vehicleEnemy.WillHonk)
            {
                GenerateFloatingNumber(vehicleEnemy);

                if (vehicleEnemy.IsDead)
                {
                    vehicleEnemy.SetBlast();
                    _game_score_bar.GainScore(2);
                }
            }
        }

        #endregion

        #region VehicleBoss

        #region VehicleBoss

        private void SpawnVehicleBosses()
        {
            VehicleBoss vehicleBoss = new(
                animateAction: AnimateVehicleBoss,
                recycleAction: RecycleVehicleBoss);

            vehicleBoss.SetZ(z: 4);

            _scene_game.AddToScene(vehicleBoss);
        }

        private void GenerateVehicleBoss()
        {
            // if scene doesn't contain a VehicleBoss then pick a random VehicleBoss and add to scene

            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _vehicle_boss_threashold.ShouldRelease(_game_score_bar.GetScore()) && !VehicleBossExists())
            {
                if (_scene_game.Children.OfType<VehicleBoss>().FirstOrDefault(x => x.IsAnimating == false) is VehicleBoss vehicleBoss)
                {
                    _audioStub.Stop(SoundType.GAME_BACKGROUND_MUSIC);
                    _audioStub.Play(SoundType.BOSS_BACKGROUND_MUSIC);
                    _audioStub.SetVolume(SoundType.AMBIENCE, 0.4);

                    vehicleBoss.Reset();
                    vehicleBoss.Reposition();
                    vehicleBoss.IsAnimating = true;
                    // set VehicleBoss health
                    vehicleBoss.Health = _vehicle_boss_threashold.GetReleasePointDifference() * 1.5;

                    _vehicle_boss_threashold.IncreaseThreasholdLimit(increment: _vehicle_boss_threashold_limit_increase, currentPoint: _game_score_bar.GetScore());

                    _vehicle_boss_health_bar.SetMaxiumHealth(vehicleBoss.Health);
                    _vehicle_boss_health_bar.SetValue(vehicleBoss.Health);
                    _vehicle_boss_health_bar.SetIcon(vehicleBoss.GetContentUri());
                    _vehicle_boss_health_bar.SetBarColor(color: Colors.Crimson);

                    GenerateInterimScreen("Crazy Honker Arrived");
                    _scene_game.ActivateSlowMotion();

                    //ToggleNightMode(true); 
                }
            }
        }

        private void AnimateVehicleBoss(Construct vehicleBoss)
        {
            VehicleBoss vehicleBoss1 = vehicleBoss as VehicleBoss;

            var speed = vehicleBoss1.Speed;

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
        }

        private void RecycleVehicleBoss(Construct vehicleBoss)
        {
            var hitBox = vehicleBoss.GetHitBox();

            VehicleBoss vehicleBoss1 = vehicleBoss as VehicleBoss;

            if (vehicleBoss1.IsDead && hitBox.Top > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left > Constants.DEFAULT_SCENE_WIDTH)
            {
                vehicleBoss.IsAnimating = false;
            }
        }

        private void LooseVehicleBossHealth(VehicleBoss vehicleBoss)
        {
            vehicleBoss.SetPopping();
            vehicleBoss.LooseHealth();

            GenerateFloatingNumber(vehicleBoss);

            _vehicle_boss_health_bar.SetValue(vehicleBoss.Health);

            if (vehicleBoss.IsDead)
            {
                _audioStub.Stop(SoundType.BOSS_BACKGROUND_MUSIC);
                _audioStub.Play(SoundType.GAME_BACKGROUND_MUSIC);
                _audioStub.SetVolume(SoundType.AMBIENCE, 0.6);

                _player.SetWinStance();
                _game_score_bar.GainScore(3);

                LevelUp();

                _scene_game.ActivateSlowMotion();

                //ToggleNightMode(false);
            }
        }

        private bool VehicleBossExists()
        {
            return _scene_game.Children.OfType<VehicleBoss>().Any(x => x.IsAnimating);
        }

        #endregion

        #region VehicleBossRocket

        private void SpawnVehicleBossRockets()
        {
            for (int i = 0; i < 4; i++)
            {
                VehicleBossRocket vehicleBossRocket = new(
                    animateAction: AnimateVehicleBossRocket,
                    recycleAction: RecycleVehicleBossRocket);

                vehicleBossRocket.SetZ(z: 7);

                _scene_game.AddToScene(vehicleBossRocket);

                SpawnDropShadow(source: vehicleBossRocket);
            }
        }

        private void GenerateVehicleBossRocket()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<VehicleBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is VehicleBoss vehicleBoss &&
                _scene_game.Children.OfType<VehicleBossRocket>().FirstOrDefault(x => x.IsAnimating == false) is VehicleBossRocket vehicleBossRocket)
            {
                vehicleBossRocket.Reset();
                vehicleBossRocket.Reposition(vehicleBoss: vehicleBoss);
                vehicleBossRocket.SetPopping();
                vehicleBossRocket.IsGravitatingUpwards = true;
                vehicleBossRocket.AwaitMoveUpRight = true;
                vehicleBossRocket.IsAnimating = true;

                GenerateDropShadow(source: vehicleBossRocket);
            }
        }

        private void AnimateVehicleBossRocket(Construct vehicleBossRocket)
        {
            VehicleBossRocket vehicleBossRocket1 = vehicleBossRocket as VehicleBossRocket;

            var speed = vehicleBossRocket1.Speed;

            if (vehicleBossRocket1.AwaitMoveUpRight)
            {
                vehicleBossRocket1.MoveUpRight(speed);
            }

            if (vehicleBossRocket1.IsBlasting)
            {
                vehicleBossRocket.Expand();
                vehicleBossRocket.Fade(Constants.DEFAULT_BLAST_FADE_SCALE);
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
        }

        private void RecycleVehicleBossRocket(Construct vehicleBossRocket)
        {
            var hitbox = vehicleBossRocket.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (vehicleBossRocket.IsFadingComplete || hitbox.Left > Constants.DEFAULT_SCENE_WIDTH || hitbox.Right < 0 || hitbox.Bottom < 0 || hitbox.Top > Constants.DEFAULT_SCENE_HEIGHT)
            {
                vehicleBossRocket.IsAnimating = false;
                vehicleBossRocket.IsGravitatingUpwards = false;                
            }
        }

        #endregion

        #endregion

        #region ZombieBoss

        #region ZombieBoss

        private void SpawnZombieBosses()
        {
            ZombieBoss zombieBoss = new(
                animateAction: AnimateZombieBoss,
                recycleAction: RecycleZombieBoss);

            zombieBoss.SetZ(z: 8);

            _scene_game.AddToScene(zombieBoss);

            SpawnDropShadow(source: zombieBoss);
        }

        private void GenerateZombieBoss()
        {
            // if scene doesn't contain a ZombieBoss then pick a ZombieBoss and add to scene

            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _zombie_boss_threashold.ShouldRelease(_game_score_bar.GetScore()) && !ZombieBossExists() &&
                _scene_game.Children.OfType<ZombieBoss>().FirstOrDefault(x => x.IsAnimating == false) is ZombieBoss zombieBoss)
            {
                _audioStub.Stop(SoundType.GAME_BACKGROUND_MUSIC);
                _audioStub.Play(SoundType.BOSS_BACKGROUND_MUSIC);
                _audioStub.SetVolume(SoundType.AMBIENCE, 0.2);

                zombieBoss.Reset();
                zombieBoss.SetPosition(
                    left: 0,
                    top: zombieBoss.Height * -1);
                zombieBoss.IsAnimating = true;

                GenerateDropShadow(source: zombieBoss);

                // set ZombieBoss health
                zombieBoss.Health = _zombie_boss_threashold.GetReleasePointDifference() * 1.5;

                _zombie_boss_threashold.IncreaseThreasholdLimit(increment: _zombie_boss_threashold_limit_increase, currentPoint: _game_score_bar.GetScore());

                _zombie_boss_health_bar.SetMaxiumHealth(zombieBoss.Health);
                _zombie_boss_health_bar.SetValue(zombieBoss.Health);
                _zombie_boss_health_bar.SetIcon(zombieBoss.GetContentUri());
                _zombie_boss_health_bar.SetBarColor(color: Colors.Crimson);

                _scene_game.ActivateSlowMotion();
                ToggleNightMode(true);

                GenerateInterimScreen("Beware of Blocks Zombie");
            }
        }

        private void AnimateZombieBoss(Construct zombieBoss)
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
                    var speed = zombieBoss1.Speed;
                    var scaling = ScreenExtensions.GetScreenSpaceScaling();

                    if (zombieBoss1.IsAttacking)
                    {
                        zombieBoss1.Move(
                            speed: speed,
                            sceneWidth: Constants.DEFAULT_SCENE_WIDTH * scaling,
                            sceneHeight: Constants.DEFAULT_SCENE_HEIGHT * scaling);

                        if (zombieBoss1.GetCloseHitBox().IntersectsWith(_player.GetCloseHitBox()))
                        {
                            LoosePlayerHealth();
                        }
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
        }

        private void RecycleZombieBoss(Construct zombieBoss)
        {
            if (zombieBoss.IsShrinkingComplete)
            {
                zombieBoss.IsAnimating = false;
            }
        }

        private void LooseZombieBossHealth(ZombieBoss zombieBoss)
        {
            zombieBoss.SetPopping();
            zombieBoss.LooseHealth();
            zombieBoss.SetHitStance();

            GenerateFloatingNumber(zombieBoss);

            _zombie_boss_health_bar.SetValue(zombieBoss.Health);

            if (zombieBoss.IsDead)
            {
                _audioStub.Stop(SoundType.BOSS_BACKGROUND_MUSIC);
                _audioStub.Play(SoundType.GAME_BACKGROUND_MUSIC);
                _audioStub.SetVolume(SoundType.AMBIENCE, 0.6);

                _player.SetWinStance();
                _game_score_bar.GainScore(3);

                LevelUp();

                _scene_game.ActivateSlowMotion();
                ToggleNightMode(false);
            }
        }

        private bool ZombieBossExists()
        {
            return _scene_game.Children.OfType<ZombieBoss>().Any(x => x.IsAnimating);
        }

        #endregion

        #region ZombieBossRocketBlock

        private void SpawnZombieBossRocketBlocks()
        {
            for (int i = 0; i < 5; i++)
            {
                ZombieBossRocketBlock zombieBossRocket = new(
                    animateAction: AnimateZombieBossRocketBlock,
                    recycleAction: RecycleZombieBossRocketBlock);

                zombieBossRocket.SetZ(z: 7);

                _scene_game.AddToScene(zombieBossRocket);

                SpawnDropShadow(source: zombieBossRocket);
            }
        }

        private void GenerateZombieBossRocketBlock()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<ZombieBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is ZombieBoss zombieBoss &&
                _scene_game.Children.OfType<ZombieBossRocketBlock>().FirstOrDefault(x => x.IsAnimating == false) is ZombieBossRocketBlock zombieBossRocket)
            {
                zombieBossRocket.Reset();
                zombieBossRocket.SetPopping();
                zombieBossRocket.Reposition();
                zombieBossRocket.IsAnimating = true;

                GenerateDropShadow(source: zombieBossRocket);
            }
        }

        private void AnimateZombieBossRocketBlock(Construct zombieBossRocket)
        {
            ZombieBossRocketBlock zombieBossRocket1 = zombieBossRocket as ZombieBossRocketBlock;

            var speed = zombieBossRocket1.Speed;

            zombieBossRocket1.MoveDownRight(speed);

            if (zombieBossRocket1.IsBlasting)
            {
                zombieBossRocket.Expand();
                zombieBossRocket.Fade(Constants.DEFAULT_BLAST_FADE_SCALE);
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
        }

        private void RecycleZombieBossRocketBlock(Construct zombieBossRocket)
        {
            var hitbox = zombieBossRocket.GetHitBox();

            var scaling = ScreenExtensions.GetScreenSpaceScaling();

            // if bomb is blasted and faed or goes out of scene bounds
            if (zombieBossRocket.IsFadingComplete || hitbox.Left > Constants.DEFAULT_SCENE_WIDTH * scaling || hitbox.Top > Constants.DEFAULT_SCENE_HEIGHT * scaling)
            {
                zombieBossRocket.IsAnimating = false;
            }
        }

        private void LooseZombieBossRocketBlockHealth(ZombieBossRocketBlock zombieBossRocket)
        {
            zombieBossRocket.LooseHealth();
            GenerateFloatingNumber(zombieBossRocket);
        }

        #endregion

        #endregion

        #region MafiaBoss

        #region MafiaBoss

        private void SpawnMafiaBosses()
        {
            MafiaBoss mafiaBoss = new(
                animateAction: AnimateMafiaBoss,
                recycleAction: RecycleMafiaBoss);

            mafiaBoss.SetZ(z: 8);

            _scene_game.AddToScene(mafiaBoss);

            SpawnDropShadow(source: mafiaBoss);
        }

        private void GenerateMafiaBoss()
        {
            // if scene doesn't contain a MafiaBoss then pick a MafiaBoss and add to scene

            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _mafia_boss_threashold.ShouldRelease(_game_score_bar.GetScore()) && !MafiaBossExists() &&
                _scene_game.Children.OfType<MafiaBoss>().FirstOrDefault(x => x.IsAnimating == false) is MafiaBoss mafiaBoss)
            {
                _audioStub.Stop(SoundType.GAME_BACKGROUND_MUSIC);
                _audioStub.Play(SoundType.BOSS_BACKGROUND_MUSIC);
                _audioStub.SetVolume(SoundType.AMBIENCE, 0.2);

                mafiaBoss.Reset();
                mafiaBoss.SetPosition(
                    left: 0,
                    top: mafiaBoss.Height * -1);
                mafiaBoss.IsAnimating = true;

                GenerateDropShadow(source: mafiaBoss);

                // set MafiaBoss health
                mafiaBoss.Health = _mafia_boss_threashold.GetReleasePointDifference() * 1.5;

                _mafia_boss_threashold.IncreaseThreasholdLimit(increment: _mafia_boss_threashold_limit_increase, currentPoint: _game_score_bar.GetScore());

                _mafia_boss_health_bar.SetMaxiumHealth(mafiaBoss.Health);
                _mafia_boss_health_bar.SetValue(mafiaBoss.Health);
                _mafia_boss_health_bar.SetIcon(mafiaBoss.GetContentUri());
                _mafia_boss_health_bar.SetBarColor(color: Colors.Crimson);

                _scene_game.ActivateSlowMotion();

                GenerateInterimScreen("Beware of Crimson Mafia");

                ToggleNightMode(true);
            }
        }

        private void AnimateMafiaBoss(Construct mafiaBoss)
        {
            MafiaBoss mafiaBoss1 = mafiaBoss as MafiaBoss;

            if (mafiaBoss1.IsDead)
            {
                mafiaBoss.Shrink();
            }
            else
            {
                mafiaBoss.Pop();

                mafiaBoss1.Hover();
                mafiaBoss1.DepleteHitStance();
                mafiaBoss1.DepleteWinStance();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    var speed = mafiaBoss1.Speed;
                    var scaling = ScreenExtensions.GetScreenSpaceScaling();

                    if (mafiaBoss1.IsAttacking)
                    {
                        mafiaBoss1.Move(
                            speed: speed,
                            sceneWidth: Constants.DEFAULT_SCENE_WIDTH * scaling,
                            sceneHeight: Constants.DEFAULT_SCENE_HEIGHT * scaling,
                            playerPoint: _player.GetCloseHitBox());


                        if (mafiaBoss1.GetCloseHitBox().IntersectsWith(_player.GetCloseHitBox()))
                        {
                            LoosePlayerHealth();
                        }
                    }
                    else
                    {
                        mafiaBoss1.MoveDownRight(speed);

                        if (mafiaBoss.GetLeft() > (Constants.DEFAULT_SCENE_WIDTH * scaling / 3)) // bring MafiaBoss to a suitable distance from player and then start attacking
                        {
                            mafiaBoss1.IsAttacking = true;
                        }
                    }
                }
            }
        }

        private void RecycleMafiaBoss(Construct mafiaBoss)
        {
            if (mafiaBoss.IsShrinkingComplete)
            {
                mafiaBoss.IsAnimating = false;
            }
        }

        private void LooseMafiaBossHealth(MafiaBoss mafiaBoss)
        {
            mafiaBoss.SetPopping();
            mafiaBoss.LooseHealth();
            mafiaBoss.SetHitStance();

            GenerateFloatingNumber(mafiaBoss);

            _mafia_boss_health_bar.SetValue(mafiaBoss.Health);

            if (mafiaBoss.IsDead)
            {
                _audioStub.Stop(SoundType.BOSS_BACKGROUND_MUSIC);
                _audioStub.Play(SoundType.GAME_BACKGROUND_MUSIC);
                _audioStub.SetVolume(SoundType.AMBIENCE, 0.6);

                _player.SetWinStance();
                _game_score_bar.GainScore(3);

                LevelUp();

                _scene_game.ActivateSlowMotion();
                ToggleNightMode(false);
            }
        }

        private bool MafiaBossExists()
        {
            return _scene_game.Children.OfType<MafiaBoss>().Any(x => x.IsAnimating);
        }

        #endregion

        #region MafiaBossRocket

        private void SpawnMafiaBossRockets()
        {
            for (int i = 0; i < 4; i++)
            {
                MafiaBossRocket mafiaBossRocket = new(
                    animateAction: AnimateMafiaBossRocket,
                    recycleAction: RecycleMafiaBossRocket);

                mafiaBossRocket.SetZ(z: 7);

                _scene_game.AddToScene(mafiaBossRocket);

                SpawnDropShadow(source: mafiaBossRocket);
            }
        }

        private void GenerateMafiaBossRocket()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<MafiaBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is MafiaBoss mafiaBoss &&
                _scene_game.Children.OfType<MafiaBossRocket>().FirstOrDefault(x => x.IsAnimating == false) is MafiaBossRocket mafiaBossRocket)
            {
                mafiaBossRocket.Reset();
                mafiaBossRocket.SetPopping();
                mafiaBossRocket.Reposition(mafiaBoss: mafiaBoss);
                mafiaBossRocket.IsAnimating = true;

                GenerateDropShadow(source: mafiaBossRocket);
                SetBossRocketDirection(source: mafiaBoss, rocket: mafiaBossRocket, rocketTarget: _player);
            }
        }

        private void AnimateMafiaBossRocket(Construct mafiaBossRocket)
        {
            MafiaBossRocket mafiaBossRocket1 = mafiaBossRocket as MafiaBossRocket;

            var speed = mafiaBossRocket1.Speed;

            if (mafiaBossRocket1.AwaitMoveDownLeft)
            {
                mafiaBossRocket1.MoveDownLeft(speed);
            }
            else if (mafiaBossRocket1.AwaitMoveUpRight)
            {
                mafiaBossRocket1.MoveUpRight(speed);
            }
            else if (mafiaBossRocket1.AwaitMoveUpLeft)
            {
                mafiaBossRocket1.MoveUpLeft(speed);
            }
            else if (mafiaBossRocket1.AwaitMoveDownRight)
            {
                mafiaBossRocket1.MoveDownRight(speed);
            }

            if (mafiaBossRocket1.IsBlasting)
            {
                mafiaBossRocket.Expand();
                mafiaBossRocket.Fade(Constants.DEFAULT_BLAST_FADE_SCALE);
            }
            else
            {
                mafiaBossRocket.Pop();
                mafiaBossRocket1.Hover();

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (mafiaBossRocket.GetCloseHitBox().IntersectsWith(_player.GetCloseHitBox()))
                    {
                        mafiaBossRocket1.SetBlast();
                        LoosePlayerHealth();
                    }

                    if (mafiaBossRocket1.AutoBlast())
                        mafiaBossRocket1.SetBlast();
                }
            }
        }

        private void RecycleMafiaBossRocket(Construct mafiaBossRocket)
        {
            var hitbox = mafiaBossRocket.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (mafiaBossRocket.IsFadingComplete || hitbox.Left > Constants.DEFAULT_SCENE_WIDTH || hitbox.Right < 0 || hitbox.Bottom < 0 || hitbox.Top > Constants.DEFAULT_SCENE_HEIGHT)
            {
                mafiaBossRocket.IsAnimating = false;
            }
        }

        #endregion                

        #region MafiaBossRocketBullsEye

        private void SpawnMafiaBossRocketBullsEyes()
        {
            for (int i = 0; i < 3; i++)
            {
                MafiaBossRocketBullsEye mafiaBossRocketBullsEye = new(
                    animateAction: AnimateMafiaBossRocketBullsEye,
                    recycleAction: RecycleMafiaBossRocketBullsEye);

                mafiaBossRocketBullsEye.SetZ(z: 7);

                _scene_game.AddToScene(mafiaBossRocketBullsEye);

                SpawnDropShadow(source: mafiaBossRocketBullsEye);
            }
        }

        private void GenerateMafiaBossRocketBullsEye()
        {
            // generate a seeking bomb if one is not in scene
            if (_scene_game.SceneState == SceneState.GAME_RUNNING &&
                _scene_game.Children.OfType<MafiaBoss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is MafiaBoss mafiaBoss &&
                _scene_game.Children.OfType<MafiaBossRocketBullsEye>().FirstOrDefault(x => x.IsAnimating == false) is MafiaBossRocketBullsEye mafiaBossRocketBullsEye)
            {
                mafiaBossRocketBullsEye.Reset();
                mafiaBossRocketBullsEye.SetPopping();
                mafiaBossRocketBullsEye.Reposition(mafiaBoss: mafiaBoss);
                mafiaBossRocketBullsEye.SetTarget(_player.GetCloseHitBox());
                mafiaBossRocketBullsEye.IsAnimating = true;

                GenerateDropShadow(source: mafiaBossRocketBullsEye);
            }
        }

        private void AnimateMafiaBossRocketBullsEye(Construct mafiaBossRocketBullsEye)
        {
            MafiaBossRocketBullsEye mafiaBossRocketBullsEye1 = mafiaBossRocketBullsEye as MafiaBossRocketBullsEye;

            var speed = mafiaBossRocketBullsEye1.Speed;

            if (mafiaBossRocketBullsEye1.IsBlasting)
            {
                mafiaBossRocketBullsEye.Expand();
                mafiaBossRocketBullsEye.Fade(Constants.DEFAULT_BLAST_FADE_SCALE);
                mafiaBossRocketBullsEye1.MoveDownRight(speed);
            }
            else
            {
                mafiaBossRocketBullsEye.Pop();
                mafiaBossRocketBullsEye.Rotate(rotationSpeed: 2.5);

                if (_scene_game.SceneState == SceneState.GAME_RUNNING)
                {
                    if (_scene_game.Children.OfType<MafiaBoss>().Any(x => x.IsAnimating && x.IsAttacking))
                    {
                        mafiaBossRocketBullsEye1.Move();

                        if (mafiaBossRocketBullsEye1.GetCloseHitBox().IntersectsWith(_player.GetCloseHitBox()))
                        {
                            mafiaBossRocketBullsEye1.SetBlast();
                            LoosePlayerHealth();
                        }
                        else
                        {
                            if (mafiaBossRocketBullsEye1.AutoBlast())
                                mafiaBossRocketBullsEye1.SetBlast();
                        }
                    }
                    else
                    {
                        mafiaBossRocketBullsEye1.SetBlast();
                    }
                }
            }
        }

        private void RecycleMafiaBossRocketBullsEye(Construct mafiaBossRocketBullsEye)
        {
            var hitbox = mafiaBossRocketBullsEye.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (mafiaBossRocketBullsEye.IsFadingComplete || hitbox.Left > Constants.DEFAULT_SCENE_WIDTH || hitbox.Right < 0 || hitbox.Bottom < 0 || hitbox.Bottom > Constants.DEFAULT_SCENE_HEIGHT)
            {
                mafiaBossRocketBullsEye.IsAnimating = false;
            }
        }

        #endregion 

        #endregion

        #region Honk

        private void SpawnHonks()
        {
            for (int i = 0; i < 10; i++)
            {
                Honk honk = new(
                    animateAction: AnimateHonk,
                    recycleAction: RecycleHonk);

                honk.SetZ(z: 5);

                _scene_game.AddToScene(honk);
            }
        }

        private void GenerateHonk(Construct source)
        {
            if (_scene_game.Children.OfType<Honk>().FirstOrDefault(x => x.IsAnimating == false) is Honk honk)
            {
                honk.SetPopping();

                honk.Reset();

                var hitBox = source.GetCloseHitBox();

                honk.Reposition(source: source);
                honk.SetRotation(_random.Next(-30, 30));
                honk.SetZ(source.GetZ() + 1);

                source.SetPopping();
                honk.IsAnimating = true;
            }
        }

        private void AnimateHonk(Construct honk)
        {
            honk.Pop();
            honk.Fade(0.06);
        }

        private void RecycleHonk(Construct honk)
        {
            if (honk.IsFadingComplete)
            {
                honk.IsAnimating = false;
            }
        }

        private void GenerateVehicleBossHonk(VehicleBoss source)
        {
            // if there are no UfoBosses or enemies in the scene the vehicles will honk

            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !UfoBossExists())
            {
                GenerateHonk(source);
            }
        }

        private void GenerateVehicleEnemyHonk(VehicleEnemy source)
        {
            // if there are no UfoBosses or enemies in the scene the vehicles will honk

            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !UfoEnemyExists() && !AnyBossExists())
            {
                GenerateHonk(source);
            }
        }

        private void GenerateUfoEnemyHonk(UfoEnemy source)
        {
            // if there are no UfoBosses in the scene the vehicles will honk

            if (_scene_game.SceneState == SceneState.GAME_RUNNING && !UfoBossExists())
            {
                GenerateHonk(source);
            }
        }

        #endregion

        #region Cloud

        private void SpawnClouds()
        {
            for (int i = 0; i < 3; i++)
            {
                Cloud cloud = new(
                    animateAction: AnimateCloud,
                    recycleAction: RecycleCloud);

                cloud.SetZ(z: 9);

                _scene_game.AddToScene(cloud);
            }
        }

        private void GenerateCloud()
        {
            if (!AnyBossExists() && _scene_game.Children.OfType<Cloud>().FirstOrDefault(x => x.IsAnimating == false) is Cloud cloud)
            {
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
                cloud.IsAnimating = true;
            }
        }

        private void AnimateCloud(Construct cloud)
        {
            Cloud cloud1 = cloud as Cloud;
            cloud1.Hover();

            var speed = cloud1.Speed;
            cloud1.MoveDownRight(speed);
        }

        private void RecycleCloud(Construct cloud)
        {
            var hitBox = cloud.GetHitBox();

            if (hitBox.Top > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left > Constants.DEFAULT_SCENE_WIDTH)
            {
                cloud.IsAnimating = false;
            }
        }

        #endregion

        #region DropShadow

        private void SpawnDropShadow(Construct source)
        {
            DropShadow dropShadow = new(
                animateAction: AnimateDropShadow,
                recycleAction: RecycleDropShadow);

            _scene_game.AddToScene(dropShadow);

            dropShadow.SetParent(construct: source);
            dropShadow.Move();
            dropShadow.SetZ(source.GetZ() - 1);
        }

        private void AnimateDropShadow(Construct construct)
        {
            DropShadow dropShadow = construct as DropShadow;
            dropShadow.Move();
        }

        private void RecycleDropShadow(Construct dropShadow)
        {
            DropShadow dropShadow1 = dropShadow as DropShadow;

            if (!dropShadow1.IsParentConstructAnimating())
            {
                dropShadow.IsAnimating = false;
            }
        }

        private void GenerateDropShadow(Construct source)
        {
            if (_scene_game.Children.OfType<DropShadow>().FirstOrDefault(x => x.Id == source.Id) is DropShadow dropShadow)
            {
                dropShadow.SetZ(source.GetZ() - 2);
                dropShadow.Reset();
                dropShadow.IsAnimating = true;
            }
        }

        #endregion

        #region Pickup

        #region HealthPickup

        private void SpawnHealthPickups()
        {
            for (int i = 0; i < 3; i++)
            {
                HealthPickup healthPickup = new(
                    animateAction: AnimateHealthPickup,
                    recycleAction: RecycleHealthPickup);

                healthPickup.SetZ(z: 6);

                _scene_game.AddToScene(healthPickup);
            }
        }

        private void GenerateHealthPickups()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING && HealthPickup.ShouldGenerate(_player.Health) &&
                _scene_game.Children.OfType<HealthPickup>().FirstOrDefault(x => x.IsAnimating == false) is HealthPickup healthPickup)
            {
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

                healthPickup.IsAnimating = true;
            }
        }

        private void AnimateHealthPickup(Construct healthPickup)
        {
            HealthPickup healthPickup1 = healthPickup as HealthPickup;

            var speed = healthPickup1.Speed;

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
        }

        private void RecycleHealthPickup(Construct healthPickup)
        {
            var hitBox = healthPickup.GetHitBox();

            if (healthPickup.IsShrinkingComplete || hitBox.Top - healthPickup.Height > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left - healthPickup.Width > Constants.DEFAULT_SCENE_WIDTH) // if object is out side of bottom right corner
            {
                healthPickup.IsAnimating = false;
            }
        }

        #endregion

        #region PowerUpPickup

        private void SpawnPowerUpPickups()
        {
            for (int i = 0; i < 3; i++)
            {
                PowerUpPickup powerUpPickup = new(
                    animateAction: AnimatePowerUpPickup,
                    recycleAction: RecyclePowerUpPickup);

                powerUpPickup.SetZ(z: 6);

                _scene_game.AddToScene(powerUpPickup);
            }
        }

        private void GeneratePowerUpPickup()
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING)
            {
                if ((AnyInAirBossExists() || UfoEnemyExists()) && !_powerUp_health_bar.HasHealth) // if any in air boss or enemy exists and currently player has no other power up
                {
                    if (_scene_game.Children.OfType<PowerUpPickup>().FirstOrDefault(x => x.IsAnimating == false) is PowerUpPickup powerUpPickup)
                    {
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

                        powerUpPickup.IsAnimating = true;
                    }
                }
            }
        }

        private void AnimatePowerUpPickup(Construct powerUpPickup)
        {
            PowerUpPickup powerUpPickup1 = powerUpPickup as PowerUpPickup;

            var speed = powerUpPickup1.Speed;

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

                    if (_player.GetCloseHitBox().IntersectsWith(hitbox))
                    {
                        powerUpPickup1.PickedUp();

                        _powerUp_health_bar.Tag = powerUpPickup1.PowerUpType;

                        switch (powerUpPickup1.PowerUpType)
                        {
                            case PowerUpType.SEEKING_SNITCH: // if seeking snitch powerup, allow using a burst of 3 seeking bombs 3 times
                                {
                                    _powerUp_health_bar.SetMaxiumHealth(9);
                                    _powerUp_health_bar.SetValue(9);

                                    GenerateInterimScreen("Seeking Snitch +9");
                                }
                                break;
                            case PowerUpType.BULLS_EYE: // if bulls eye powerup, allow using a single shot of 20 bombs
                                {
                                    _powerUp_health_bar.SetMaxiumHealth(20);
                                    _powerUp_health_bar.SetValue(20);

                                    GenerateInterimScreen("Bylls Eye +20");
                                }
                                break;
                            case PowerUpType.ARMOR:
                                {
                                    _powerUp_health_bar.SetMaxiumHealth(10); // if armor powerup then take additional 10 hits
                                    _powerUp_health_bar.SetValue(10);

                                    GenerateInterimScreen("Armor +10");
                                }
                                break;
                            default:
                                break;
                        }

                        _powerUp_health_bar.SetIcon(powerUpPickup1.GetContentUri());
                        _powerUp_health_bar.SetBarColor(color: Colors.Green);
                    }
                }
            }
        }

        private void RecyclePowerUpPickup(Construct powerUpPickup)
        {
            var hitBox = powerUpPickup.GetHitBox();

            if (hitBox.Top - powerUpPickup.Height > Constants.DEFAULT_SCENE_HEIGHT || hitBox.Left - powerUpPickup.Width > Constants.DEFAULT_SCENE_WIDTH || powerUpPickup.IsShrinkingComplete)
            {
                powerUpPickup.IsAnimating = false;
            }
        }

        private void DepletePowerUp()
        {
            // use up the power up
            if (_powerUp_health_bar.HasHealth)
                _powerUp_health_bar.SetValue(_powerUp_health_bar.GetValue() - 1);
        }

        #endregion      

        #endregion

        #region FloatingNumber

        private void SpawnFloatingNumbers()
        {
            for (int i = 0; i < 5; i++)
            {
                FloatingNumber floatingNumber = new(
                    animateAction: AnimateFloatingNumber,
                    recycleAction: RecycleFloatingNumber);

                floatingNumber.SetZ(z: 10);

                _scene_game.AddToScene(floatingNumber);
            }
        }

        private void GenerateFloatingNumber(HealthyConstruct source)
        {
            if (!_scene_game.IsSlowMotionActivated && _scene_game.Children.OfType<FloatingNumber>().FirstOrDefault(x => x.IsAnimating == false) is FloatingNumber floatingNumberTop)
            {
                floatingNumberTop.Reset(source.HitPoint);
                floatingNumberTop.Reposition(source);
                floatingNumberTop.IsAnimating = true;
            }
        }

        private void AnimateFloatingNumber(Construct floatingNumber)
        {
            FloatingNumber floatingNumber1 = floatingNumber as FloatingNumber;
            floatingNumber1.Move();
            floatingNumber1.DepleteOnScreenDelay();
        }

        private void RecycleFloatingNumber(Construct floatingNumber)
        {
            FloatingNumber floatingNumber1 = floatingNumber as FloatingNumber;

            if (floatingNumber1.IsDepleted)
            {
                floatingNumber.IsAnimating = false;
            }
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

        private void SetBossRocketDirection(Construct source, AnimableConstruct rocket, Construct rocketTarget)
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

        #region Boss

        private bool AnyBossExists()
        {
            return (UfoBossExists() || VehicleBossExists() || ZombieBossExists() || MafiaBossExists());
        }

        private bool AnyInAirBossExists()
        {
            return (UfoBossExists() || ZombieBossExists() || MafiaBossExists());
        }

        #endregion

        #region Controller

        private void SetController()
        {
            _game_controller.SetScene(_scene_game);
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
            _sound_pollution_health_bar.Visibility = visibility;
        }

        #endregion

        #region Scene

        private void PrepareGameScene()
        {
            _scene_game.Clear();

            #region Player

            SpawnPlayerBalloon();
            SpawnPlayerRockets();
            SpawnPlayerHonkBombs();
            SpawnPlayerRocketSeekings();
            SpawnPlayerRocketBullsEyes();

            #endregion

            #region UfoEnemy

            SpawnUfoEnemys();
            SpawnUfoEnemyRockets();

            _scene_game.AddToScene(
            new Generator(
                delay: 180,
                elaspedAction: GenerateUfoEnemy,
                scramble: true));

            #endregion

            #region UfoBoss

            SpawnUfoBosses();
            SpawnUfoBossRockets();
            SpawnUfoBossRocketSeekings();

            _scene_game.AddToScene(

            new Generator(
                delay: 10,
                elaspedAction: GenerateUfoBoss),

            new Generator(
                delay: 40,
                elaspedAction: GenerateUfoBossRocket,
                scramble: true),

            new Generator(
                delay: 200,
                elaspedAction: GenerateUfoBossRocketSeeking,
                scramble: true)
                );

            #endregion

            #region FloatingNumber

            SpawnFloatingNumbers();

            #endregion

            #region Pickup

            SpawnHealthPickups();
            SpawnPowerUpPickups();

            _scene_game.AddToScene(

            new Generator(
                delay: 800,
                elaspedAction: GenerateHealthPickups),

            new Generator(
                delay: 800,
                elaspedAction: GeneratePowerUpPickup));

            #endregion

            #region Road

            SpawnRoadMarks();
            SpawnRoadSideBillboards();
            SpawnRoadSideLamps();
            SpawnRoadSideLightBillboards();
            SpawnRoadSideWalks();
            SpawnRoadSideTrees();
            SpawnRoadSideHedges();

            _scene_game.AddToScene(

            new Generator(
                delay: 38,
                elaspedAction: GenerateRoadMark),

            new Generator(
                delay: 72,
                elaspedAction: GenerateRoadSideBillboard),

            new Generator(
                delay: 36,
                elaspedAction: GenerateRoadSideLamp),

            new Generator(
                delay: 36,
                elaspedAction: GenerateRoadSideLightBillboard),

            new Generator(
                delay: 18,
                elaspedAction: GenerateRoadSideWalk),

            new Generator(
                delay: 30,
                elaspedAction: GenerateRoadSideTree),

            new Generator(
                delay: 38,
                elaspedAction: GenerateRoadSideHedge));

            #endregion

            #region Cloud

            SpawnClouds();

            _scene_game.AddToScene(
            new Generator(
                delay: 400,
                elaspedAction: GenerateCloud,
                scramble: true));

            #endregion

            #region VehicleEnemy

            SpawnVehicleEnemys();
            SpawnHonks();

            _scene_game.AddToScene(new Generator(
                delay: 95,
                elaspedAction: GenerateVehicleEnemy));

            #endregion

            #region VehicleBoss

            SpawnVehicleBosses();
            SpawnVehicleBossRockets();

            _scene_game.AddToScene(
            new Generator(
                delay: 10,
                elaspedAction: GenerateVehicleBoss),

            new Generator(
                delay: 50,
                elaspedAction: GenerateVehicleBossRocket,
                scramble: true));

            #endregion

            #region ZombieBoss

            SpawnZombieBosses();
            SpawnZombieBossRocketBlocks();

            _scene_game.AddToScene(
            new Generator(
                delay: 10,
                elaspedAction: GenerateZombieBoss),

            new Generator(
                delay: 30,
                elaspedAction: GenerateZombieBossRocketBlock)
            );

            #endregion

            #region MafiaBoss

            SpawnMafiaBosses();
            SpawnMafiaBossRockets();
            SpawnMafiaBossRocketBullsEyes();

            _scene_game.AddToScene(
            new Generator(
                delay: 10,
                elaspedAction: GenerateMafiaBoss),

            new Generator(
                delay: 75,
                elaspedAction: GenerateMafiaBossRocket),

            new Generator(
                delay: 45,
                elaspedAction: GenerateMafiaBossRocketBullsEye));

            #endregion
        }

        private void PrepareMainMenuScene()
        {
            _scene_main_menu.Clear();

            SpawnAssetsLoadingScreen();
            SpawnInterimScreen();
            SpawnGameStartScreen();
            SpawnPlayerCharacterSelectionScreen();
            SpawnPlayerHonkBombSelectionScreen();
            SpawnPromptOrientationChangeScreen();
        }

        private void SetSceneScaling()
        {
            var scaling = ScreenExtensions.GetScreenSpaceScaling();

            LoggingExtensions.Log($"ScreenSpaceScaling: {scaling}");

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

        #region Events

        #region Load

        private void HonkBomberPage_Loaded(object sender, RoutedEventArgs e)
        {
            ScreenExtensions.DisplayInformation.OrientationChanged += DisplayInformation_OrientationChanged;
            ScreenExtensions.SetRequiredDisplayOrientations(DisplayOrientations.Landscape, DisplayOrientations.LandscapeFlipped);

            // set display orientation to required orientation
            if (!ScreenExtensions.IsScreenInRequiredOrientation())
                ScreenExtensions.ChangeDisplayOrientationAsRequired();

            SetController();
            PrepareMainMenuScene();
            _scene_main_menu.Play();

            SizeChanged += HonkBomberPage_SizeChanged;

            if (ScreenExtensions.IsScreenInRequiredOrientation()) // if the screen is in desired orientation the show asset loading screen
            {
                ScreenExtensions.EnterFullScreen(true);
                GenerateAssetsLoadingScreen(); // if generators are not added to game scene, show the assets loading screen               
            }
            else
            {
                GeneratePromptOrientationChangeScreen();
            }
        }

        private void HonkBomberPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= HonkBomberPage_SizeChanged;
            ScreenExtensions.DisplayInformation.OrientationChanged -= DisplayInformation_OrientationChanged;
            UnsetController();
        }

        #endregion

        #region Size

        private void HonkBomberPage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            ScreenExtensions.Width = args.NewSize.Width <= Constants.DEFAULT_SCENE_WIDTH ? args.NewSize.Width : Constants.DEFAULT_SCENE_WIDTH;
            ScreenExtensions.Height = args.NewSize.Height <= Constants.DEFAULT_SCENE_HEIGHT ? args.NewSize.Height : Constants.DEFAULT_SCENE_HEIGHT;

            SetSceneScaling();

            if (_scene_game.SceneState == SceneState.GAME_RUNNING)
            {
                _player.Reposition();
                GenerateDropShadow(source: _player);
            }

            RepositionHoveringTitleScreens();
            LoggingExtensions.Log($"Width: {ScreenExtensions.Width} x Height: {ScreenExtensions.Height}");
        }

        #endregion

        #region Orientation

        private void DisplayInformation_OrientationChanged(DisplayInformation sender, object args)
        {
            if (_scene_game.SceneState == SceneState.GAME_RUNNING) // if screen orientation is changed while game is running, pause the game
            {
                PauseGame();
            }
            else
            {
                ScreenExtensions.EnterFullScreen(true);

                if (ScreenExtensions.IsScreenInRequiredOrientation())
                {
                    if (_scene_main_menu.Children.OfType<PromptOrientationChangeScreen>().FirstOrDefault(x => x.IsAnimating) is PromptOrientationChangeScreen promptOrientationChangeScreen)
                    {
                        RecyclePromptOrientationChangeScreen(promptOrientationChangeScreen);
                        GenerateAssetsLoadingScreen();
                    }
                }
                else // ask to change orientation
                {
                    _scene_game.Pause();
                    _scene_main_menu.Pause();

                    _audioStub.Pause(SoundType.GAME_BACKGROUND_MUSIC);

                    foreach (var hoveringTitleScreen in _scene_main_menu.Children.OfType<HoveringTitleScreen>().Where(x => x.IsAnimating))
                    {
                        hoveringTitleScreen.IsAnimating = false;
                    }

                    foreach (var construct in _scene_game.Children.OfType<Construct>())
                    {
                        construct.IsAnimating = false;
                    }

                    GeneratePromptOrientationChangeScreen();
                }
            }

            LoggingExtensions.Log($"CurrentOrientation: {sender.CurrentOrientation}");
        }

        #endregion

        #endregion
    }
}
