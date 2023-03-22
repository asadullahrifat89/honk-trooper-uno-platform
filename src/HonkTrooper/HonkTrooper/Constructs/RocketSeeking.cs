using System;
using Windows.Foundation;

namespace HonkTrooper
{
    public partial class RocketSeeking : Construct
    {
        #region Fields

        private readonly double _grace = 7;
        private readonly double _lag = 60;

        #endregion

        public bool Seek(Rect hitbox)
        {
            bool hasMoved = false;

            double left = GetLeft();
            double top = GetTop();

            double playerMiddleX = left + Width / 2;
            double playerMiddleY = top + Height / 2;

            // move up
            if (hitbox.Y < playerMiddleY - _grace)
            {
                var distance = Math.Abs(hitbox.Y - playerMiddleY);
                double speed = CalculateSpeed(distance);

                SetTop(top - speed);

                hasMoved = true;
            }

            // move left
            if (hitbox.X < playerMiddleX - _grace)
            {
                var distance = Math.Abs(hitbox.X - playerMiddleX);
                double speed = CalculateSpeed(distance);

                SetLeft(left - speed);

                hasMoved = true;
            }

            // move down
            if (hitbox.Y > playerMiddleY + _grace)
            {
                var distance = Math.Abs(hitbox.Y - playerMiddleY);
                double speed = CalculateSpeed(distance);

                SetTop(top + speed);

                hasMoved = true;
            }

            // move right
            if (hitbox.X > playerMiddleX + _grace)
            {
                var distance = Math.Abs(hitbox.X - playerMiddleX);
                double speed = CalculateSpeed(distance);

                SetLeft(left + speed);

                hasMoved = true;
            }

            return hasMoved;
        }

        private double CalculateSpeed(double distance)
        {
            var speed = distance / _lag;

            return speed < 4 ? 4 : speed;
        }
    }
}

