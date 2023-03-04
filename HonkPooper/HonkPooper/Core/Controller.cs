using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace HonkPooper
{
    public partial class Controller : Border
    {
        #region Fields

        private Scene _scene;
        private Construct _player;

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
        }

        #endregion

        #region Methods

        public void SetScene(Scene scene, Construct player)
        {
            _scene = scene;
            _player = player;
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
