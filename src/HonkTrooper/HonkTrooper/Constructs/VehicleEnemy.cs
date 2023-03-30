using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;

namespace HonkTrooper
{
    public partial class VehicleEnemy : VehicleBase
    {
        #region Fields

        private readonly Random _random;

        private readonly Uri[] _vehicle_small_uris;
        private readonly Uri[] _vehicle_large_uris;

        private readonly AudioStub _audioStub;

        #endregion

        #region Ctor

        public VehicleEnemy(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            _random = new Random();

            _vehicle_small_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.VEHICLE_ENEMY_SMALL).Select(x => x.Uri).ToArray();
            _vehicle_large_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.VEHICLE_ENEMY_LARGE).Select(x => x.Uri).ToArray();

            WillHonk = Convert.ToBoolean(_random.Next(2));

            var vehicleType = _random.Next(2);

            Uri uri;

            switch (vehicleType)
            {
                case 0:
                    {
                        ConstructType = ConstructType.VEHICLE_ENEMY_SMALL;

                        SetConstructSize();

                        uri = ConstructExtensions.GetRandomContentUri(_vehicle_small_uris);

                        var content = new Image()
                        {
                            Source = new BitmapImage(uriSource: uri)
                        };
                        SetChild(content);
                    }
                    break;
                case 1:
                    {
                        ConstructType = ConstructType.VEHICLE_ENEMY_LARGE;

                        SetConstructSize();

                        uri = ConstructExtensions.GetRandomContentUri(_vehicle_large_uris);

                        var content = new Image()
                        {
                            Source = new BitmapImage(uriSource: uri)
                        };
                        SetChild(content);
                    }
                    break;
                default:
                    break;
            }

            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;

            if (WillHonk)
                SetHonkDelay();

            _audioStub = new AudioStub((SoundType.HONK_BUST_REACTION, 1, false));
        }

        #endregion     

        #region Methods

        public void Reset()
        {
            SetScaleTransform(1);

            SpeedOffset = _random.Next((int)Constants.DEFAULT_SPEED_OFFSET * -1, 0);

            WillHonk = Convert.ToBoolean(_random.Next(2));

            if (WillHonk)
                SetHonkDelay();
        }

        public void SetBlast()
        {
            WillHonk = false;
            SetPopping();
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET - 1;

            var willReact = _random.Next(2);

            if (willReact > 0)
                _audioStub.Play(SoundType.HONK_BUST_REACTION);
        }

        #endregion
    }
}
