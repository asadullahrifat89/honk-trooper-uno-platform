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
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            ConstructType = ConstructType.ROAD_SIDE_STRIPE;

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.ROAD_SIDE_STRIPE);

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            SetSize(width: width, height: height);

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            Background = new SolidColorBrush(Colors.Goldenrod);
            CornerRadius = new CornerRadius(5);
            BorderThickness = new Thickness(leftRight: Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS, topBottom: 0);
            BorderBrush = App.Current.Resources["BorderColor"] as SolidColorBrush;

            SetSkewY(42);
            SetRotation(-63.5);

            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
        }

        #endregion
    }
}
