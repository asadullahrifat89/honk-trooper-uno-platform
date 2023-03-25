namespace HonkTrooper
{
    public partial class HoveringTitleScreen : AnimableConstruct
    {
        public void Reposition()
        {
            SetPosition(
                  left: (((Scene.Width / 4) * 2) - Width / 2),
                  top: ((Scene.Height / 2) - Height / 2),
                  z: 10);
        }
    }
}
