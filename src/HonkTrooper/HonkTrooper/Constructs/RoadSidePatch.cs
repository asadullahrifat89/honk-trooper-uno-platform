using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class RoadSidePatch : Construct
    {
        #region Ctor

        public RoadSidePatch(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.ROAD_SIDE_PATCH;

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.ROAD_SIDE_PATCH);

            var width = size.Width;
            var height = size.Height;

            SetSize(width: width, height: height);

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

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
