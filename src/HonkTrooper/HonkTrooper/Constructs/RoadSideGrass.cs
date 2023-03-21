using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;


namespace HonkTrooper
{
    public partial class RoadSideGrass : Construct
    {
        #region Fields

        private readonly Image _content_image;
        private readonly Uri[] _grass_uris;

        #endregion

        #region Ctor

        public RoadSideGrass(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            ConstructType = ConstructType.ROAD_SIDE_GRASS;

            _grass_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.ROAD_SIDE_GRASS).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.ROAD_SIDE_GRASS);

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            SetSize(width: width, height: height);

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            //Background = App.Current.Resources["RoadSideGrassColor"] as SolidColorBrush;
            //CornerRadius = new CornerRadius(5);

            //SetSkewY(42);
            //SetRotation(-63.5);

            var uri = ConstructExtensions.GetRandomContentUri(_grass_uris);

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(_content_image);

            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
        }

        #endregion
    }
}
