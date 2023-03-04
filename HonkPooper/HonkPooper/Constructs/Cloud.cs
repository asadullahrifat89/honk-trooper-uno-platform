using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;

namespace HonkPooper
{
    public partial class Cloud : Construct
    {
        #region Fields

        private Random _random;
        private Uri[] _cloud_uris;

        #endregion

        #region Ctor

        public Cloud(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            _random = new Random();

            _cloud_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.CLOUD).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.CLOUD);

            ConstructType = ConstructType.CLOUD;

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var uri = _cloud_uris[_random.Next(0, _cloud_uris.Length)];

            var content = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(content);

            IsometricDisplacement = 0.5;
            SpeedOffset = 3;

            DropShadowDistance = 100;
        }

        #endregion

        #region Methods

        public void Reset()
        {
            SpeedOffset = _random.Next(3, 7);
        }

        #endregion
    }
}
