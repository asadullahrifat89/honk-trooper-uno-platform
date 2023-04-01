using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

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
            Action<Construct> animateAction,
            Action<Construct> recycleAction)
        {
            ConstructType = ConstructType.CLOUD;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            _random = new Random();

            _cloud_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.CLOUD).Select(x => x.Uri).ToArray();

            SetConstructSize(ConstructType);

            var uri = ConstructExtensions.GetRandomContentUri(_cloud_uris);
            _bitmapImage = new BitmapImage(uriSource: uri);
            _content_image = new()
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
            Speed = _random.Next(Constants.DEFAULT_CONSTRUCT_SPEED - 2, Constants.DEFAULT_CONSTRUCT_SPEED);

            var uri = ConstructExtensions.GetRandomContentUri(_cloud_uris);
            _bitmapImage.UriSource = uri;
        }

        #endregion
    }
}
