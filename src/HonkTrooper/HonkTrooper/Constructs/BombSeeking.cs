using System;
using Windows.Foundation;

namespace HonkTrooper
{
    public partial class BombSeeking : Construct
    {
        #region Fields

        private readonly double _grace = 7;
        private readonly double _lag = 60;

        #endregion

        public bool Seek(Rect bossHitbox)
        {
            bool hasMoved = false;

            double left = GetLeft();
            double top = GetTop();

            double playerMiddleX = left + Width / 2;
            double playerMiddleY = top + Height / 2;

            // move up
            if (bossHitbox.Y < playerMiddleY - _grace)
            {
                var distance = Math.Abs(bossHitbox.Y - playerMiddleY);
                double speed = GetFlightSpeed(distance);

                SetTop(top - speed);

                hasMoved = true;
            }

            // move left
            if (bossHitbox.X < playerMiddleX - _grace)
            {
                var distance = Math.Abs(bossHitbox.X - playerMiddleX);
                double speed = GetFlightSpeed(distance);

                SetLeft(left - speed);

                hasMoved = true;
            }

            // move down
            if (bossHitbox.Y > playerMiddleY + _grace)
            {
                var distance = Math.Abs(bossHitbox.Y - playerMiddleY);
                double speed = GetFlightSpeed(distance);

                SetTop(top + speed);

                hasMoved = true;
            }

            // move right
            if (bossHitbox.X > playerMiddleX + _grace)
            {
                var distance = Math.Abs(bossHitbox.X - playerMiddleX);
                double speed = GetFlightSpeed(distance);

                SetLeft(left + speed);

                hasMoved = true;
            }

            return hasMoved;
        }

        private double GetFlightSpeed(double distance)
        {
            var flightSpeed = distance / _lag;

            return flightSpeed;
        }
    }
}
