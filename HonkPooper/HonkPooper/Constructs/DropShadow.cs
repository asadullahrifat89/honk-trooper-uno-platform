using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Linq;
using System.Diagnostics.Contracts;

namespace HonkPooper
{
    public partial class DropShadow : Construct
    {
        #region Properties

        public Construct Source { get; set; }

        public double SourceSpeed { get; set; } = 0;

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

            Background = new SolidColorBrush(Colors.Gray);
            CornerRadius = new CornerRadius(30);
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
            Source = construct;            
        }

        public void Reset()
        {
            SetPosition(
                left: (Source.GetLeft() + Source.Width / 2) - Width / 2,
                top: Source.GetBottom() + (Source.DropShadowDistance));
        }

        public void Move()
        {
            SetLeft((Source.GetLeft() + Source.Width / 2) - Width / 2);

            if (Source.IsGravitating)
            {
                SetTop(GetTop() + SourceSpeed * IsometricDisplacement);
            }
            else
            {
                SetTop(Source.GetBottom() + Source.DropShadowDistance);
            }
        }

        #endregion      
    }
}
