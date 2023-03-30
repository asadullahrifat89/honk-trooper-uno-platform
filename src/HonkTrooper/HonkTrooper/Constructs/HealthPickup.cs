using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class HealthPickup : MovableConstruct
    {
        #region Fields

        private readonly Random _random;
        private readonly Image _content_image;


        private readonly AudioStub _audioStub;

        #endregion

        #region Ctor

        public HealthPickup(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.HEALTH_PICKUP;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            _random = new Random();

            SetConstructSize();

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.HEALTH_PICKUP).Uri)
            };

            SetChild(_content_image);

            SpeedOffset = 0;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE;
            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;

            _audioStub = new AudioStub((SoundType.HEALTH_PICKUP, 1, false));
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

        public void PickedUp()
        {
            _audioStub.Play(SoundType.HEALTH_PICKUP);

            IsPickedUp = true;
        }

        #endregion

        #region Properties

        public bool IsPickedUp { get; set; }

        #endregion
    }
}
