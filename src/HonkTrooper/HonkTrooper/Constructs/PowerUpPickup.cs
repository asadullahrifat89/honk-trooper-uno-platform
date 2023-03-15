﻿using Microsoft.UI.Xaml.Controls;
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

        private readonly Sound[] _power_up_pickup_sounds;

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

            PowerUpType = (PowerUpType)_random.Next(0, Enum.GetNames(typeof(PowerUpType)).Length);

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
            IsometricDisplacement = 0.5;

            _power_up_pickup_sounds = Constants.SOUND_TEMPLATES.Where(x => x.SoundType == SoundType.POWERUP_PICKUP).Select(x => x.Uri).Select(uri => new Sound(uri: uri, volume: 0.8)).ToArray();
        }

        #endregion

        #region Methods

        public void Reset()
        {
            IsPickedUp = false;

            PowerUpType = (PowerUpType)_random.Next(0, Enum.GetNames(typeof(PowerUpType)).Length);

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
            var sound = _power_up_pickup_sounds[_random.Next(0, _power_up_pickup_sounds.Length)];
            sound.Play();

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
