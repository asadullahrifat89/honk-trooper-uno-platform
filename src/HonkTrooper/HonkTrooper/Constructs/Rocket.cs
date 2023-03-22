namespace HonkTrooper
{
    public partial class Rocket : DirectionalMovingConstruct
    {
        public bool AwaitMoveUpRight { get; set; }

        public bool AwaitMoveDownLeft { get; set; }

        public bool AwaitMoveUpLeft { get; set; }

        public bool AwaitMoveDownRight { get; set; }

        public bool IsBlasting { get; set; }
    }
}
