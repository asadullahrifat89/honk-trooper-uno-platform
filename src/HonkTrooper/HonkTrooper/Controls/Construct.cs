using Microsoft.UI.Xaml;
using System;
using System.Linq;

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
        public Action<Construct> AnimateAction { get; set; }

        /// <summary>
        /// Recycling function.
        /// </summary>
        public Action<Construct> RecycleAction { get; set; }

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

        public double GetMovementSpeed()
        {
            return Scene.Speed + SpeedOffset;
        }

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

        public void SetConstructSize()
        {
            if (ConstructType != ConstructType.NONE)
            {
                var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType);

                var width = size.Width;
                var height = size.Height;

                SetSize(width: width, height: height);
            }
        }

        #endregion
    }
}
