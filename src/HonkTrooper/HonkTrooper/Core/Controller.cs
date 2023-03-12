using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.UI.Core;

namespace HonkTrooper
{
    public partial class Controller : ControllerBase
    {

        #region Fields

        private int _move_up_activator;
        private int _move_down_activator;
        private int _move_left_activator;
        private int _move_right_activator;

        #endregion

        #region Properties

        public Grid Joystick { get; set; }

        public Button AttackButton { get; set; }

        public Button PauseButton { get; set; }

        public Gyrometer Gyrometer { get; set; }

        public Accelerometer Accelerometer { get; set; }

        public double AngularVelocityX { get; set; }

        public double AngularVelocityY { get; set; }

        public double AngularVelocityZ { get; set; }

        //public double AccelerationX { get; set; }

        //public double AccelerationY { get; set; }

        //public double AccelerationZ { get; set; }

        #endregion

        #region Ctor

        public Controller()
        {
            CanDrag = false;

            SetAttackButton();
            SetPauseButton();

            KeyUp += Controller_KeyUp;
            KeyDown += Controller_KeyDown;

            SetJoyStick();
            SetGyrometer();
            //SetAccelerometer();
            
        }

        #endregion

        #region Methods

        public void SetJoyStick()
        {
            Joystick = new()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(20),
            };

            #region Isometric Movement

            #region Placeholder

            Joystick.RowDefinitions.Add(new RowDefinition());
            Joystick.RowDefinitions.Add(new RowDefinition());

