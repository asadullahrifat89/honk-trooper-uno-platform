using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class Tree : Construct
    {
        #region Fields
        
        private readonly Image _content_image; 

        #endregion

        #region Ctor

        public Tree(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.TREE);

            ConstructType = ConstructType.TREE;

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.TREE).Uri)
            };

            SetChild(_content_image);

            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
            IsometricDisplacement = 0.5;
            DropShadowDistance = -20;
        }

        #endregion
    }
}
