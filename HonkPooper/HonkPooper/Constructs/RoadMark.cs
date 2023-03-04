using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HonkPooper
{
    public partial class RoadMark : Construct
    {
        #region Ctor

        public RoadMark(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double scaling)
        {
            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.ROAD_MARK);

            ConstructType = ConstructType.ROAD_MARK;

            var width = size.Width * scaling;
            var height = size.Height * scaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);
            SpeedOffset = 3;

            Background = new SolidColorBrush(Colors.White);
            CornerRadius = new CornerRadius(5);

            SetSkewY(42);
            SetRotation(-63.5);

            IsometricDisplacement = 0.5;
        } 

        #endregion
    }
}
