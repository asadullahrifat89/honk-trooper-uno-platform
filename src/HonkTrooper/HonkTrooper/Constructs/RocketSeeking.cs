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

        public void Seek(Rect target, bool doubleSpeed = false)
        {
            double left = GetLeft();
            double top = GetTop();

            double playerMiddleX = left + Width / 2;
            double playerMiddleY = top + Height / 2;

            // move up
            if (target.Y < playerMiddleY - _grace)
            {
                var distance = Math.Abs(target.Y - playerMiddleY);
                double speed = CalculateSpeed(distance, doubleSpeed);

                SetTop(top - speed);
            }

            // move left
            if (target.X < playerMiddleX - _grace)
            {
                var distance = Math.Abs(target.X - playerMiddleX);
                double speed = CalculateSpeed(distance, doubleSpeed);

                SetLeft(left - speed);
            }

            // move down
            if (target.Y > playerMiddleY + _grace)
            {
                var distance = Math.Abs(target.Y - playerMiddleY);
                double speed = CalculateSpeed(distance, doubleSpeed);

                SetTop(top + speed);
            }

            // move right
            if (target.X > playerMiddleX + _grace)
            {
                var distance = Math.Abs(target.X - playerMiddleX);
                double speed = CalculateSpeed(distance, doubleSpeed);

                SetLeft(left + speed);
            }
        }

        private double CalculateSpeed(double distance, bool doubleSpeed = false)
        {
            var speed = distance / _lag;

            speed = speed < 4 ? 4 : speed;

            return doubleSpeed ? speed * 1.3 : speed;
        }
    }
}

