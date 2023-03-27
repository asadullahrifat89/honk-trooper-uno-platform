using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class RoadSideWalk : MovableConstruct
    {
        #region Ctor

        public RoadSideWalk(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.ROAD_SIDE_WALK;

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.ROAD_SIDE_WALK);

            var width = size.Width;
            var height = size.Height;

            SetSize(width: width, height: height);

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            BorderThickness = new Thickness(leftRight: 30, topBottom: 5);

            SetSkewY(36);
            SetRotation(-63.5);

            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
        }

        #endregion

        #region Methods

        public void Reset()
        {
            if (Scene.Children.OfType<Construct>().Any(x => (x.ConstructType is ConstructType.ZOMBIE_BOSS or ConstructType.UFO_BOSS) && x.IsAnimating))
            {
                Background = App.Current.Resources["RoadSideWalkColor2"] as SolidColorBrush;
                BorderBrush = App.Current.Resources["RoadSideWalkBorderColor2"] as SolidColorBrush;
            }
            else
            {
                Background = App.Current.Resources["RoadSideWalkColor"] as SolidColorBrush;
                BorderBrush = App.Current.Resources["RoadSideWalkBorderColor"] as SolidColorBrush;
            }
        }

        #endregion
    }
}
