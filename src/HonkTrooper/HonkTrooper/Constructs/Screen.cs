using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Linq;
using Microsoft.UI.Xaml.Controls;

namespace HonkTrooper
{
    public partial class Screen : Construct
    {
        #region Fields

        private double _hoverDelay;
        private readonly double _hoverDelayDefault = 15;

        private readonly double _hoverSpeed = 0.3;

        #endregion

        #region Ctor

        public Screen(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.SCREEN);

            ConstructType = ConstructType.SCREEN;

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            //Background = new SolidColorBrush(Colors.Goldenrod);
            CornerRadius = new CornerRadius(5);

            IsometricDisplacement = 0.5;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET * 3;
        }

        #endregion

        #region Properties

        public bool AwaitMoveDown { get; set; }

        public bool AwaitMoveUp { get; set; }

        #endregion

        #region Methods

        public void Reset()
        {
            AwaitMoveDown = false;
            AwaitMoveUp = false;
        }

        public void MoveDown(double speed)
        {
            SetTop(GetTop() + speed);
        }

        public void MoveUp(double speed)
        {
            SetTop(GetTop() - speed);
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
