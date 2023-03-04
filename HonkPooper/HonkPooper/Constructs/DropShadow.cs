using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Linq;

namespace HonkPooper
{
    public partial class DropShadow : Construct
    {
        #region Properties

        public Construct Source { get; set; }

        #endregion

        #region Ctor

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

            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
        }

        #endregion

        #region Methods

        public void SetParent(Construct construct, double downScaling)
        {
            Source = construct;

            // linking this shadow instance with a construct
            Id = Source.Id;

            //_origin = new((Source.GetLeft() + Source.Width / 2) - Width / 2, Source.GetBottom() + (Source.DropShadowDistance * downScaling));
        }

        public void Move(double downScaling)
        {
            if (Source.IsGravitating)
            {
                SetPosition(
                    left: (Source.GetLeft() + Source.Width / 2) - Width / 2,
                    top: Source.GetBottom() + (Source.DropShadowDistance * downScaling));
            }
            else
            {
                SetPosition(
                    left: (Source.GetLeft() + Source.Width / 2) - Width / 2,
                    top: Source.GetBottom() + (Source.DropShadowDistance * downScaling));
            }
        }

        #endregion      
    }
}
