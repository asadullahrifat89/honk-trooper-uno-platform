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

    public partial class DirectionalMovingConstruct : Construct
    {
        public void MoveUp(double speed)
        {
            SetTop(GetTop() - speed * 2);
        }

        public void MoveDown(double speed)
        {
            SetTop(GetTop() + speed * 2);
        }

        public void MoveLeft(double speed)
        {
            SetLeft(GetLeft() - speed * 2);
        }

        public void MoveRight(double speed)
        {
            SetLeft(GetLeft() + speed * 2);
        }


        public void MoveUpRight(double speed)
        {
            SetLeft(GetLeft() + speed * 2);
            SetTop(GetTop() - speed);
        }

        public void MoveUpLeft(double speed)
        {
            SetLeft(GetLeft() - speed * 2);
            SetTop(GetTop() - speed);
        }

        public void MoveDownRight(double speed)
        {
            SetLeft(GetLeft() + speed * 2);
            SetTop(GetTop() + speed);
        }

        public void MoveDownLeft(double speed)
        {
            SetLeft(GetLeft() - speed * 2);
            SetTop(GetTop() + speed);
        }
    }
}
