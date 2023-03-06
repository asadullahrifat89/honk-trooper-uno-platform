using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class Tree : Construct
    {
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

            var content = new Image()
            {
                Source = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.TREE).Uri)
            };

            SetChild(content);
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
            IsometricDisplacement = 0.5;
        }

        #endregion
    }
}
