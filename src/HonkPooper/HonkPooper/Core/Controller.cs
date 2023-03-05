﻿using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace HonkPooper
{
    public partial class Controller : ControllerBase
    {
        #region Fields

        private int _keysSize = 55;

        private readonly CompositeTransform _arrowsKeysContainerTransform = new()
        {
            CenterX = 0.5,
            CenterY = 0.5,
            Rotation = 0,
            ScaleX = 1,
            ScaleY = 1,
        };

        #endregion

        #region Properties

        public Grid ArrowsKeysContainer { get; set; }

        public Button StartButton { get; set; }
        public Button AttackButton { get; set; }

        #endregion

        #region Ctor

        public Controller()
        {
            //PointerPressed += Controller_PointerPressed;
            //PointerMoved += Controller_PointerMoved;

            KeyUp += Controller_KeyUp;
            KeyDown += Controller_KeyDown;

            SetupArrowKeysContainer();
            SetupAttackButton();
            SetupStartButton();
        }

        #endregion

        #region Methods

        public void SetArrowsKeysContainerRotation(double rotation)
        {
            _arrowsKeysContainerTransform.Rotation = rotation;
        }

        private void SetupStartButton()
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

        private void SetupAttackButton()
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

        private void SetupArrowKeysContainer()
        {
            ArrowsKeysContainer = new()
            {
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Bottom,
                RenderTransform = _arrowsKeysContainerTransform,
            };

            ArrowsKeysContainer.RowDefinitions.Add(new RowDefinition());
            ArrowsKeysContainer.RowDefinitions.Add(new RowDefinition());
            ArrowsKeysContainer.RowDefinitions.Add(new RowDefinition());

            ArrowsKeysContainer.ColumnDefinitions.Add(new ColumnDefinition());
            ArrowsKeysContainer.ColumnDefinitions.Add(new ColumnDefinition());
            ArrowsKeysContainer.ColumnDefinitions.Add(new ColumnDefinition());

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

            SetRow(up, 0);
            SetColumn(up, 1);

            SetRow(left, 1);
            SetColumn(left, 0);

            SetRow(right, 1);
            SetColumn(right, 2);

            SetRow(down, 2);
            SetColumn(down, 1);

            ArrowsKeysContainer.Children.Add(up);
            ArrowsKeysContainer.Children.Add(down);
            ArrowsKeysContainer.Children.Add(left);
            ArrowsKeysContainer.Children.Add(right);

            this.Children.Add(ArrowsKeysContainer);
        }

        #endregion
    }
}
