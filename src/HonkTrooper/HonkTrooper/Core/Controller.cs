using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Foundation;

namespace HonkTrooper
{
    public partial class Controller : ControllerBase
    {
        #region Fields

        public event EventHandler<bool> OnPlayPause;

        #endregion

        #region Properties

        public Grid DirectionKeys { get; set; }

        public Button PlayButton { get; set; }

        public Button PauseButton { get; set; }

        public Button AttackButton { get; set; }

        #endregion

        #region Ctor

        public Controller()
        {
            CanDrag = false;

            KeyUp += Controller_KeyUp;
            KeyDown += Controller_KeyDown;

            SetDirectionKeys();
            SetAttackButton();
            //SetPlayButton();
            SetPauseButton();
        }

        #endregion

        #region Methods

        private void SetDirectionKeys()
        {
            DirectionKeys = new()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(20),
                //Visibility = Visibility.Collapsed,
            };

            DirectionKeys.RowDefinitions.Add(new RowDefinition());
            DirectionKeys.RowDefinitions.Add(new RowDefinition());

            DirectionKeys.ColumnDefinitions.Add(new ColumnDefinition());
            DirectionKeys.ColumnDefinitions.Add(new ColumnDefinition());

            Border up = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS),
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Up,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },
                Margin = new Thickness(Constants.DEFAULT_CONTROLLER_DIRECTION_KEYS_MARGIN),
            };

            up.PointerEntered += (s, e) => { ActivateMoveUp(); };
            up.PointerExited += (s, e) => { DeactivateMoveUp(); };

            SetRow(up, 0);
            SetColumn(up, 0);

            Border down = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS),
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Forward
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = 45 },
                Margin = new Thickness(Constants.DEFAULT_CONTROLLER_DIRECTION_KEYS_MARGIN),
            };

            down.PointerEntered += (s, e) => { ActivateMoveDown(); };
            down.PointerExited += (s, e) => { DeactivateMoveDown(); };

            SetRow(down, 1);
            SetColumn(down, 1);

            Border left = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS),
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Back,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },
                Margin = new Thickness(Constants.DEFAULT_CONTROLLER_DIRECTION_KEYS_MARGIN),
            };

            left.PointerEntered += (s, e) => { ActivateMoveLeft(); };
            left.PointerExited += (s, e) => { DeactivateMoveLeft(); };

            SetRow(left, 1);
            SetColumn(left, 0);

            Border right = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS),
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Forward,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },
                Margin = new Thickness(Constants.DEFAULT_CONTROLLER_DIRECTION_KEYS_MARGIN),
            };

            right.PointerEntered += (s, e) => { ActivateMoveRight(); };
            right.PointerExited += (s, e) => { DeactivateMoveRight(); };

            SetRow(right, 0);
            SetColumn(right, 1);

            DirectionKeys.Children.Add(up);
            DirectionKeys.Children.Add(down);
            DirectionKeys.Children.Add(left);
            DirectionKeys.Children.Add(right);

            this.Children.Add(DirectionKeys);
        }

        //private void SetPlayButton()
        //{

        //    PlayButton = new()
        //    {
        //        Background = new SolidColorBrush(Colors.Goldenrod),
        //        Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
        //        Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
        //        CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS),
        //        Content = new SymbolIcon()
        //        {
        //            Symbol = Symbol.Play,
        //        },
        //        BorderBrush = new SolidColorBrush(Colors.White),
        //        BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
        //        HorizontalAlignment = HorizontalAlignment.Right,
        //        VerticalAlignment = VerticalAlignment.Top,
        //        Margin = new Thickness(20),
        //    };

        //    PlayButton.Click += (s, e) =>
        //    {
        //        PlayPauseScene();
        //    };
        //    this.Children.Add(PlayButton);
        //}

        private void SetPauseButton()
        {
            PauseButton = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS),
                Content = new SymbolIcon()
                {
                    Symbol = Symbol.Pause,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(20),
                //Visibility = Visibility.Collapsed,
            };

            PauseButton.Click += (s, e) =>
            {
                //PlayPauseScene();

                OnPlayPause?.Invoke(this, false);
            };

            this.Children.Add(PauseButton);
        }

        //public void PlayPauseScene()
        //{
        //    if (ToggleScenePlayOrPause())
        //    {
        //        //OnPlayScene();
        //        OnPlayPause?.Invoke(this, true);
        //    }
        //    else
        //    {
        //        //OnPauseScene();
        //        OnPlayPause?.Invoke(this, false);
        //    }
        //}

        //public void ToggleHudVisibility(Visibility visibility) 
        //{
        //    DirectionKeys.Visibility = visibility;
        //    AttackButton.Visibility = visibility;

        //    PauseButton.Visibility = visibility;
        //    PlayButton.Visibility = visibility;
        //}

        //private void OnPauseScene()
        //{
        //    // game stopped
        //    DirectionKeys.Visibility = Visibility.Collapsed;
        //    AttackButton.Visibility = Visibility.Collapsed;

        //    PauseButton.Visibility = Visibility.Collapsed;
        //    PlayButton.Visibility = Visibility.Visible;

        //    OnPlayPause?.Invoke(this, false);
        //}

        //private void OnPlayScene()
        //{
        //    // game running
        //    DirectionKeys.Visibility = Visibility.Visible;
        //    AttackButton.Visibility = Visibility.Visible;

        //    PauseButton.Visibility = Visibility.Visible;
        //    PlayButton.Visibility = Visibility.Collapsed;

        //    AttackButton.Focus(FocusState.Programmatic);

        //    OnPlayPause?.Invoke(this, true);
        //}

        private void SetAttackButton()
        {
            AttackButton = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS),
                Content = new SymbolIcon()
                {
                    Symbol = Symbol.Target,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(20),                
            };

            AttackButton.Click += (s, e) => { ActivateAttack(); };
            this.Children.Add(AttackButton);
        }

        #endregion
    }
}
