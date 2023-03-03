using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HonkPooper
{
    public partial class Tree : Construct
    {
        #region Ctor

        public Tree(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double scaling)
        {
            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.TREE);

            ConstructType = ConstructType.TREE;

            var width = size.Width * scaling;
            var height = size.Height * scaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var content = new Image()
            {
                Source = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.TREE).Uri)
            };

            SetChild(content);
            SpeedOffset = 3 * scaling;
            IsometricDisplacement = 0.5;
        }

        #endregion
    }
}
