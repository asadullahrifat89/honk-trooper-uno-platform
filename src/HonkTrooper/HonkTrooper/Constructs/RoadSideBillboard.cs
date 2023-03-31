using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class RoadSideBillboard : MovableConstruct
    {
        #region Fields

        private readonly ImageEx _content_image;
        private readonly BitmapImage _bitmapImage;

        private readonly Uri[] _billboard_uris;

        #endregion

        #region Ctor

        public RoadSideBillboard(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.ROAD_SIDE_BILLBOARD;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            _billboard_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.ROAD_SIDE_BILLBOARD).Select(x => x.Uri).ToArray();

            SetConstructSize();

            var uri = ConstructExtensions.GetRandomContentUri(_billboard_uris);
            _bitmapImage = new BitmapImage(uriSource: uri);

            _content_image = new()
            {
                Source = _bitmapImage,
                Height = this.Height,
                Width = this.Width,
                IsCacheEnabled = true,
            };

            SetChild(_content_image);

            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            DropShadowDistance = -10;
        }

        #endregion
    }
}
