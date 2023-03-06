using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace HonkTrooper
{
    public partial class Controller : ControllerBase
    {
        #region Fields

        private readonly int _keysSize = 60;
        private readonly int _keyCornerRadius = 30;
        private readonly int _keyBorderThickness = 4;
        private readonly int _directionKeysMargin = 6;

        //private readonly CompositeTransform _directionKeysTransform = new()
        //{
        //    CenterX = 0.5,
        //    CenterY = 0.5,
        //    Rotation = 0,
        //    ScaleX = 1,
        //    ScaleY = 1,
        //};

        #endregion

        #region Properties

        public Grid DirectionKeys { get; set; }

        public Button StartButton { get; set; }

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
            SetStartButton();
        }

        #endregion

        #region Methods

        private void SetDirectionKeys()
        {
            DirectionKeys = new()
            {
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Bottom,
                Margin = new Microsoft.UI.Xaml.Thickness(20),
                //RenderTransformOrigin = new Point(0.5, 0.5),
                //RenderTransform = _directionControlsTransform,
            };

            DirectionKeys.RowDefinitions.Add(new RowDefinition());
            DirectionKeys.RowDefinitions.Add(new RowDefinition());

            DirectionKeys.ColumnDefinitions.Add(new ColumnDefinition());
            DirectionKeys.ColumnDefinitions.Add(new ColumnDefinition());

            Border up = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = _keysSize,
                Width = _keysSize,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(_keyCornerRadius),
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Up,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Microsoft.UI.Xaml.Thickness(_keyBorderThickness),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },
                Margin = new Microsoft.UI.Xaml.Thickness(_directionKeysMargin),
            };

            up.PointerEntered += (s, e) => { ActivateMoveUp(); };
            up.PointerExited += (s, e) => { DeactivateMoveUp(); };

            SetRow(up, 0);
            SetColumn(up, 0);

            Border down = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = _keysSize,
                Width = _keysSize,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(_keyCornerRadius),
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Forward
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Microsoft.UI.Xaml.Thickness(_keyBorderThickness),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = 45 },
                Margin = new Microsoft.UI.Xaml.Thickness(_directionKeysMargin),
            };

            down.PointerEntered += (s, e) => { ActivateMoveDown(); };
            down.PointerExited += (s, e) => { DeactivateMoveDown(); };

            SetRow(down, 1);
            SetColumn(down, 1);

            Border left = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = _keysSize,
                Width = _keysSize,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(_keyCornerRadius),
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Back,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Microsoft.UI.Xaml.Thickness(_keyBorderThickness),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },
                Margin = new Microsoft.UI.Xaml.Thickness(_directionKeysMargin),
            };

            left.PointerEntered += (s, e) => { ActivateMoveLeft(); };
            left.PointerExited += (s, e) => { DeactivateMoveLeft(); };

            SetRow(left, 1);
            SetColumn(left, 0);

            Border right = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = _keysSize,
                Width = _keysSize,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(_keyCornerRadius),
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Forward,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Microsoft.UI.Xaml.Thickness(_keyBorderThickness),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },
                Margin = new Microsoft.UI.Xaml.Thickness(_directionKeysMargin),
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

        private void SetStartButton()
        {
            StartButton = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = _keysSize,
                Width = _keysSize,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(_keyCornerRadius),
                Content = new SymbolIcon()
                {
                    Symbol = Symbol.Play,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Microsoft.UI.Xaml.Thickness(_keyBorderThickness),
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Top,
                Margin = new Microsoft.UI.Xaml.Thickness(20),
            };

            StartButton.Click += (s, e) =>
            {
                if (SceneStartOrStop())
                {
                    AttackButton.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
                    StartButton.Content = new SymbolIcon()
                    {
                        Symbol = Symbol.Pause,
                    };
                }
                else
                {
                    StartButton.Content = new SymbolIcon()
                    {
                        Symbol = Symbol.Play,
                    };
                }

            };
            this.Children.Add(StartButton);
        }

        private void SetAttackButton()
        {
            AttackButton = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = _keysSize,
                Width = _keysSize,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(_keyCornerRadius),
                Content = new SymbolIcon()
                {
                    Symbol = Symbol.Target,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Microsoft.UI.Xaml.Thickness(_keyBorderThickness),
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Bottom,
                Margin = new Microsoft.UI.Xaml.Thickness(20),
            };

            AttackButton.Click += (s, e) => { ActivateAttack(); };
            this.Children.Add(AttackButton);
        }

        #endregion
    }
}
