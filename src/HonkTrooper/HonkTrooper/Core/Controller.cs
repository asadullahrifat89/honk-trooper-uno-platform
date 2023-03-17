using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Devices.Sensors;
using Windows.Foundation;

namespace HonkTrooper
{
    public partial class Controller : ControllerBase
    {
        #region Fields

        //private int _move_up_activator;
        //private int _move_down_activator;
        //private int _move_left_activator;
        //private int _move_right_activator;

        //private int _move_x_activator;
        //private int _move_y_activator;

        #endregion

        #region Properties

        public Border JoyStick { get; set; }

        public Grid Keypad { get; set; }

        public Button AttackButton { get; set; }

        public Button PauseButton { get; set; }

        public Gyrometer Gyrometer { get; set; }

        public double AngularVelocityX { get; set; }

        public double AngularVelocityY { get; set; }

        public double AngularVelocityZ { get; set; }

        public bool JoyStickActive { get; set; }

        #endregion

        #region Ctor

        public Controller()
        {
            CanDrag = false;

            SetAttackButton();
            SetPauseButton();

            KeyUp += Controller_KeyUp;
            KeyDown += Controller_KeyDown;

            SetJoystick();
            //SetKeypad();            
            //SetGyrometer();             
        }

        #endregion

        #region Methods

        public void SetJoystick()
        {
            JoyStick = new Border()
            {
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE * 3.5,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE * 3.5,
                //Background = new SolidColorBrush(Colors.Goldenrod),
                CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS * 3.5),
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(20),

            };

            Canvas canvas = new Canvas()
            {
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE * 3.5,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE * 3.5,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Colors.Goldenrod),
                Opacity = 0.5,
            };


