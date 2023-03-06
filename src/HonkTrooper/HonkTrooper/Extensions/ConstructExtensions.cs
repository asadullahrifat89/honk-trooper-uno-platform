using Windows.Foundation;

namespace HonkTrooper
{
    public static class ConstructExtensions
    {
        #region Methods      

        /// <summary>
        /// Checks if a two rects intersect.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IntersectsWith(this Rect source, Rect target)
        {
            var targetX = target.X;
            var targetY = target.Y;
            var sourceX = source.X;
            var sourceY = source.Y;

            var sourceWidth = source.Width;
            var sourceHeight = source.Height;

            var targetWidth = target.Width;
            var targetHeight = target.Height;

            if (source.Width >= 0.0 && target.Width >= 0.0
                && targetX <= sourceX + sourceWidth && targetX + targetWidth >= sourceX
                && targetY <= sourceY + sourceHeight)
            {
                return targetY + targetHeight >= sourceY;
            }

            return false;
        }

        public static Rect GetHitBox(this Construct Construct)
        {
            var rect = new Rect(
              x: Construct.GetLeft(),
              y: Construct.GetTop(),
              width: Construct.Width,
              height: Construct.Height);

            //Construct.SetHitBoxBorder(rect);

            return rect;
        }

        public static Rect GetCloseHitBox(this Construct Construct)
        {
            var fourthWidht = Construct.Width / 5;
            var fourthHeight = Construct.Height / 5;

            var rect = new Rect(
                x: Construct.GetLeft() + fourthWidht,
                y: Construct.GetTop() + fourthHeight,
                width: Construct.Width - fourthWidht,
                height: Construct.Height - fourthHeight);

            //Construct.SetHitBoxBorder(rect);

            return rect;
        }

        public static Rect GetHorizontalHitBox(this Construct Construct)
        {
            var fourthHeight = Construct.Height / 4;

            var rect = new Rect(
                x: Construct.GetLeft(),
                y: Construct.GetTop() + fourthHeight,
                width: Construct.Width,
                height: Construct.Height - fourthHeight);

            //Construct.SetHitBoxBorder(rect);

            return rect;
        }

        public static Rect GetOverlappingHitBox(this Construct Construct)
        {
            var fourthWidht = Construct.Width / 3;
            var fourthHeight = Construct.Height / 3;

            var rect = new Rect(
                x: Construct.GetLeft() + fourthWidht,
                y: Construct.GetTop() + fourthHeight,
                width: Construct.Width - fourthWidht,
                height: Construct.Height - fourthHeight);

            //Construct.SetHitBoxBorder(rect);

            return rect;
        }

        public static Rect GetDistantHitBox(this Construct Construct)
        {
            var maxWidth = (Construct.Width * 4);
            var maxHeight = (Construct.Height * 4);

            return new Rect(
                x: Construct.GetLeft() - maxWidth,
                y: Construct.GetTop() - maxHeight,
                width: Construct.Width + maxWidth,
                height: Construct.Height + maxHeight);
        }

        #endregion
    }
}
