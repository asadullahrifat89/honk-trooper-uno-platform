using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Foundation;
using Windows.Graphics.Display;

namespace HonkTrooper
{
    public partial class ControllerBase : Grid
    {
        #region Fields

        public event EventHandler<DisplayOrientations> RequiresScreenOrientationChange;

        private Scene Scene;

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
                //case Windows.System.VirtualKey.Enter:
                //    {
                //        ToggleScenePlayOrPause();
                //    }
                //    break;
                //case Windows.System.VirtualKey.Escape:
                //    {
                //        // Console.WriteLine("Escape");
                //    }
                //    break;
                case Windows.System.VirtualKey.Space:
                    {
                        ActivateAttack();
                    }
                    break;
                default:
                    break;
            }
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

        public void Controller_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            PointerPoint point = e.GetCurrentPoint(Scene);
            PointerPosition = point.Position;
        }

        public void Controller_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            PointerPoint point = e.GetCurrentPoint(Scene);
            PointerPosition = point.Position;
        }

        #endregion

        #region Methods

        public void Reset()
        {
            DeactivateAttack();
            DeactivateMoveLeft();
            DeactivateMoveRight();
            DeactivateMoveUp();
            DeactivateMoveDown();
        }

        public void SetScene(Scene scene)
        {
            Scene = scene;
        }

        public void ActivateAttack()
        {
            IsAttacking = true;

            // Console.WriteLine("Space");
        }

        public void ActivateMoveDown()
        {
            IsMoveDown = true;
            IsMoveUp = false;

            // Console.WriteLine("Down");
        }

        public void ActivateMoveUp()
        {
            IsMoveUp = true;
            IsMoveDown = false;

            // Console.WriteLine("Up");
        }

        public void ActivateMoveRight()
        {
            IsMoveLeft = false;
            IsMoveRight = true;

            // Console.WriteLine("Right");
        }

        public void ActivateMoveLeft()
        {
            IsMoveLeft = true;
            IsMoveRight = false;

            // Console.WriteLine("Left");
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

        #endregion        
    }
}
