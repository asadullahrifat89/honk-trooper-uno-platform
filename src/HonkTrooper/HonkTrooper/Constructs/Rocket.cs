namespace HonkTrooper
{
    public partial class Rocket : Construct
    {
        public void MoveDownLeft(double speed)
        {
            SetLeft(GetLeft() - speed * 2);
            SetTop(GetTop() + speed);
        }

        public void MoveUpRight(double speed)
        {
            SetLeft(GetLeft() + speed * 2);
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
