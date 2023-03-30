namespace HonkTrooper
{
    public partial class VehicleBossBase : VehicleBase
    {
        #region Fields

        #endregion

        #region Properties

        public bool IsAttacking { get; set; }

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
