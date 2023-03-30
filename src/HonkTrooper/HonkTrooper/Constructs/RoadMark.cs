using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class RoadMark : MovableConstruct
    {
        #region Ctor

        public RoadMark(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.ROAD_MARK;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetConstructSize();

            Background = new SolidColorBrush(Colors.White);
            CornerRadius = new CornerRadius(5);

            SetSkewY(42);
            SetRotation(-63.5);

            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
        }

        #endregion
    }
}
