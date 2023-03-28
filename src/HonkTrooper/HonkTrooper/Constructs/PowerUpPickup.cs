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

        private readonly AudioStub _audioStub;
        #endregion

        #region Ctor

        public PowerUpPickup(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP);

            ConstructType = ConstructType.POWERUP_PICKUP;

            _random = new Random();

            var width = size.Width;
            var height = size.Height;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            PowerUpType = (PowerUpType)_random.Next(Enum.GetNames(typeof(PowerUpType)).Length);

            switch (PowerUpType)
            {
                case PowerUpType.SEEKING_SNITCH:
                    {
                        _content_image = new Image()
                        {
                            Source = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_SEEKING_SNITCH).Uri)
                        };
                    }
                    break;
                case PowerUpType.ARMOR:
                    {
                        _content_image = new Image()
                        {
                            Source = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_ARMOR).Uri)
                        };
                    }
                    break;
                case PowerUpType.BULLS_EYE:
                    {
                        _content_image = new Image()
                        {
                            Source = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_BULLS_EYE).Uri)
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

            PowerUpType = (PowerUpType)_random.Next(Enum.GetNames(typeof(PowerUpType)).Length);

            //TODO: Comment this line upon testing completion : PowerUpType = PowerUpType.BULLS_EYE
            PowerUpType = PowerUpType.BULLS_EYE;

            switch (PowerUpType)
            {
                case PowerUpType.SEEKING_SNITCH:
                    {
                        _content_image.Source = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_SEEKING_SNITCH).Uri);
                    }
                    break;
                case PowerUpType.ARMOR:
                    {
                        _content_image.Source = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_ARMOR).Uri);
                    }
                    break;
                case PowerUpType.BULLS_EYE:
                    {
                        _content_image.Source = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_BULLS_EYE).Uri);
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
        SEEKING_SNITCH,
        ARMOR,
        BULLS_EYE,
    }
}
