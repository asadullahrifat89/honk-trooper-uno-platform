using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Linq;

namespace HonkPooper
{
    public partial class DropShadow : Construct
    {
        public DropShadow(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.PLAYER_DROP_ZONE);

            ConstructType = ConstructType.PLAYER_DROP_ZONE;

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            Background = new SolidColorBrush(Colors.Gray);
            CornerRadius = new CornerRadius(30);
            Opacity = 0.8;
        }

        public void Move(Construct parent, double downScaling)
        {
            SetPosition(
                   left: (parent.GetLeft() + parent.Width / 2) - Width / 2,
                   top: parent.GetBottom() + (50 * downScaling));
        }
    }
}
