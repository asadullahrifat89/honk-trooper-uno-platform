using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class TitleScreen : Construct
    {
        #region Fields

        private double _hoverDelay;
        private readonly double _hoverDelayDefault = 15;

        private readonly double _hoverSpeed = 0.3;

        private TextBlock _titleScreenText;

        #endregion

        #region Ctor

        public TitleScreen
            (Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            Func<bool> playAction,
            double downScaling)
        {
            ConstructType = ConstructType.TITLE_SCREEN;

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.TITLE_SCREEN);

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            SetSize(width: width, height: height);

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            CornerRadius = new CornerRadius(5);

            IsometricDisplacement = 0.5;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
            DropShadowDistance = 50;

            Grid grid = new();
            grid.Children.Add(new Border()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                CornerRadius = new CornerRadius(15),
                Opacity = 0.6,
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
            });

            StackPanel container = new()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            Image titleScreenIcon = new Image()
            {
                Source = new BitmapImage(Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.PLAYER).Uri),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(0, 0, 0, 5),
                Height = 110,
                Width = 110,
            };

            container.Children.Add(titleScreenIcon);

            _titleScreenText = new TextBlock()
            {
                Text = "Honk Trooper",
                FontSize = Constants.DEFAULT_GUI_FONT_SIZE,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 5),
                Foreground = new SolidColorBrush(Colors.White),
            };

            container.Children.Add(_titleScreenText);

            Button playButton = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS),
                Content = new SymbolIcon()
                {
                    Symbol = Symbol.Play,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Foreground = new SolidColorBrush(Colors.White),
            };

            playButton.Click += (s, e) => { playAction(); };

            container.Children.Add(playButton);

            grid.Children.Add(container);

            SetChild(grid);
        }

        #endregion      

        #region Methods

        public void Reposition()
        {
            SetPosition(
                  left: ((Scene.Width / 4) * 2) - Width / 2,
                  top: (Scene.Height / 2) - Height / 2,
                  z: 10);
        }

        public void SetTitle(string title)
        {
            _titleScreenText.Text = title;
        }

        public void Hover()
        {
            if (Scene.IsSlowMotionActivated)
            {
                _hoverDelay -= 0.5;

                if (_hoverDelay > 0)
                {
                    SetTop(GetTop() + _hoverSpeed / Constants.DEFAULT_SLOW_MOTION_REDUCTION_FACTOR);
                }
                else
                {
                    SetTop(GetTop() - _hoverSpeed / Constants.DEFAULT_SLOW_MOTION_REDUCTION_FACTOR);

                    if (_hoverDelay <= _hoverDelayDefault * -1)
                        _hoverDelay = _hoverDelayDefault;
                }
            }

            else
            {
                _hoverDelay--;

                if (_hoverDelay > 0)
                {
                    SetTop(GetTop() + _hoverSpeed);
                }
                else
                {
                    SetTop(GetTop() - _hoverSpeed);

                    if (_hoverDelay <= _hoverDelayDefault * -1)
                        _hoverDelay = _hoverDelayDefault;
                }
            }
        }

        #endregion
    }
}
