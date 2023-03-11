using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Linq;
using Microsoft.UI.Xaml.Controls;

namespace HonkTrooper
{
    public partial class ScreenElement : Construct
    {
        #region Fields

        private double _hoverDelay;
        private readonly double _hoverDelayDefault = 15;

        private readonly double _hoverSpeed = 0.3;

        #endregion

        #region Ctor

        public ScreenElement
            (Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling,
            ConstructType constructType)
        {
            ConstructType = constructType;

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == constructType);

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            SetSize(width: width, height: height);

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            CornerRadius = new CornerRadius(5);

            IsometricDisplacement = 0.5;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
            DropShadowDistance = 50;
        }

        #endregion

        #region Properties

        public bool AwaitMoveDown { get; set; }

        public bool AwaitMoveUp { get; set; }

        #endregion

        #region Methods


        public void Reposition()
        {
            SetPosition(
                  left: ((Scene.Width / 4) * 2) - Width / 2,
                  top: (Scene.Height / 2) + 10 * Scene.DownScaling,
                  z: 10);
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
