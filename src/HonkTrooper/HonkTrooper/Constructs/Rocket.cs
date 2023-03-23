namespace HonkTrooper
{
    public partial class Rocket : DirectionalMovingConstruct
    {
        public bool AwaitMoveUp { get; set; }

        public bool AwaitMoveDown { get; set; }

        public bool AwaitMoveLeft { get; set; }

        public bool AwaitMoveRight { get; set; }

        public bool AwaitMoveUpRight { get; set; }

        public bool AwaitMoveDownLeft { get; set; }

        public bool AwaitMoveUpLeft { get; set; }

        public bool AwaitMoveDownRight { get; set; }

        public bool IsBlasting { get; set; }
    }
}