            Joystick.ColumnDefinitions.Add(new ColumnDefinition());
            Joystick.ColumnDefinitions.Add(new ColumnDefinition());

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
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },
                Margin = new Thickness(Constants.DEFAULT_CONTROLLER_DIRECTION_KEYS_MARGIN),
            };

            up.PointerEntered += (s, e) => { ActivateMoveUp(); };
            up.PointerExited += (s, e) => { DeactivateMoveUp(); };

            Grid.SetRow(up, 0);
            Grid.SetColumn(up, 0);

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

            Grid.SetRow(down, 1);
            Grid.SetColumn(down, 1);

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
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },
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
                RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },
                Margin = new Thickness(Constants.DEFAULT_CONTROLLER_DIRECTION_KEYS_MARGIN),
            };

            right.PointerEntered += (s, e) => { ActivateMoveRight(); };
            right.PointerExited += (s, e) => { DeactivateMoveRight(); };

            Grid.SetRow(right, 0);
            Grid.SetColumn(right, 1);

            #endregion

            #endregion

            #region Linear Movement

            //#region Placeholder

            //Joystick.RowDefinitions.Add(new RowDefinition());
            //Joystick.RowDefinitions.Add(new RowDefinition());
            //Joystick.RowDefinitions.Add(new RowDefinition());

            //Joystick.ColumnDefinitions.Add(new ColumnDefinition());
            //Joystick.ColumnDefinitions.Add(new ColumnDefinition());
            //Joystick.ColumnDefinitions.Add(new ColumnDefinition()); 

            //#endregion

            //#region Up

            //Border up = new()
            //{
            //    Background = new SolidColorBrush(Colors.Goldenrod),
            //    Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
            //    Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
            //    CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS),
            //    Child = new SymbolIcon()
            //    {
            //        Symbol = Symbol.Up,
            //    },
            //    BorderBrush = new SolidColorBrush(Colors.White),
            //    BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
            //    RenderTransformOrigin = new Point(0.5, 0.5),
            //    //RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },
            //    Margin = new Thickness(Constants.DEFAULT_CONTROLLER_DIRECTION_KEYS_MARGIN),
            //};

            //up.PointerEntered += (s, e) => { ActivateMoveUp(); };
            //up.PointerExited += (s, e) => { DeactivateMoveUp(); };

            //Grid.SetRow(up, 0);
            //Grid.SetColumn(up, 1);

            //#endregion

            //#region Down

            //Border down = new()
            //{
            //    Background = new SolidColorBrush(Colors.Goldenrod),
            //    Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
            //    Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
            //    CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS),
            //    Child = new SymbolIcon()
            //    {
            //        Symbol = Symbol.Up
            //    },
            //    BorderBrush = new SolidColorBrush(Colors.White),
            //    BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
            //    RenderTransformOrigin = new Point(0.5, 0.5),
            //    RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = 180 },
            //    Margin = new Thickness(Constants.DEFAULT_CONTROLLER_DIRECTION_KEYS_MARGIN),
            //};

            //down.PointerEntered += (s, e) => { ActivateMoveDown(); };
            //down.PointerExited += (s, e) => { DeactivateMoveDown(); };

            //Grid.SetRow(down, 2);
            //Grid.SetColumn(down, 1);

            //#endregion

            //#region Left

            //Border left = new()
            //{
            //    Background = new SolidColorBrush(Colors.Goldenrod),
            //    Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
            //    Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
            //    CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS),
            //    Child = new SymbolIcon()
            //    {
            //        Symbol = Symbol.Back,
            //    },
            //    BorderBrush = new SolidColorBrush(Colors.White),
            //    BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
            //    RenderTransformOrigin = new Point(0.5, 0.5),
            //    //RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },
            //    Margin = new Thickness(Constants.DEFAULT_CONTROLLER_DIRECTION_KEYS_MARGIN),
            //};

            //left.PointerEntered += (s, e) => { ActivateMoveLeft(); };
            //left.PointerExited += (s, e) => { DeactivateMoveLeft(); };

            //Grid.SetRow(left, 1);
            //Grid.SetColumn(left, 0);

            //#endregion

            //#region Right

            //Border right = new()
            //{
            //    Background = new SolidColorBrush(Colors.Goldenrod),
            //    Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
            //    Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
            //    CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS),
            //    Child = new SymbolIcon()
            //    {
            //        Symbol = Symbol.Forward,
            //    },
            //    BorderBrush = new SolidColorBrush(Colors.White),
            //    BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
            //    RenderTransformOrigin = new Point(0.5, 0.5),
            //    //RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = -45 },
            //    Margin = new Thickness(Constants.DEFAULT_CONTROLLER_DIRECTION_KEYS_MARGIN),
            //};

            //right.PointerEntered += (s, e) => { ActivateMoveRight(); };
            //right.PointerExited += (s, e) => { DeactivateMoveRight(); };

            //Grid.SetRow(right, 1);
            //Grid.SetColumn(right, 2);

            //#endregion

            #endregion

            Joystick.Children.Add(up);
            Joystick.Children.Add(down);
            Joystick.Children.Add(left);
            Joystick.Children.Add(right);

            this.Children.Add(Joystick);
        }

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

        //public void SetAccelerometer()
        //{
        //    Accelerometer = Accelerometer.GetDefault();

        //    if (Accelerometer is not null)
        //    {
        //        // Console.WriteLine($"Gyrometer detected.");
        //        Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
        //    }
        //}
     
        #endregion

        #region Events

        private void Gyrometer_ReadingChanged(Gyrometer sender, GyrometerReadingChangedEventArgs args)
        {
            //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //{
            AngularVelocityX = args.Reading.AngularVelocityX;
            AngularVelocityY = args.Reading.AngularVelocityY;
            AngularVelocityZ = args.Reading.AngularVelocityZ;
            //});
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

            #region Isometric Movement V2

            //if (AngularVelocityX > 0)
            //{
            //    if (AngularVelocityX > 15)
            //    {
            //        ActivateMoveDown();
            //    }
            //    else
            //    {
            //        DeactivateMoveUp();
            //    }
            //}
            //else
            //{
            //    if (Math.Abs(AngularVelocityX) > 15)
            //    {
            //        ActivateMoveUp();
            //    }
            //    else
            //    {
            //        DeactivateMoveUp();
            //    }
            //}

            //if (AngularVelocityY > 0)
            //{
            //    if (AngularVelocityY > 10)
            //    {
            //        ActivateMoveRight();
            //    }
            //    else
            //    {
            //        DeactivateMoveLeft();
            //    }
            //}
            //else
            //{
            //    if (Math.Abs(AngularVelocityY) > 10)
            //    {
            //        ActivateMoveLeft();
            //    }
            //    else
            //    {
            //        DeactivateMoveRight();
            //    }
            //}

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

        //private void Accelerometer_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
        //{
        //    AccelerationX = args.Reading.AccelerationX;
        //    AccelerationY = args.Reading.AccelerationY;
        //    AccelerationZ = args.Reading.AccelerationZ;

        //    Console.WriteLine($"AccelerationX: {AccelerationX}");
        //    Console.WriteLine($"AccelerationY: {AccelerationY}");
        //    Console.WriteLine($"AccelerationZ: {AccelerationZ}");

        //    if (AccelerationX > 0)
        //    {
        //        if (AccelerationX > 15)
        //        {
        //            ActivateMoveDown();
        //        }
        //        else
        //        {
        //            DeactivateMoveUp();
        //            DeactivateMoveDown();
        //        }
        //    }
        //    else
        //    {
        //        if (Math.Abs(AccelerationX) > 15)
        //        {
        //            ActivateMoveUp();
        //        }
        //        else
        //        {
        //            DeactivateMoveUp();
        //            DeactivateMoveDown();
        //        }
        //    }

        //    if (AccelerationY > 0)
        //    {
        //        if (AccelerationY > 10)
        //        {
        //            ActivateMoveRight();
        //        }
        //        else
        //        {
        //            DeactivateMoveLeft();
        //            DeactivateMoveRight();
        //        }
        //    }
        //    else
        //    {
        //        if (Math.Abs(AccelerationY) > 10)
        //        {
        //            ActivateMoveLeft();
        //        }
        //        else
        //        {
        //            DeactivateMoveRight();
        //            DeactivateMoveLeft();
        //        }
        //    }
        //}


        #endregion
    }
}
