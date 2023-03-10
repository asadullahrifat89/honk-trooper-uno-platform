using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace HonkTrooper
{
    public partial class Construct : ConstructBase
    {
        #region Properties

        /// <summary>
        /// Type of the construct.
        /// </summary>
        public ConstructType ConstructType { get; set; }

        /// <summary>
        /// Animation function.
        /// </summary>
        public Func<Construct, bool> AnimateAction { get; set; }

        /// <summary>
        /// Recycling function.
        /// </summary>
        public Func<Construct, bool> RecycleAction { get; set; }

        /// <summary>
        /// Adds an offset while animating this construct with the scene speed.
        /// </summary>
        public double SpeedOffset { get; set; } = 0;

        /// <summary>
        /// Displacement value that determines isometric movement.
        /// </summary>
        public double IsometricDisplacement { get; set; }

        /// <summary>
        /// The scene to which this construct is visible in.
        /// </summary>
        public Scene Scene { get; set; }

        #endregion

        #region Methods     

        //public void SetAction(
        //    Func<Construct, bool> movementAction,
        //    Func<Construct, bool> recycleAction)
        //{
        //    AnimateAction = movementAction;
        //    RecycleAction = recycleAction;
        //}

        public void Animate()
        {
            AnimateAction(this);
        }

        public void Recycle()
        {
            RecycleAction(this);
        }

        public void SetChild(UIElement uIElement)
        {
            Child = uIElement;
        }

        public Uri GetContentUri()
        {
            if (Child is not null && Child is Image image)
            {
                var bitmapImage = image.Source as BitmapImage;
                return bitmapImage.UriSource;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
