using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;

namespace HonkTrooper
{
    public partial class Vehicle : Construct
    {
        #region Fields

        private readonly Random _random;

        private readonly Uri[] _vehicle_small_uris;
        private readonly Uri[] _vehicle_large_uris;

        private double _honkDelay;

        #endregion

        #region Ctor

        public Vehicle(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            _random = new Random();

            _vehicle_small_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.VEHICLE_SMALL).Select(x => x.Uri).ToArray();
            _vehicle_large_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.VEHICLE_LARGE).Select(x => x.Uri).ToArray();

            WillHonk = Convert.ToBoolean(_random.Next(2));

            var vehicleType = _random.Next(2);

            (ConstructType ConstructType, double Height, double Width) size;
            Uri uri;

            switch (vehicleType)
            {
                case 0:
                    {
                        size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.VEHICLE_SMALL);

                        uri = ConstructExtensions.GetRandomContentUri(_vehicle_small_uris);

                        ConstructType = ConstructType.VEHICLE_SMALL;

                        var width = size.Width;
                        var height = size.Height;

                        SetSize(width: width, height: height);

                        AnimateAction = animateAction;
                        RecycleAction = recycleAction;

                        var content = new Image()
                        {
                            Source = new BitmapImage(uriSource: uri)
                        };
                        SetChild(content);
                    }
                    break;
                case 1:
                    {
                        size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.VEHICLE_LARGE);

                        uri = ConstructExtensions.GetRandomContentUri(_vehicle_large_uris);

                        ConstructType = ConstructType.VEHICLE_LARGE;

                        var width = size.Width;
                        var height = size.Height;

                        SetSize(width: width, height: height);

                        AnimateAction = animateAction;
                        RecycleAction = recycleAction;

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

            SetScaleTransform(1);

            SpeedOffset = _random.Next(-2, 4);

            WillHonk = Convert.ToBoolean(_random.Next(2));

            if (WillHonk)
                SetHonkDelay();
        }

        public void Reposition()
        {
            var topOrLeft = _random.Next(2); // generate top and left corner lane wise vehicles
            var lane = _random.Next(2); // generate number of lanes based of screen height

            switch (topOrLeft)
            {
                case 0:
                    {
                        var xLaneWidth = Constants.DEFAULT_SCENE_WIDTH / 4;

                        SetPosition(
                            left: lane == 0 ? 0 : (xLaneWidth - Width / 2),
                            top: Height * -1);
                    }
                    break;
                case 1:
                    {
                        var yLaneHeight = Constants.DEFAULT_SCENE_HEIGHT / 6;

                        SetPosition(
                            left: Width * -1,
                            top: lane == 0 ? 0 : (yLaneHeight));
                    }
                    break;
                default:
                    break;
            }
        }

        public bool Honk()
        {
            if (!IsMarkedForBombing && WillHonk)
            {
                if (Scene.IsSlowMotionActivated)
                    _honkDelay -= 0.5;
                else
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

        public void SetBlast()
        {
            IsMarkedForBombing = true;
            WillHonk = false;
            SetPopping();
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET + 1;
        }

        #endregion
    }
}
