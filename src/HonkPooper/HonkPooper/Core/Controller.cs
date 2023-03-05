using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace HonkPooper
{
    public partial class Controller : ControllerBase
    {
        #region Fields

        private int _keysSize = 55;

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

            SizeChanged += Controller_SizeChanged;
        }

        private void Controller_SizeChanged(object sender, Microsoft.UI.Xaml.SizeChangedEventArgs args)
        {
            var _sceneWidth = args.NewSize.Width;
            var _sceneHeight = args.NewSize.Height;

            var downScaling = ScreenExtensions.GetDownScaling(_sceneWidth);

            _directionControlsTransform.ScaleX = 1 * downScaling;
            _directionControlsTransform.ScaleY = 1 * downScaling;
        }

        #endregion

        #region Methods

        public void SetDirectionControlsRotation(double rotation)
        {
            _directionControlsTransform.Rotation = rotation;
        }

        private void SetStartButton()
        {
            StartButton = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = _keysSize,
                Width = _keysSize,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(30),
                Content = new SymbolIcon()
                {
                    Symbol = Symbol.Play,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Microsoft.UI.Xaml.Thickness(6),
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
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(30),
                Content = new SymbolIcon()
                {
                    Symbol = Symbol.Bold,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Microsoft.UI.Xaml.Thickness(6),
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
                RenderTransform = _directionControlsTransform,
            };

            DirectionControls.RowDefinitions.Add(new RowDefinition());
            DirectionControls.RowDefinitions.Add(new RowDefinition());
            DirectionControls.RowDefinitions.Add(new RowDefinition());

            DirectionControls.ColumnDefinitions.Add(new ColumnDefinition());
            DirectionControls.ColumnDefinitions.Add(new ColumnDefinition());
            DirectionControls.ColumnDefinitions.Add(new ColumnDefinition());

            Border up = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = _keysSize,
                Width = _keysSize,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(30),
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Up,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Microsoft.UI.Xaml.Thickness(6),
            };

            up.PointerPressed += (s, e) => { ActivateMoveUp(); };
            up.PointerReleased += (s, e) => { DeactivateMoveUp(); };

            Border down = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = _keysSize,
                Width = _keysSize,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(30),
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Download
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Microsoft.UI.Xaml.Thickness(6),
            };

            down.PointerPressed += (s, e) => { ActivateMoveDown(); };
            down.PointerReleased += (s, e) => { DeactivateMoveDown(); };

            Border left = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = _keysSize,
                Width = _keysSize,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(30),
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Back,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Microsoft.UI.Xaml.Thickness(6),
            };

            left.PointerPressed += (s, e) => { ActivateMoveLeft(); };
            left.PointerReleased += (s, e) => { DeactivateMoveLeft(); };

            Border right = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = _keysSize,
                Width = _keysSize,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(30),
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Forward,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Microsoft.UI.Xaml.Thickness(6),
            };

            right.PointerPressed += (s, e) => { ActivateMoveRight(); };
            right.PointerReleased += (s, e) => { DeactivateMoveRight(); };

            Grid.SetRow(up, 0);
            Grid.SetColumn(up, 1);

            Grid.SetRow(left, 1);
            Grid.SetColumn(left, 0);

            Grid.SetRow(right, 1);
            Grid.SetColumn(right, 2);

            Grid.SetRow(down, 2);
            Grid.SetColumn(down, 1);

            DirectionControls.Children.Add(up);
            DirectionControls.Children.Add(down);
            DirectionControls.Children.Add(left);
            DirectionControls.Children.Add(right);

            this.Children.Add(DirectionControls);
        }

        #endregion
    }
}