            Construct upLeft = new()
            {
                Tag = MovementDirection.UpLeft,
                //Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Up,
                },
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },
            };

            Construct up = new()
            {
                Tag = MovementDirection.Up,
                //Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Up,
                },
                RenderTransformOrigin = new Point(0.5, 0.5),
            };

            Construct upRight = new()
            {
                Tag = MovementDirection.UpRight,
                //Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Forward,
                },
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },
            };

            Construct left = new()
            {
                Tag = MovementDirection.Left,
                //Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Back,
                },
                RenderTransformOrigin = new Point(0.5, 0.5),
            };

            Construct right = new()
            {
                Tag = MovementDirection.Right,
                //Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Forward,
                },

                RenderTransformOrigin = new Point(0.5, 0.5),
            };

            Construct downLeft = new()
            {
                Tag = MovementDirection.DownLeft,
                //Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Back,
                },
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },
            };

            Construct down = new()
            {
                Tag = MovementDirection.Down,
                //Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Up
                },
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = 180 },
            };

            Construct downRight = new()
            {
                Tag = MovementDirection.DownRight,
                //Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Forward
                },
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = 45 },
            };

            Construct thumb = new()
            {
                Tag = MovementDirection.None,
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS),
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
            };

            //upLeft.SetPosition(left: 0, top: 0);
            //canvas.Children.Add(upLeft);

            up.SetPosition(left: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 1.25, top: 0);
            canvas.Children.Add(up);

            //upRight.SetPosition(left: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 2, top: 0);
            //canvas.Children.Add(upRight);

            left.SetPosition(left: 0, top: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 1.25);
            canvas.Children.Add(left);

            right.SetPosition(left: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 2 * 1.25, top: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 1.25);
            canvas.Children.Add(right);

            //downLeft.SetPosition(left: 0, top: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 2);
            //canvas.Children.Add(downLeft);

            down.SetPosition(left: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 1.25, top: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 2 * 1.25);
            canvas.Children.Add(down);

            //downRight.SetPosition(left: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 2, top: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 2);
            //canvas.Children.Add(downRight);

            thumb.SetPosition(left: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 1.25, top: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 1.20);
            canvas.Children.Add(thumb);

            canvas.PointerPressed += (s, e) =>
            {
                DeactivateMoveUp();
                DeactivateMoveDown();
                DeactivateMoveLeft();
                DeactivateMoveRight();

                //Microsoft.UI.Input.PointerPoint point = e.GetCurrentPoint(canvas);

                //thumb.SetPosition(left: point.Position.X - thumb.Width / 2, top: point.Position.Y - thumb.Height / 2);

                JoyStickActive = true;
            };
            canvas.PointerMoved += (s, e) =>
            {
                if (JoyStickActive)
                {
                    Microsoft.UI.Input.PointerPoint point = e.GetCurrentPoint(canvas);
                    thumb.SetPosition(left: point.Position.X - thumb.Width / 2, top: point.Position.Y - thumb.Height / 2);

                    //if (thumb.GetHitBox().IntersectsWith(upLeft.GetCloseHitBox()))
                    //{
                    //    ActivateMoveUp();
                    //    ActivateMoveLeft();
                    //}
                    //else
                    //{
                    //    DeactivateMoveUp();
                    //    DeactivateMoveLeft();
                    //}

                    if (thumb.GetHitBox().IntersectsWith(up.GetCloseHitBox()))
                    {
                        ActivateMoveUp();
                    }
                    else
                    {
                        DeactivateMoveUp();
                    }

                    //if (thumb.GetHitBox().IntersectsWith(upRight.GetCloseHitBox()))
                    //{
                    //    ActivateMoveUp();
                    //    ActivateMoveRight();
                    //}
                    //else
                    //{
                    //    DeactivateMoveUp();
                    //    DeactivateMoveRight();
                    //}

                    if (thumb.GetHitBox().IntersectsWith(left.GetCloseHitBox()))
                    {
                        ActivateMoveLeft();
                    }
                    else
                    {
                        DeactivateMoveLeft();
                    }

                    if (thumb.GetHitBox().IntersectsWith(right.GetCloseHitBox()))
                    {
                        ActivateMoveRight();
                    }
                    else
                    {
                        DeactivateMoveRight();
                    }

                    //if (thumb.GetHitBox().IntersectsWith(downLeft.GetCloseHitBox()))
                    //{
                    //    ActivateMoveDown();
                    //    ActivateMoveLeft();
                    //}
                    //else
                    //{
                    //    DeactivateMoveDown();
                    //    DeactivateMoveLeft();
                    //}

                    if (thumb.GetHitBox().IntersectsWith(down.GetCloseHitBox()))
                    {
                        ActivateMoveDown();
                    }
                    else
                    {
                        DeactivateMoveDown();
                    }

                    //if (thumb.GetHitBox().IntersectsWith(downRight.GetCloseHitBox()))
                    //{
                    //    ActivateMoveDown();
                    //    ActivateMoveRight();
                    //}
                    //else
                    //{
                    //    DeactivateMoveDown();
                    //    DeactivateMoveRight();
                    //}
                }
            };
            canvas.PointerReleased += (s, e) =>
            {
                DeactivateMoveUp();
                DeactivateMoveDown();
                DeactivateMoveLeft();
                DeactivateMoveRight();

                JoyStickActive = false;

                thumb.SetPosition(left: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 1.25, top: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 1.20);
            };
            //canvas.PointerExited += (s, e) =>
            //{
            //    DeactivateMoveUp();
            //    DeactivateMoveDown();
            //    DeactivateMoveLeft();
            //    DeactivateMoveRight();

            //    JoyStickActive = false;

            //    thumb.SetPosition(left: Constants.DEFAULT_CONTROLLER_KEY_SIZE, top: Constants.DEFAULT_CONTROLLER_KEY_SIZE);
            //};

            JoyStick.Child = canvas;

            this.Children.Add(JoyStick);
        }

        public void SetKeypad()
        {
            Keypad = new()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(20),
            };

            Keypad.RowDefinitions.Add(new RowDefinition());
            Keypad.RowDefinitions.Add(new RowDefinition());
            Keypad.RowDefinitions.Add(new RowDefinition());

            Keypad.ColumnDefinitions.Add(new ColumnDefinition());
            Keypad.ColumnDefinitions.Add(new ColumnDefinition());
            Keypad.ColumnDefinitions.Add(new ColumnDefinition());

            #region UpLeft

            Border upLeft = new()
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

            upLeft.PointerEntered += (s, e) =>
            {
                ActivateMoveUp();
                ActivateMoveLeft();
            };
            upLeft.PointerExited += (s, e) =>
            {
                DeactivateMoveUp();
                DeactivateMoveLeft();
            };

            Grid.SetRow(upLeft, 0);
            Grid.SetColumn(upLeft, 0);

            #endregion

            #region Up

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
                //RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },
                Margin = new Thickness(Constants.DEFAULT_CONTROLLER_DIRECTION_KEYS_MARGIN),
            };

            up.PointerEntered += (s, e) => { ActivateMoveUp(); };
            up.PointerExited += (s, e) => { DeactivateMoveUp(); };

            Grid.SetRow(up, 0);
            Grid.SetColumn(up, 1);

            #endregion

            #region UpRight

            Border upRight = new()
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

            upRight.PointerEntered += (s, e) =>
            {
                ActivateMoveUp();
                ActivateMoveRight();
            };
            upRight.PointerExited += (s, e) =>
            {
                DeactivateMoveUp();
                DeactivateMoveRight();
            };

            Grid.SetRow(upRight, 0);
            Grid.SetColumn(upRight, 2);

            #endregion 

            #region Left

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
                //RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },
                Margin = new Thickness(Constants.DEFAULT_CONTROLLER_DIRECTION_KEYS_MARGIN),
            };

            left.PointerEntered += (s, e) => { ActivateMoveLeft(); };
            left.PointerExited += (s, e) => { DeactivateMoveLeft(); };

            Grid.SetRow(left, 1);
            Grid.SetColumn(left, 0);

            #endregion

            #region Right

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
                //RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },
                Margin = new Thickness(Constants.DEFAULT_CONTROLLER_DIRECTION_KEYS_MARGIN),
            };

            right.PointerEntered += (s, e) => { ActivateMoveRight(); };
            right.PointerExited += (s, e) => { DeactivateMoveRight(); };

            Grid.SetRow(right, 1);
            Grid.SetColumn(right, 2);

            //#endregion

            #endregion

            #region DownLeft

            Border downLeft = new()
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

            downLeft.PointerEntered += (s, e) =>
            {
                ActivateMoveDown();
                ActivateMoveLeft();
            };
            downLeft.PointerExited += (s, e) =>
            {
                DeactivateMoveDown();
                DeactivateMoveLeft();
            };

            Grid.SetRow(downLeft, 2);
            Grid.SetColumn(downLeft, 0);

            #endregion            

            #region Down

            Border down = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS),
                Child = new SymbolIcon()
                {
                    Symbol = Symbol.Up
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = 180 },
                Margin = new Thickness(Constants.DEFAULT_CONTROLLER_DIRECTION_KEYS_MARGIN),
            };

            down.PointerEntered += (s, e) => { ActivateMoveDown(); };
            down.PointerExited += (s, e) => { DeactivateMoveDown(); };

            Grid.SetRow(down, 2);
            Grid.SetColumn(down, 1);

            #endregion

            #region DownRight

            Border downRight = new()
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

            downRight.PointerEntered += (s, e) =>
            {
                ActivateMoveDown();
                ActivateMoveRight();
            };
            downRight.PointerExited += (s, e) =>
            {
                DeactivateMoveDown();
                DeactivateMoveRight();
            };

            Grid.SetRow(downRight, 2);
            Grid.SetColumn(downRight, 2);

            #endregion

            Keypad.Children.Add(upLeft);
            Keypad.Children.Add(up);
            Keypad.Children.Add(upRight);

            Keypad.Children.Add(left);
            Keypad.Children.Add(right);

            Keypad.Children.Add(downLeft);
            Keypad.Children.Add(down);
            Keypad.Children.Add(downRight);

            this.Children.Add(Keypad);
        }

        private void SetAttackButton()
        {
            AttackButton = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE * 1.5,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE * 1.5,
                CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS * 1.5),
                Content = new SymbolIcon()
                {
                    Symbol = Symbol.Target,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(30),
            };

            AttackButton.Click += (s, e) => { ActivateAttack(); };
            this.Children.Add(AttackButton);
        }

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
            };
            this.Children.Add(PauseButton);
        }

        public void SetGyrometer()
        {
            Gyrometer = Gyrometer.GetDefault();

            if (Gyrometer is not null)
            {
                // Console.WriteLine($"Gyrometer detected.");
                Gyrometer.ReadingChanged += Gyrometer_ReadingChanged;
            }
        }

        public void UnsetGyrometer()
        {

            if (Gyrometer is not null)
            {
                // Console.WriteLine($"Gyrometer detected.");
                Gyrometer.ReadingChanged -= Gyrometer_ReadingChanged;
            }
        }

        #endregion

        #region Events

        private void Gyrometer_ReadingChanged(Gyrometer sender, GyrometerReadingChangedEventArgs args)
        {
            AngularVelocityX = args.Reading.AngularVelocityX;
            AngularVelocityY = args.Reading.AngularVelocityY;
            AngularVelocityZ = args.Reading.AngularVelocityZ;

            Console.WriteLine($"AngularVelocityX: {AngularVelocityX}");
            Console.WriteLine($"AngularVelocityY: {AngularVelocityY}");
            Console.WriteLine($"AngularVelocityZ: {AngularVelocityZ}");

            #region Isometric Movement V1

            if (AngularVelocityX > 0)
            {
                if (AngularVelocityX > 15)
                {
                    ActivateMoveDown();
                }
                else
                {
                    DeactivateMoveDown();
                }
            }
            else
            {
                if (Math.Abs(AngularVelocityX) > 15)
                {
                    ActivateMoveUp();
                }
                else
                {
                    DeactivateMoveUp();
                }
            }

            if (AngularVelocityY > 0)
            {
                if (AngularVelocityY > 10)
                {
                    ActivateMoveRight();
                }
                else
                {
                    DeactivateMoveRight();
                }
            }
            else
            {
                if (Math.Abs(AngularVelocityY) > 10)
                {
                    ActivateMoveLeft();
                }
                else
                {
                    DeactivateMoveLeft();
                }
            }

            #endregion           

            #region Linear Movement

            //if (AngularVelocityY > 0)
            //{
            //    if (AngularVelocityY > 20)
            //    {
            //        ActivateMoveUp();
            //    }
            //    else
            //    {
            //        DeactivateMoveUp();
            //        DeactivateMoveDown();
            //    }
            //}
            //else
            //{
            //    if (Math.Abs(AngularVelocityY) > 20)
            //    {
            //        ActivateMoveDown();
            //    }
            //    else
            //    {
            //        DeactivateMoveUp();
            //        DeactivateMoveDown();
            //    }
            //}

            //if (AngularVelocityX > 0)
            //{
            //    if (AngularVelocityX > 15)
            //    {
            //        ActivateMoveRight();
            //    }
            //    else
            //    {
            //        DeactivateMoveRight();
            //        DeactivateMoveLeft();
            //    }
            //}
            //else
            //{
            //    if (Math.Abs(AngularVelocityX) > 15)
            //    {
            //        ActivateMoveLeft();
            //    }
            //    else
            //    {
            //        DeactivateMoveRight();
            //        DeactivateMoveLeft();
            //    }
            //}

            #endregion
        }

        #endregion
    }
}
