using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;

namespace HonkTrooper
{
    public partial class Cloud : AnimableConstruct
    {
        #region Fields

        private readonly Random _random;
        private readonly Uri[] _cloud_uris;

        private readonly Image _content_image;
        private readonly BitmapImage _bitmapImage;

        #endregion

        #region Ctor

        public Cloud(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.CLOUD;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            _random = new Random();

            _cloud_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.CLOUD).Select(x => x.Uri).ToArray();

            SetConstructSize();

            var uri = ConstructExtensions.GetRandomContentUri(_cloud_uris);
            _bitmapImage = new BitmapImage(uriSource: uri);
            _content_image = new Image()
            {
                Source = _bitmapImage,
                Height = this.Height,
                Width = this.Width,
            };

            SetChild(_content_image);

            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE + 200;
            Opacity = 0.6;
        }

        #endregion

        #region Methods

        public void Reset()
        {
            SpeedOffset = _random.Next(-3, 3);

            var uri = ConstructExtensions.GetRandomContentUri(_cloud_uris);
            _bitmapImage.UriSource = uri;
        }

        #endregion
    }
}
