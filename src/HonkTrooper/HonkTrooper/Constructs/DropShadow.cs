using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class DropShadow : Construct
    {
        #region Properties

        public Construct ParentConstruct { get; set; }

        public double ParentConstructSpeed { get; set; } = 0;

        #endregion

        #region Ctor

        public DropShadow(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.DROP_SHADOW);

            ConstructType = ConstructType.DROP_SHADOW;

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            Height = 25 * downScaling;

            SetSize(width: width, height: height);

            Background = new SolidColorBrush(Colors.DarkGray);
            CornerRadius = new CornerRadius(100);
            Opacity = 0.8;

            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
            IsometricDisplacement = 0.5;
        }

        #endregion

        #region Methods

        public void SetParent(Construct construct)
        {
            // linking this shadow instance with a construct

            Id = construct.Id;
            ParentConstruct = construct;
        }

        public void Reset()
        {
            SetPosition(
                left: (ParentConstruct.GetLeft() + ParentConstruct.Width / 2) - Width / 2,
                top: ParentConstruct.GetBottom() + (ParentConstruct.DropShadowDistance));
        }

        public void SyncWidth()
        {
            // adjust shadow with with the source
            if (Width != ParentConstruct.Width * 0.6)
                Width = ParentConstruct.Width * 0.6;
        }

        public void Move()
        {
            SetLeft((ParentConstruct.GetLeft() + ParentConstruct.Width / 2) - Width / 2);

            if (ParentConstruct.IsGravitating)
            {
                SetTop(GetTop() + ParentConstructSpeed * IsometricDisplacement);
            }
            else
            {
                SetTop(ParentConstruct.GetBottom() + ParentConstruct.DropShadowDistance);
            }
        }

        #endregion      
    }
}
