using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Foundation;
using Windows.Security.Cryptography.Certificates;

namespace HonkPooper
{
    public partial class ControlerBase : Border
    {
        #region Fields

        private Scene _scene;

        #endregion

        #region Ctor

        public ControlerBase()
        {

        }

        #endregion

        #region Properties

        public Point PointerPosition { get; set; }

        public bool IsMoveUp { get; set; }

        public bool IsMoveDown { get; set; }

        public bool IsMoveLeft { get; set; }

        public bool IsMoveRight { get; set; }

        public bool IsAttacking { get; set; }

        #endregion

        #region Events

        public void Controller_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Left:
                    {
                        ActivateMoveLeft();
                    }
                    break;
                case Windows.System.VirtualKey.Right:
                    {
                        ActivateMoveRight();
                    }
                    break;
                case Windows.System.VirtualKey.Up:
                    {
                        ActivateMoveUp();
                    }
                    break;
                case Windows.System.VirtualKey.Down:
                    {
                        ActivateMoveDown();
                    }
                    break;
                case Windows.System.VirtualKey.Enter:
                    {
                        StartScene();
                    }
                    break;
                case Windows.System.VirtualKey.Escape:
                    {
                        Console.WriteLine("Escape");
                    }
                    break;
                case Windows.System.VirtualKey.Space:
                    {
                        ActivateAttack();
                    }
                    break;
                default:
                    break;
            }
        }

        public void ActivateAttack()
        {
            IsAttacking = true;

            Console.WriteLine("Space");
        }

        public void StartScene()
        {
            if (_scene.IsAnimating)
                _scene.Stop();
            else
                _scene.Start();

            Console.WriteLine("Enter");
        }

        public void ActivateMoveDown()
        {
            IsMoveDown = true;
            IsMoveUp = false;

            Console.WriteLine("Down");
        }

        public void ActivateMoveUp()
        {
            IsMoveUp = true;
            IsMoveDown = false;

            Console.WriteLine("Up");
        }

        public void ActivateMoveRight()
        {
            IsMoveLeft = false;
            IsMoveRight = true;

            Console.WriteLine("Right");
        }

        public void ActivateMoveLeft()
        {
            IsMoveLeft = true;
            IsMoveRight = false;

            Console.WriteLine("Left");
        }

        public void Controller_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Left:
                    {
                        DeactivateMoveLeft();
                    }
                    break;
                case Windows.System.VirtualKey.Right:
                    {
                        DeactivateMoveRight();
                    }
                    break;
                case Windows.System.VirtualKey.Up:
                    {
                        DeactivateMoveUp();
                    }
                    break;
                case Windows.System.VirtualKey.Down:
                    {
                        DeactivateMoveDown();
                    }
                    break;
                case Windows.System.VirtualKey.Space:
                    {
                        DeactivateAttack();
                    }
                    break;
                default:
                    break;
            }
        }

        public void DeactivateAttack()
        {
            IsAttacking = false;
        }

        public void DeactivateMoveDown()
        {
            IsMoveDown = false;
        }

        public void DeactivateMoveUp()
        {
            IsMoveUp = false;
        }

        public void DeactivateMoveRight()
        {
            IsMoveRight = false;
        }

        public void DeactivateMoveLeft()
        {
            IsMoveLeft = false;
        }

        public void Controller_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            PointerPoint point = e.GetCurrentPoint(_scene);
            PointerPosition = point.Position;
        }

        public void Controller_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            PointerPoint point = e.GetCurrentPoint(_scene);
            PointerPosition = point.Position;
        }

        #endregion

        #region Methods

        public void SetScene(Scene scene)
        {
            _scene = scene;
        }

        #endregion        
    }

    public partial class Controller : ControlerBase
    {
        #region Ctor

        public Controller()
        {
            PointerPressed += Controller_PointerPressed;
            PointerMoved += Controller_PointerMoved;
            KeyUp += Controller_KeyUp;
            KeyDown += Controller_KeyDown;
        }

        #endregion
    }

    public partial class OnScreenController : ControlerBase
    {
        #region Fields
        
        private int _keysSize = 55;

        #endregion

        #region Ctor

        public OnScreenController()
        {
            Grid keysContainer = new()
            {
                //HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right,
                //VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Bottom,
                Margin = new Microsoft.UI.Xaml.Thickness(10)
            };

            keysContainer.RowDefinitions.Add(new RowDefinition());
            keysContainer.RowDefinitions.Add(new RowDefinition());
            keysContainer.RowDefinitions.Add(new RowDefinition());

            keysContainer.ColumnDefinitions.Add(new ColumnDefinition());
            keysContainer.ColumnDefinitions.Add(new ColumnDefinition());
            keysContainer.ColumnDefinitions.Add(new ColumnDefinition());

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

            up.PointerPressed += (s,e)=> { ActivateMoveUp(); };
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

            keysContainer.Children.Add(up);
            keysContainer.Children.Add(down);
            keysContainer.Children.Add(left);
            keysContainer.Children.Add(right);

            this.Child = keysContainer;
        } 

        #endregion
    }
}
