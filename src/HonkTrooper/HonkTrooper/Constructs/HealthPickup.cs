using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class HealthPickup : Construct
    {
        #region Ctor

        public HealthPickup(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.HEALTH_PICKUP);

            ConstructType = ConstructType.HEALTH_PICKUP;

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var content = new Image()
            {
                Source = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.HEALTH_PICKUP).Uri)
            };

            SetChild(content);
            SpeedOffset = 0;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE;
            IsometricDisplacement = 0.5;
        }

        #endregion

        #region Methods

        public static bool ShouldGenerate(double playerHealth)
        {
            return playerHealth <= 80;
        }

        public void Reset()
        {
            IsPickedUp = false;
        }

        #endregion

        #region Properties

        public bool IsPickedUp { get; set; }

        #endregion
    }
}
