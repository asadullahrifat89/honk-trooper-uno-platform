using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class DropShadow : MovableConstruct
    {
        #region Properties

        private Construct ParentConstruct { get; set; }

        private double ParentConstructSpeed { get; set; } = 0;

        #endregion

        #region Ctor

        public DropShadow(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.DROP_SHADOW;

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.DROP_SHADOW);

            var width = size.Width;
            var height = size.Height;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            Height = 25;

            SetSize(width: width, height: height);

            Background = new SolidColorBrush(Colors.Black);
            CornerRadius = new CornerRadius(100);
            Opacity = 0.3;

            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
        }

        #endregion

        #region Methods

        public bool IsParentConstructAnimating()
        {
            return ParentConstruct.IsAnimating;
        }

        public void SetParent(Construct construct)
        {
            Id = construct.Id;
            ParentConstruct = construct;
        }

        public void Reset()
        {
            SetPosition(
                left: (ParentConstruct.GetLeft() + ParentConstruct.Width / 2) - Width / 2,
                top: ParentConstruct.GetBottom() + (ParentConstruct.DropShadowDistance));

            ParentConstructSpeed = ParentConstruct.GetMovementSpeed();

            Height = 25;
            Width = ParentConstruct.Width * 0.5;
        }

        public void Move()
        {
            SetLeft((ParentConstruct.GetLeft() + ParentConstruct.Width / 2) - Width / 2);

            if (ParentConstruct.IsGravitatingDownwards)
            {
                MoveDownRight(ParentConstructSpeed * IsometricDisplacement);

                if (Width < ParentConstruct.Width)
                    Width += 1;
            }
            else if (ParentConstruct.IsGravitatingUpwards)
            {
                MoveDownRight(ParentConstructSpeed);

                if (Width > 0)
                {
                    Width -= 0.5;
                    Height -= 0.3;
                }
            }
            else
            {
                SetTop(ParentConstruct.GetBottom() + ParentConstruct.DropShadowDistance); // in normal circumstances, follow the parent
            }
        }

        #endregion
    }
}
