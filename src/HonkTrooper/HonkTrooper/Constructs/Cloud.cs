using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;

namespace HonkTrooper
{
    public partial class Cloud : Construct
    {
        #region Fields

        private readonly Random _random;
        private readonly Uri[] _cloud_uris;

        private readonly Image _content_image;

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

            var uri = ConstructExtensions.GetRandomContentUri(_cloud_uris);

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(_content_image);

            IsometricDisplacement = 0.5;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE + 200;
            Opacity = 0.6;
        }

        #endregion

        #region Methods

        public void Reset()
        {
            SpeedOffset = _random.Next(-2, 3);

            var uri = ConstructExtensions.GetRandomContentUri(_cloud_uris);
            _content_image.Source = new BitmapImage(uri);
        }

        #endregion
    }
}
