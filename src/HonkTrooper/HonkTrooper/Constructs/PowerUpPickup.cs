﻿using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class PowerUpPickup : MovableConstruct
    {
        #region Fields

        private readonly Random _random;
        private readonly ImageElement _content_image;
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

            SetConstructSize(ConstructType);

            PowerUpType = (PowerUpType)_random.Next(Enum.GetNames(typeof(PowerUpType)).Length);

            Uri uri = null;

            switch (PowerUpType)
            {
                case PowerUpType.SEEKING_SNITCH:
                    {
                        uri = Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_SEEKING_SNITCH).Uri;
                    }
                    break;
                case PowerUpType.ARMOR:
                    {
                        uri = Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_ARMOR).Uri;
                    }
                    break;
                case PowerUpType.BULLS_EYE:
                    {
                        uri = Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_BULLS_EYE).Uri;
                    }
                    break;
                default:
                    break;
            }

            _content_image = new(uri: uri, width: this.Width, height: this.Height);

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

            Uri uri = null;

            switch (PowerUpType)
            {
                case PowerUpType.SEEKING_SNITCH:
                    {
                        uri = Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_SEEKING_SNITCH).Uri;
                    }
                    break;
                case PowerUpType.ARMOR:
                    {
                        uri = Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_ARMOR).Uri;
                    }
                    break;
                case PowerUpType.BULLS_EYE:
                    {
                        uri = Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.POWERUP_PICKUP_BULLS_EYE).Uri;
                    }
                    break;
                default:
                    break;
            }

            _content_image.SetSource(uri);
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
