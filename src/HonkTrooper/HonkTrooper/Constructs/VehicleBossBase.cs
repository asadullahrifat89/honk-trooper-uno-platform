namespace HonkTrooper
{
    public partial class VehicleBossBase : VehicleBase
    {
        #region Fields

        #endregion

        #region Properties

        public bool IsAttacking { get; set; }

        public double Health { get; set; }

        public double HitPoint { get; set; } = 5;

        public bool IsDead => Health <= 0;

        #endregion

        #region Methods

        public void Reset()
        {
            Opacity = 1;
            Health = 100;

            IsAttacking = false;
            WillHonk = true;
        }

        public void LooseHealth()
        {
            Health -= HitPoint;

            if (IsDead)
            {
                SpeedOffset = Constants.DEFAULT_SPEED_OFFSET - 1;
                IsAttacking = false;
            }
        }

        #endregion
    }
}
