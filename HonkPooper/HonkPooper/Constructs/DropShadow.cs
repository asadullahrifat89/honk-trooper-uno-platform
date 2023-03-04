using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Linq;

namespace HonkPooper
{
    public partial class DropShadow : Construct
    {
        private Construct _parent;

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

        public void SetParent(Construct construct)
        {
            _parent = construct;

            // linking this shadow instance with a construct
            Id = _parent.Id;
        }

        public void Move(double downScaling)
        {
            SetPosition(
                   left: (_parent.GetLeft() + _parent.Width / 2) - Width / 2,
                   top: _parent.GetBottom() + (_parent.DropShadowDistance * downScaling));
        }
    }
}
