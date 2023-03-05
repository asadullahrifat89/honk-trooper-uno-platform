using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Foundation;

namespace HonkPooper
{
    public partial class Controller : ControlerBase
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

        #endregion

        #region Ctor

        public Controller()
        {
            //PointerPressed += Controller_PointerPressed;
            //PointerMoved += Controller_PointerMoved;
            KeyUp += Controller_KeyUp;
            KeyDown += Controller_KeyDown;

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

            Grid.SetRow(up, 0);
            Grid.SetColumn(up, 1);

            Grid.SetRow(left, 1);
            Grid.SetColumn(left, 0);

            Grid.SetRow(right, 1);
            Grid.SetColumn(right, 2);

            Grid.SetRow(down, 2);
            Grid.SetColumn(down, 1);

            ArrowsKeysContainer.Children.Add(up);
            ArrowsKeysContainer.Children.Add(down);
            ArrowsKeysContainer.Children.Add(left);
            ArrowsKeysContainer.Children.Add(right);

            this.Children.Add(ArrowsKeysContainer);

            Button attack = new()
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

            attack.Click += (s, e) => { ActivateAttack(); };
            this.Children.Add(attack);
        }

        #endregion

        #region Methods

        public void SetArrowsKeysContainerRotation(double rotation)
        {
            _arrowsKeysContainerTransform.Rotation = rotation;
        }

        #endregion
    }
}
