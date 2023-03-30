using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class RoadSideWalk : MovableConstruct
    {
        #region Fields

        private readonly Image _content_image;
        private readonly Uri[] _side_walk_uris;

        #endregion

        #region Ctor

        public RoadSideWalk(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.ROAD_SIDE_WALK;

            _side_walk_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.ROAD_SIDE_WALK).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.ROAD_SIDE_WALK);

            var width = size.Width;
            var height = size.Height;

            SetSize(width: width, height: height);

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            var uri = ConstructExtensions.GetRandomContentUri(_side_walk_uris);

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: uri),
                Stretch = Stretch.Fill
            };

            SetChild(_content_image);

            BorderBrush = App.Current.Resources["BorderColor"] as SolidColorBrush;
            BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS, 0);

            SetSkewY(36);
            SetRotation(-63.5);

            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
        }

        #endregion

        #region Methods

        public void Reset()
        {
            if (Scene.IsInNightMode)
            {
                _content_image.Opacity = 0.2;
            }
            else
            {
                _content_image.Opacity = 1;
            }
        }

        #endregion
    }
}
