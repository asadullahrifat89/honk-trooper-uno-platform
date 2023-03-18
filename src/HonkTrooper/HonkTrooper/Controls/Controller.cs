using Microsoft.UI;
using Microsoft.UI.Input;
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

        #region Public

        public Button AttackButton { get; set; }

        public Button PauseButton { get; set; }

        #endregion

        #region Private

        private Canvas Thumbstick { get; set; }

        private Construct ThumbstickThumb { get; set; }

        private Construct ThumbstickUpLeft { get; set; }

        private Construct ThumbstickUp { get; set; }

        private Construct ThumbstickUpRight { get; set; }

        private Construct ThumbstickLeft { get; set; }

        private Construct ThumbstickRight { get; set; }

        private Construct ThumbstickDownLeft { get; set; }

        private Construct ThumbstickDown { get; set; }

        private Construct ThumbstickDownRight { get; set; }

        private Gyrometer Gyrometer { get; set; }

        private bool GyrometerReadingsActive { get; set; }

        private double AngularVelocityX { get; set; }

        private double AngularVelocityY { get; set; }

        private double AngularVelocityZ { get; set; }

        private bool IsThumbstickActive { get; set; }

        private Grid Keypad { get; set; }

        #endregion

        #endregion

        #region Ctor

        public Controller()
        {
            CanDrag = false;

            SetAttackButton();
            SetPauseButton();

            KeyUp += Controller_KeyUp;
            KeyDown += Controller_KeyDown;

            SetThumbstick();
            //SetKeypad();            
            //SetGyrometer();
        }

        #endregion

        #region Methods

        #region Thumbstick

        private void SetThumbstick()
        {
            var sizeXY = 3.5;

            Thumbstick = new Canvas()
            {
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE * sizeXY,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE * sizeXY,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Colors.Transparent),
                RenderTransformOrigin = new Point(1, 1),
                //RenderTransform = new ScaleTransform() { CenterX = 0.5, CenterY = 0.5, ScaleX = 0.70, ScaleY = 0.70 }
            };

            var safeZone = new Construct()
            {
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE * 1.55,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE * 1.55,
                Background = new SolidColorBrush(Colors.Goldenrod),
                CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS * sizeXY),
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Opacity = 0.3,
            };

            ThumbstickUpLeft = new()
            {
                Tag = MovementDirection.UpLeft,
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                //Child = new SymbolIcon()
                //{
                //    Symbol = Symbol.Up,
                //},
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },

                //Background = new SolidColorBrush(Colors.Goldenrod),
                //BorderBrush = new SolidColorBrush(Colors.White),
                //BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
            };

            ThumbstickUp = new()
            {
                Tag = MovementDirection.Up,
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE * sizeXY,
                //Child = new SymbolIcon()
                //{
                //    Symbol = Symbol.Up,
                //},
                RenderTransformOrigin = new Point(0.5, 0.5),

                //Background = new SolidColorBrush(Colors.Goldenrod),
                //BorderBrush = new SolidColorBrush(Colors.White),
                //BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
            };

            ThumbstickUpRight = new()
            {
                Tag = MovementDirection.UpRight,
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                //Child = new SymbolIcon()
                //{
                //    Symbol = Symbol.Forward,
                //},
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },

                //Background = new SolidColorBrush(Colors.Goldenrod),
                //BorderBrush = new SolidColorBrush(Colors.White),
                //BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
            };

            ThumbstickLeft = new()
            {
                Tag = MovementDirection.Left,
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE * sizeXY,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                //Child = new SymbolIcon()
                //{
                //    Symbol = Symbol.Back,
                //},
                RenderTransformOrigin = new Point(0.5, 0.5),

                //Background = new SolidColorBrush(Colors.Goldenrod),
                //BorderBrush = new SolidColorBrush(Colors.White),
                //BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
            };

            ThumbstickRight = new()
            {
                Tag = MovementDirection.Right,
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE * sizeXY,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                //Child = new SymbolIcon()
                //{
                //    Symbol = Symbol.Forward,
                //},
                RenderTransformOrigin = new Point(0.5, 0.5),

                //Background = new SolidColorBrush(Colors.Goldenrod),
                //BorderBrush = new SolidColorBrush(Colors.White),
                //BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
            };

            ThumbstickDownLeft = new()
            {
                Tag = MovementDirection.DownLeft,
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                //Child = new SymbolIcon()
                //{
                //    Symbol = Symbol.Back,
                //},
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },

                //Background = new SolidColorBrush(Colors.Goldenrod),
                //BorderBrush = new SolidColorBrush(Colors.White),
                //BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
            };

            ThumbstickDown = new()
            {
                Tag = MovementDirection.Down,
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE * sizeXY,
                //Child = new SymbolIcon()
                //{
                //    Symbol = Symbol.Up
                //},
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = 180 },

                //Background = new SolidColorBrush(Colors.Goldenrod),
                //BorderBrush = new SolidColorBrush(Colors.White),
                //BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
            };

            ThumbstickDownRight = new()
            {
                Tag = MovementDirection.DownRight,
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                //Child = new SymbolIcon()
                //{
                //    Symbol = Symbol.Forward
                //},
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = 45 },

                //Background = new SolidColorBrush(Colors.Goldenrod),
                //BorderBrush = new SolidColorBrush(Colors.White),
                //BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
            };

            ThumbstickThumb = new()
            {
                Tag = MovementDirection.None,
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE * 0.90,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE * 0.90,
                CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS * 2),

                Background = new SolidColorBrush(Colors.Goldenrod),
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
            };

            safeZone.SetPosition(left: Thumbstick.Width / 2 - safeZone.Width / 2, top: Thumbstick.Height / 2 - safeZone.Height / 2);
            Thumbstick.Children.Add(safeZone);

            ThumbstickUpLeft.SetPosition(left: 0, top: 0);
            Thumbstick.Children.Add(ThumbstickUpLeft);

            ThumbstickUp.SetPosition(left: 0 * 1.25, top: 0);
            Thumbstick.Children.Add(ThumbstickUp);

            ThumbstickUpRight.SetPosition(left: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 2 * 1.25, top: 0);
            Thumbstick.Children.Add(ThumbstickUpRight);

            ThumbstickLeft.SetPosition(left: 0, top: 0);
            Thumbstick.Children.Add(ThumbstickLeft);

            ThumbstickRight.SetPosition(left: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 2 * 1.25, top: 0);
            Thumbstick.Children.Add(ThumbstickRight);

            ThumbstickDownLeft.SetPosition(left: 0, top: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 2 * 1.25);
            Thumbstick.Children.Add(ThumbstickDownLeft);

            ThumbstickDown.SetPosition(left: 0 * 1.25, top: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 2 * 1.25);
            Thumbstick.Children.Add(ThumbstickDown);

            ThumbstickDownRight.SetPosition(left: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 2 * 1.25, top: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 2 * 1.25);
            Thumbstick.Children.Add(ThumbstickDownRight);

            SetDefaultThumbstickPosition();
            Thumbstick.Children.Add(ThumbstickThumb);

            Thumbstick.PointerPressed += (s, e) =>
            {
                var point = e.GetCurrentPoint(Thumbstick);
                SetThumbstickThumbPosition(point);

                DeactivateMoveUp();
                DeactivateMoveDown();
                DeactivateMoveLeft();
                DeactivateMoveRight();

                IsThumbstickActive = true;
                ActivateThumbstick();
            };
            Thumbstick.PointerMoved += (s, e) =>
            {
                if (IsThumbstickActive)
                {
                    var point = e.GetCurrentPoint(Thumbstick);
                    SetThumbstickThumbPosition(point);

                    ActivateThumbstick();
                }
            };
            Thumbstick.PointerReleased += (s, e) =>
            {
                DeactivateMoveUp();
                DeactivateMoveDown();
                DeactivateMoveLeft();
                DeactivateMoveRight();

                IsThumbstickActive = false;
                SetDefaultThumbstickPosition();
            };

            this.Children.Add(Thumbstick);
        }

        private void SetThumbstickThumbPosition(PointerPoint point)
        {
            ThumbstickThumb?.SetPosition(
                left: point.Position.X - ThumbstickThumb.Width / 2,
                top: point.Position.Y - ThumbstickThumb.Height / 2);
        }

        public void SetDefaultThumbstickPosition()
        {
            ThumbstickThumb?.SetPosition(
                left: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 1.30,
                top: Constants.DEFAULT_CONTROLLER_KEY_SIZE * 1.30);
        }

        private void MoveThumbstickThumbWithGyrometer(double speedX, double speedY)
        {
            if (ThumbstickThumb is not null)
            {
                ThumbstickThumb.SetLeft(ThumbstickThumb.GetLeft() + speedX);
                ThumbstickThumb.SetTop(ThumbstickThumb.GetTop() + speedY);

                ActivateThumbstick();
            }
        }

        private void ActivateThumbstick()
        {
            if (ThumbstickThumb.GetHitBox().IntersectsWith(ThumbstickUpLeft.GetCloseHitBox()))
            {
                ActivateMoveUp();
                ActivateMoveLeft();
            }
            else
            {
                DeactivateMoveUp();
                DeactivateMoveLeft();
            }

            if (ThumbstickThumb.GetHitBox().IntersectsWith(ThumbstickUpRight.GetCloseHitBox()))
            {
                ActivateMoveUp();
                ActivateMoveRight();
            }
            else
            {
                DeactivateMoveUp();
                DeactivateMoveRight();
            }

            if (ThumbstickThumb.GetHitBox().IntersectsWith(ThumbstickDownLeft.GetCloseHitBox()))
            {
                ActivateMoveDown();
                ActivateMoveLeft();
            }
            else
            {
                DeactivateMoveDown();
                DeactivateMoveLeft();
            }

            if (ThumbstickThumb.GetHitBox().IntersectsWith(ThumbstickDownRight.GetCloseHitBox()))
            {
                ActivateMoveDown();
                ActivateMoveRight();
            }
            else
            {
                DeactivateMoveDown();
                DeactivateMoveRight();
            }

            if (ThumbstickThumb.GetHitBox().IntersectsWith(ThumbstickUp.GetCloseHitBox()))
            {
                ActivateMoveUp();
            }
            else
            {
                DeactivateMoveUp();
            }

            if (ThumbstickThumb.GetHitBox().IntersectsWith(ThumbstickLeft.GetCloseHitBox()))
            {
                ActivateMoveLeft();
            }
            else
            {
                DeactivateMoveLeft();
            }

            if (ThumbstickThumb.GetHitBox().IntersectsWith(ThumbstickRight.GetCloseHitBox()))
            {
                ActivateMoveRight();
            }
            else
            {
                DeactivateMoveRight();
            }

            if (ThumbstickThumb.GetHitBox().IntersectsWith(ThumbstickDown.GetCloseHitBox()))
            {
                ActivateMoveDown();
            }
            else
            {
                DeactivateMoveDown();
            }
        }

        #endregion

        #region Keypad

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

        #endregion

        #region Gyrometer

        public void SetGyrometer()
        {
            Gyrometer = Gyrometer.GetDefault();

            if (Gyrometer is not null)
            {
                GyrometerReadingsActive = false;
                LoggerExtensions.Log($"Gyrometer detected.");
            }
        }

        public void UnsetGyrometer()
        {
            if (Gyrometer is not null)
            {
                LoggerExtensions.Log($"Gyrometer detected.");
                DeactivateGyrometerReading();
            }
        }

        public void ActivateGyrometerReading()
        {
            if (!GyrometerReadingsActive && Gyrometer is not null)
            {
                Gyrometer.ReportInterval = (int)Constants.DEFAULT_FRAME_TIME;
                GyrometerReadingsActive = true;
                Gyrometer.ReadingChanged += Gyrometer_ReadingChanged;
            }
        }

        public void DeactivateGyrometerReading()
        {
            if (GyrometerReadingsActive && Gyrometer is not null)
            {
                Gyrometer.ReportInterval = 0;
                GyrometerReadingsActive = false;
                Gyrometer.ReadingChanged -= Gyrometer_ReadingChanged;
            }
        }

        private void Gyrometer_ReadingChanged(Gyrometer sender, GyrometerReadingChangedEventArgs args)
        {
            if (GyrometerReadingsActive)
            {
                AngularVelocityX = args.Reading.AngularVelocityX;
                AngularVelocityY = args.Reading.AngularVelocityY;
                AngularVelocityZ = args.Reading.AngularVelocityZ;

                LoggerExtensions.Log($"AngularVelocityX: {AngularVelocityX}");
                LoggerExtensions.Log($"AngularVelocityY: {AngularVelocityY}");
                LoggerExtensions.Log($"AngularVelocityZ: {AngularVelocityZ}");

#if __ANDROID__ || __IOS__
                MoveThumbstickThumbWithGyrometer(AngularVelocityX / 2.0, AngularVelocityY * -1 / 2.0);
#else
                MoveThumbstickThumbWithGyrometer(AngularVelocityX / 1.5, AngularVelocityY * -1 / 1.5);
#endif
            }
        }

        #endregion

        #region Attack

        private void SetAttackButton()
        {
            AttackButton = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE * 1.2,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE * 1.2,
                CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS * 2),
                Content = new SymbolIcon()
                {
                    Symbol = Symbol.Target,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(40),
            };

            AttackButton.Click += (s, e) => { ActivateAttack(); };
            this.Children.Add(AttackButton);
        }

        #endregion

        #region Pause

        private void SetPauseButton()
        {
            PauseButton = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE * 0.75,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE * 0.75,
                CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS * 2),
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

        #endregion

        #endregion
    }
}
