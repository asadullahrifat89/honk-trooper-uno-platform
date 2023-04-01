using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class PowerUpPickup : MovableConstruct
    {
        #region Fields

        private readonly Random _random;
        private readonly Image _content_image;
        private readonly BitmapImage _bitmapImage;

        private readonly AudioStub _audioStub;
        #endregion

        #region Ctor

        public PowerUpPickup(
            Action<Construct> animateAction,
            Action<Construct> recycleAction)
        {
            ConstructType = ConstructType.POWERUP_PICKUP;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            _random = new Random();

            SetConstructSize();

            PowerUpType = (PowerUpType)_random.Next(Enum.GetNames(typeof(PowerUpType)).Length);

            switch (PowerUpType)
            {
                case PowerUpType.SEEKING_SNITCH:
                    {
                        _bitmapImage = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_SEEKING_SNITCH).Uri);
                    }
                    break;
                case PowerUpType.ARMOR:
                    {
                        _bitmapImage = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_ARMOR).Uri);
                    }
                    break;
                case PowerUpType.BULLS_EYE:
                    {
                        _bitmapImage = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_BULLS_EYE).Uri);
                    }
                    break;
                default:
                    break;
            }

            _content_image = new()
            {
                Source = _bitmapImage,
                Height = this.Height,
                Width = this.Width,
                
            };

            SetChild(_content_image);

            Speed = Constants.DEFAULT_CONSTRUCT_SPEED - 2;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE;
            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;

            _audioStub = new AudioStub((SoundType.POWERUP_PICKUP, 1, false));
        }

        #endregion

        #region Methods

        public void Reset()
        {
            IsPickedUp = false;
            SetScaleTransform(1);

            PowerUpType = (PowerUpType)_random.Next(Enum.GetNames(typeof(PowerUpType)).Length);

            switch (PowerUpType)
            {
                case PowerUpType.SEEKING_SNITCH:
                    {
                        _bitmapImage.UriSource = Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_SEEKING_SNITCH).Uri;
                    }
                    break;
                case PowerUpType.ARMOR:
                    {
                        _bitmapImage.UriSource = Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_ARMOR).Uri;
                    }
                    break;
                case PowerUpType.BULLS_EYE:
                    {
                        _bitmapImage.UriSource = Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_BULLS_EYE).Uri;
                    }
                    break;
                default:
                    break;
            }
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
        SEEKING_SNITCH,
        ARMOR,
        BULLS_EYE,
    }
}
