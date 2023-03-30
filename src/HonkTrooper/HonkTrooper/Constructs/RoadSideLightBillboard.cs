using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class RoadSideLightBillboard : MovableConstruct
    {
        #region Fields

        private readonly Image _content_image;
        private readonly Uri[] _tree_uris;

        #endregion

        #region Ctor

        public RoadSideLightBillboard(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.ROAD_SIDE_LIGHT_BILLBOARD;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            _tree_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.ROAD_SIDE_LIGHT_BILLBOARD).Select(x => x.Uri).ToArray();

            SetConstructSize();

            var uri = ConstructExtensions.GetRandomContentUri(_tree_uris);

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(_content_image);

            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            DropShadowDistance = -5;
        }

        #endregion
    }
}
