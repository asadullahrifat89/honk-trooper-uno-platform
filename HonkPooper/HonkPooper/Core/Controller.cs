using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace HonkPooper
{
    public partial class Controller : Grid
    {
        #region Fields

        private Scene _scene;
        private int _keysSize = 55;

        #endregion

        #region Properties

        public Point PointerPosition { get; set; }

        public bool IsMoveUp { get; set; }

        public bool IsMoveDown { get; set; }

        public bool IsMoveLeft { get; set; }

        public bool IsMoveRight { get; set; }

        public bool IsAttacking { get; set; }

        #endregion

        #region Ctor
        public Controller()
        {
            PointerPressed += Controller_PointerPressed;
            PointerMoved += Controller_PointerMoved;
            KeyUp += Controller_KeyUp;
            KeyDown += Controller_KeyDown;

            Grid keysContainer = new()
            {
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Bottom,
                Margin = new Microsoft.UI.Xaml.Thickness(10)
            };

            keysContainer.RowDefinitions.Add(new RowDefinition());
            keysContainer.RowDefinitions.Add(new RowDefinition());
            keysContainer.RowDefinitions.Add(new RowDefinition());

            keysContainer.ColumnDefinitions.Add(new ColumnDefinition());
            keysContainer.ColumnDefinitions.Add(new ColumnDefinition());
            keysContainer.ColumnDefinitions.Add(new ColumnDefinition());

            Button up = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = _keysSize,
                Width = _keysSize,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(30),
                Content = new SymbolIcon()
                {
                    Symbol = Symbol.Up,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Microsoft.UI.Xaml.Thickness(6),
            };

            Button down = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = _keysSize,
                Width = _keysSize,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(30),
                Content = new SymbolIcon()
                {
                    Symbol = Symbol.Download
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Microsoft.UI.Xaml.Thickness(6),
            };
            Button left = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = _keysSize,
                Width = _keysSize,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(30),
                Content = new SymbolIcon()
                {
                    Symbol = Symbol.Back,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Microsoft.UI.Xaml.Thickness(6),
            };
            Button right = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = _keysSize,
                Width = _keysSize,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(30),
                Content = new SymbolIcon()
                {
                    Symbol = Symbol.Forward,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Microsoft.UI.Xaml.Thickness(6),
            };

            Grid.SetRow(up, 0);
            Grid.SetColumn(up, 1);

            Grid.SetRow(left, 1);
            Grid.SetColumn(left, 0);

            Grid.SetRow(right, 1);
            Grid.SetColumn(right, 2);

            Grid.SetRow(down, 2);
            Grid.SetColumn(down, 1);

            keysContainer.Children.Add(up);
            keysContainer.Children.Add(down);
            keysContainer.Children.Add(left);
            keysContainer.Children.Add(right);

            this.Children.Add(keysContainer);


        }

        #endregion

        #region Methods

        public void SetScene(Scene scene)
        {
            _scene = scene;            
        }

        #endregion

        #region Events

        private void Controller_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Left:
                    {
                        IsMoveLeft = true;
                        IsMoveRight = false;
                    }
                    break;
                case Windows.System.VirtualKey.Right:
                    {
                        IsMoveLeft = false;
                        IsMoveRight = true;
                    }
                    break;
                case Windows.System.VirtualKey.Up:
                    {
                        IsMoveUp = true;
                        IsMoveDown = false;
                    }
                    break;
                case Windows.System.VirtualKey.Down:
                    {
                        IsMoveDown = true;
                        IsMoveUp = false;
                    }
                    break;
                case Windows.System.VirtualKey.Enter:
                    {
                        if (_scene.IsAnimating)
                            _scene.Stop();
                        else
                            _scene.Start();
                    }
                    break;
                case Windows.System.VirtualKey.Escape:
                    {

                    }
                    break;
                case Windows.System.VirtualKey.Space:
                    {
                        IsAttacking = true;
                    }
                    break;
                default:
                    break;
            }
        }

        private void Controller_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Left:
                    {
                        IsMoveLeft = false;
                    }
                    break;
                case Windows.System.VirtualKey.Right:
                    {
                        IsMoveRight = false;
                    }
                    break;
                case Windows.System.VirtualKey.Up:
                    {
                        IsMoveUp = false;
                    }
                    break;
                case Windows.System.VirtualKey.Down:
                    {
                        IsMoveDown = false;
                    }
                    break;
                case Windows.System.VirtualKey.Space:
                    {
                        IsAttacking = false;
                    }
                    break;
                default:
                    break;
            }
        }

        private void Controller_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            PointerPoint point = e.GetCurrentPoint(_scene);
            PointerPosition = point.Position;
        }

        private void Controller_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            PointerPoint point = e.GetCurrentPoint(_scene);
            PointerPosition = point.Position;
        }

        #endregion
    }
}
