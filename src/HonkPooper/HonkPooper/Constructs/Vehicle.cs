using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;

namespace HonkPooper
{
    public partial class Vehicle : Construct
    {
        #region Fields

        private Random _random;

        private Uri[] _vehicle_small_uris;
        private Uri[] _vehicle_large_uris;

        private int _honkDelay;        

        #endregion

        #region Ctor

        public Vehicle(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            _random = new Random();

            _vehicle_small_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.VEHICLE_SMALL).Select(x => x.Uri).ToArray();
            _vehicle_large_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.VEHICLE_LARGE).Select(x => x.Uri).ToArray();

            WillHonk = Convert.ToBoolean(_random.Next(0, 2));

            var vehicleType = _random.Next(0, 2);

            (ConstructType ConstructType, double Height, double Width) size;
            Uri uri;

            double speedOffset = _random.Next(-4, 2);

            switch (vehicleType)
            {
                case 0:
                    {
                        size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.VEHICLE_SMALL);

                        var vehicles = _vehicle_small_uris;
                        uri = vehicles[_random.Next(0, vehicles.Length)];

                        ConstructType = ConstructType.VEHICLE_SMALL;

                        var width = size.Width * downScaling;
                        var height = size.Height * downScaling;

                        SetSize(width: width, height: height);

                        AnimateAction = animateAction;
                        RecycleAction = recycleAction;

                        var content = new Image()
                        {
                            Source = new BitmapImage(uriSource: uri)
                        };
                        SetChild(content);

                        SpeedOffset = speedOffset;
                    }
                    break;
                case 1:
                    {
                        size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.VEHICLE_LARGE);

                        var vehicles = _vehicle_large_uris;
                        uri = vehicles[_random.Next(0, vehicles.Length)];

                        ConstructType = ConstructType.VEHICLE_LARGE;

                        var width = size.Width * downScaling;
                        var height = size.Height * downScaling;

                        SetSize(width: width, height: height);

                        AnimateAction = animateAction;
                        RecycleAction = recycleAction;

                        var content = new Image()
                        {
                            Source = new BitmapImage(uriSource: uri)
                        };
                        SetChild(content);

                        SpeedOffset = speedOffset;
                    }
                    break;
                default:
                    break;
            }

            IsometricDisplacement = 0.5;

            if (WillHonk)
                SetHonkDelay();
        }

        #endregion

        #region Properties

        public bool WillHonk { get; set; }

        public bool IsMarkedForBombing { get; set; }

        #endregion

        #region Methods

        public void Reset()
        {
            IsMarkedForBombing = false;
            AwaitingPop = false;

            SetScaleTransform(1);

            SpeedOffset = _random.Next(-4, 2);

            WillHonk = Convert.ToBoolean(_random.Next(0, 2));

            if (WillHonk)
            {
                SetHonkDelay();
            }
        }

        public bool Honk()
        {
            if (!IsMarkedForBombing && WillHonk)
            {
                _honkDelay--;

                if (_honkDelay < 0)
                {
                    SetHonkDelay();                    
                    return true;
                }
            }

            return false;

        }

        public void SetHonkDelay()
        {
            _honkDelay = _random.Next(30, 80);
        }

        public void Bomb()
        {
            IsMarkedForBombing = true;
            WillHonk = false;
            AwaitingPop = true;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET + 1;
        }

        #endregion
    }
}
