using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class RoadSideHedge : MovableConstruct
    {
        #region Fields

        private readonly Image _content_image;
        private readonly BitmapImage _bitmapImage;

        private readonly Uri[] _tree_uris;

        #endregion

        #region Ctor

        public RoadSideHedge(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.ROAD_SIDE_HEDGE;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            _tree_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.ROAD_SIDE_HEDGE).Select(x => x.Uri).ToArray();

            SetConstructSize();

            var uri = ConstructExtensions.GetRandomContentUri(_tree_uris);
            _bitmapImage = new BitmapImage(uriSource: uri);

            _content_image = new Image()
            {
                Source = _bitmapImage,
                Height = this.Height,
                Width = this.Width,
            };

            SetChild(_content_image);

            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            DropShadowDistance = 0;
        }

        #endregion
    }
}
