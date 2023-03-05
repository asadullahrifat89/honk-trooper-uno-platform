using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace HonkPooper
{
    public partial class Controller : ControllerBase
    {
        #region Fields

        private int _keysSize = 50;
        private int _keyCornerRadius = 30;
        private int _keyBorderThickness = 4;

        private readonly CompositeTransform _directionControlsTransform = new()
        {
            CenterX = 0.5,
            CenterY = 0.5,
            Rotation = 0,
            ScaleX = 1,
            ScaleY = 1,
        };

        #endregion

        #region Properties

        public Grid DirectionControls { get; set; }

        public Button StartButton { get; set; }

        public Button AttackButton { get; set; }

        #endregion

        #region Ctor

        public Controller()
        {
            //PointerPressed += Controller_PointerPressed;
            //PointerMoved += Controller_PointerMoved;

            CanDrag = false;

            KeyUp += Controller_KeyUp;
            KeyDown += Controller_KeyDown;

            SetDirectionControls();
            SetAttackButton();
            SetStartButton();

            //SizeChanged += Controller_SizeChanged;
        }

        //private void Controller_SizeChanged(object sender, Microsoft.UI.Xaml.SizeChangedEventArgs args)
        //{
        //    var _sceneWidth = args.NewSize.Width;
        //    var _sceneHeight = args.NewSize.Height;

        //    var downScaling = ScreenExtensions.GetDownScaling(_sceneWidth);

        //    _directionControlsTransform.ScaleX = 1 * downScaling;
        //    _directionControlsTransform.ScaleY = 1 * downScaling;
        //}

        #endregion

        #region Methods

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
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Top,
                Margin = new Microsoft.UI.Xaml.Thickness(20),
            };

            AttackButton.Click += (s, e) => { ActivateAttack(); };
            this.Children.Add(AttackButton);
        }

        private void SetDirectionControls()
        {
            DirectionControls = new()
            {
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Bottom,
                Margin = new Microsoft.UI.Xaml.Thickness(20),
                //RenderTransformOrigin = new Point(0.5, 0.5),
                //RenderTransform = _directionControlsTransform,
            };

            DirectionControls.RowDefinitions.Add(new RowDefinition());
            DirectionControls.RowDefinitions.Add(new RowDefinition());

            DirectionControls.ColumnDefinitions.Add(new ColumnDefinition());
            DirectionControls.ColumnDefinitions.Add(new ColumnDefinition());

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
                Margin = new Microsoft.UI.Xaml.Thickness(5),
            };

            up.PointerPressed += (s, e) => { ActivateMoveUp(); };
            up.PointerReleased += (s, e) => { DeactivateMoveUp(); };

            Grid.SetRow(up, 0);
            Grid.SetColumn(up, 0);

            Border down = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = _keysSize,
                Width = _keysSize,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(_keyCornerRadius),
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Download
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Microsoft.UI.Xaml.Thickness(_keyBorderThickness),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },
                Margin = new Microsoft.UI.Xaml.Thickness(5),
            };

            down.PointerPressed += (s, e) => { ActivateMoveDown(); };
            down.PointerReleased += (s, e) => { DeactivateMoveDown(); };

            Grid.SetRow(down, 1);
            Grid.SetColumn(down, 1);

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
                Margin = new Microsoft.UI.Xaml.Thickness(5),
            };

            left.PointerPressed += (s, e) => { ActivateMoveLeft(); };
            left.PointerReleased += (s, e) => { DeactivateMoveLeft(); };

            Grid.SetRow(left, 1);
            Grid.SetColumn(left, 0);

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
                Margin = new Microsoft.UI.Xaml.Thickness(5),
            };

            right.PointerPressed += (s, e) => { ActivateMoveRight(); };
            right.PointerReleased += (s, e) => { DeactivateMoveRight(); };

            Grid.SetRow(right, 0);
            Grid.SetColumn(right, 1);

            DirectionControls.Children.Add(up);
            DirectionControls.Children.Add(down);
            DirectionControls.Children.Add(left);
            DirectionControls.Children.Add(right);

            this.Children.Add(DirectionControls);
        }

        #endregion
    }
}
