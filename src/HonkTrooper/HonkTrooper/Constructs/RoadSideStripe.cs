using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System;
using System.Linq;
using Microsoft.UI;

namespace HonkTrooper
{
    public partial class RoadSideStripe : Construct
    {
        #region Ctor

        public RoadSideStripe(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.ROAD_SIDE_STRIPE;

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.ROAD_SIDE_STRIPE);

            var width = size.Width;
            var height = size.Height;

            SetSize(width: width, height: height);

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            Background = new SolidColorBrush(Colors.White);

            SetSkewY(42);
            SetRotation(-63.5);
            CornerRadius = 5;

            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
        }

        #endregion
    }
}
