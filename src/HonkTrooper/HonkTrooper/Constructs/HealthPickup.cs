using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class HealthPickup : Construct
    {
        #region Fields
        
        private readonly Random _random;
        private readonly Image _content_image;

        private readonly Audio[] _health_pickup_sounds; 

        #endregion

        #region Ctor

        public HealthPickup(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            _random = new Random();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.HEALTH_PICKUP);

            ConstructType = ConstructType.HEALTH_PICKUP;

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.HEALTH_PICKUP).Uri)
            };

            SetChild(_content_image);

            SpeedOffset = 0;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE;
            IsometricDisplacement = 0.5;

            _health_pickup_sounds = Constants.SOUND_TEMPLATES.Where(x => x.SoundType == SoundType.HEALTH_PICKUP).Select(x => x.Uri).Select(uri => new Audio(uri: uri)).ToArray();
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
            var sound = _health_pickup_sounds[_random.Next(0, _health_pickup_sounds.Length)];
            sound.Play();

            IsPickedUp = true;
        }

        #endregion

        #region Properties

        public bool IsPickedUp { get; set; }

        #endregion
    }
}
