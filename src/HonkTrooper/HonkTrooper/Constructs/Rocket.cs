namespace HonkTrooper
{
    public partial class Rocket : Construct
    {
        public void MoveUp(double speed)
        {
            SetTop(GetTop() - speed);
        }

        public void MoveDown(double speed)
        {
            SetTop(GetTop() + speed);
        }

        public void MoveLeft(double speed)
        {
            SetLeft(GetLeft() - speed);
        }

        public void MoveRight(double speed)
        {
            SetLeft(GetLeft() + speed);
        }

        public void MoveDownLeft(double speed)
        {
            SetLeft(GetLeft() - speed);
            SetTop(GetTop() + speed);
        }

        public void MoveUpRight(double speed)
        {
            SetLeft(GetLeft() + speed);
            SetTop(GetTop() - speed);
        }

        public void MoveUpLeft(double speed)
        {
            SetLeft(GetLeft() - speed);
            SetTop(GetTop() - speed * IsometricDisplacement);
        }

        public void MoveDownRight(double speed)
        {
            SetLeft(GetLeft() + speed);
            SetTop(GetTop() + speed * IsometricDisplacement);
        }
    }
}
