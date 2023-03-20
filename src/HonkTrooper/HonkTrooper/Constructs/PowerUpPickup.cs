using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class PowerUpPickup : Construct
    {
        #region Fields

        private readonly Random _random;
        private readonly Image _content_image;

        private readonly AudioStub _audioStub;
        #endregion

        #region Ctor

        public PowerUpPickup(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP);

            ConstructType = ConstructType.POWERUP_PICKUP;

            _random = new Random();

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            PowerUpType = (PowerUpType)_random.Next( Enum.GetNames(typeof(PowerUpType)).Length);

            switch (PowerUpType)
            {
                case PowerUpType.SEEKING_BALLS:
                    {
                        _content_image = new Image()
                        {
                            Source = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_SEEKING_BALLS).Uri)
                        };
                    }
                    break;
                case PowerUpType.FORCE_SHIELD:
                    {
                        _content_image = new Image()
                        {
                            Source = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_FORCE_SHIELD).Uri)
                        };
                    }
                    break;
                default:
                    break;
            }

            SetChild(_content_image);

            SpeedOffset = 0;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE;
            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;

            _audioStub = new AudioStub((SoundType.POWERUP_PICKUP, 1, false));
        }

        #endregion

        #region Methods

        public void Reset()
        {
            IsPickedUp = false;

            PowerUpType = (PowerUpType)_random.Next( Enum.GetNames(typeof(PowerUpType)).Length);

            switch (PowerUpType)
            {
                case PowerUpType.SEEKING_BALLS:
                    {
                        _content_image.Source = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_SEEKING_BALLS).Uri);
                    }
                    break;
                case PowerUpType.FORCE_SHIELD:
                    {
                        _content_image.Source = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_FORCE_SHIELD).Uri);
                    }
                    break;
                default:
                    break;
            }

            SetChild(_content_image);
        }

        public void PickedUp()
        {
            _audioStub.Play(SoundType.POWERUP_PICKUP);

            IsPickedUp = true;
        }

        #endregion

        #region Properties

        public bool IsPickedUp { get; set; }

        public PowerUpType PowerUpType { get; set; }

        #endregion
    }

    public enum PowerUpType
    {
        SEEKING_BALLS,
        FORCE_SHIELD,
    }
}
