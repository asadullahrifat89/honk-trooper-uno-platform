using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class ManholeCover : MovableConstruct
    {
        #region Fields

        private readonly Image _content_image;
        private readonly Uri[] _manhole_uris;

        #endregion

        #region Ctor

        public ManholeCover(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.MANHOLE_COVER;

            _manhole_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.MANHOLE_COVER).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.MANHOLE_COVER);

            var width = size.Width;
            var height = size.Height;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var uri = ConstructExtensions.GetRandomContentUri(_manhole_uris);

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(_content_image);

            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            DropShadowDistance = -22;
        }

        #endregion
    }
}
