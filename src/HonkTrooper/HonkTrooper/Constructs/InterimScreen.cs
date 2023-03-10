using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class InterimScreen : Construct
    {
        #region Fields

        private double _hoverDelay;
        private readonly double _hoverDelayDefault = 15;

        private readonly double _hoverSpeed = 0.3;

        private readonly TextBlock _titleScreenText;

        private double _messageOnScreenDelay;
        private readonly double _messageOnScreenDelayDefault = 20;

        #endregion

        #region Ctor

        public InterimScreen(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            ConstructType = ConstructType.INTERIM_SCREEN;

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.INTERIM_SCREEN);

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            SetSize(width: width, height: height);

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            CornerRadius = new CornerRadius(5);

            IsometricDisplacement = 0.5;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE;

            _titleScreenText = new TextBlock()
            {
                Text = "Honk Trooper",
                FontSize = Constants.DEFAULT_GUI_FONT_SIZE,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5),
                Foreground = new SolidColorBrush(Colors.White),
            };

            SetChild(_titleScreenText);
        }

        #endregion

        #region Properties

        public bool IsDepleted => _messageOnScreenDelay <= 0;

        #endregion

        #region Methods

        public void Reset()
        {
            _messageOnScreenDelay = _messageOnScreenDelayDefault;
        }

        public bool DepleteOnScreenDelay()
        {
            _messageOnScreenDelay -= 0.1;
            return true;
        }

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
