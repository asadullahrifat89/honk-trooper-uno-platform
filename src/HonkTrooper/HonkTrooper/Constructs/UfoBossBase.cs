namespace HonkTrooper
{
    public partial class UfoBossBase : AnimableConstruct
    {
        public bool IsAttacking { get; set; }

        public double Health { get; set; }

        public double HitPoint { get; set; } = 5;

        public bool IsDead => Health <= 0;
    }
}
