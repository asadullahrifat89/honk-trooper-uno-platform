using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class FloatingNumber : MovableConstruct
    {
        #region Fields

        private readonly TextBlock _textBlock;

        #endregion

        #region Ctor

        public FloatingNumber(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.FLOATING_NUMBER;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetConstructSize();

            _textBlock = new TextBlock();

            SetChild(_textBlock);

            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
        }

        #endregion

        #region Methods

        public void Reset(double number)
        {
            _textBlock.Text = number.ToString("00");
        }

        public void Reposition(Construct source)
        {
            SetPosition(
                left: (source.GetLeft() + source.Width / 2) - Width / 2,
                top: source.GetTop() - (20));
        }

        #endregion
    }
}
