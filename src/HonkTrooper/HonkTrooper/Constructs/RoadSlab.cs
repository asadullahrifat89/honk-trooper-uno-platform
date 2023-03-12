using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class RoadSlab : Construct
    {
        #region Ctor

        public RoadSlab(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            ConstructType = ConstructType.ROAD_SLAB;

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.ROAD_SLAB);

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            SetSize(width: width, height: height);

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            Background = new SolidColorBrush(Colors.Gray);
            CornerRadius = new CornerRadius(10);

            SetSkewY(42);
            SetRotation(-63.5);

            IsometricDisplacement = 0.5;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
        } 

        #endregion
    }
}

