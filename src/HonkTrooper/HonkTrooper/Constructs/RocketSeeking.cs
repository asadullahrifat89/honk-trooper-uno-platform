using System;
using Windows.Foundation;

namespace HonkTrooper
{
    public partial class RocketSeeking : MovableConstruct
    {
        #region Fields

        private readonly double _grace = 7;
        private readonly double _lag = 60;

        #endregion

        public void Seek(Rect target)
        {
            double left = GetLeft();
            double top = GetTop();

            double playerMiddleX = left + Width / 2;
            double playerMiddleY = top + Height / 2;

            // move up
            if (target.Y < playerMiddleY - _grace)
            {
                var distance = Math.Abs(target.Y - playerMiddleY);
                double speed = CalculateSpeed(distance);

                SetTop(top - speed);
            }

            // move left
            if (target.X < playerMiddleX - _grace)
            {
                var distance = Math.Abs(target.X - playerMiddleX);
                double speed = CalculateSpeed(distance);

                SetLeft(left - speed);
            }

            // move down
            if (target.Y > playerMiddleY + _grace)
            {
                var distance = Math.Abs(target.Y - playerMiddleY);
                double speed = CalculateSpeed(distance);

                SetTop(top + speed);
            }

            // move right
            if (target.X > playerMiddleX + _grace)
            {
                var distance = Math.Abs(target.X - playerMiddleX);
                double speed = CalculateSpeed(distance);

                SetLeft(left + speed);
            }
        }

        private double CalculateSpeed(double distance)
        {
            var speed = distance / _lag;

            return speed < 4 ? 4 : speed;
        }
    }
}

