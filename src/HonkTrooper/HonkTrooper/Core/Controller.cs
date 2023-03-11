using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using Windows.Foundation;

namespace HonkTrooper
{
    public partial class Controller : ControllerBase
    {
        #region Properties

        public Grid DirectionButtons { get; set; }

        public Button AttackButton { get; set; }

        public Button PauseButton { get; set; }

        #endregion

        #region Ctor

        public Controller()
        {
            CanDrag = false;

            KeyUp += Controller_KeyUp;
            KeyDown += Controller_KeyDown;

            Setup();
        }

        #endregion

        #region Methods

        public void Setup()
        {
            SetDirectionKeys();
            SetAttackButton();
            SetPauseButton();
        }

        private void SetDirectionKeys()
        {
            DirectionButtons = new()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(20),
            };

            DirectionButtons.RowDefinitions.Add(new RowDefinition());
            DirectionButtons.RowDefinitions.Add(new RowDefinition());

            DirectionButtons.ColumnDefinitions.Add(new ColumnDefinition());
            DirectionButtons.ColumnDefinitions.Add(new ColumnDefinition());

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

            DirectionButtons.Children.Add(up);
            DirectionButtons.Children.Add(down);
            DirectionButtons.Children.Add(left);
            DirectionButtons.Children.Add(right);

            this.Children.Add(DirectionButtons);
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

        #endregion
    }
}
