namespace HonkTrooper
{
    public partial class HoveringTitleScreen : Construct
    {
        private double _hoverDelay;
        private readonly double _hoverDelayDefault = 15;

        private readonly double _hoverSpeed = 0.3;

        public void Hover()
        {
            _hoverDelay--;

            if (_hoverDelay >= 0)
            {
                SetTop(GetTop() + _hoverSpeed);
            }
            else
            {
                SetTop(GetTop() - _hoverSpeed);

                if (_hoverDelay <= _hoverDelayDefault * -1)
                    _hoverDelay = _hoverDelayDefault;
            }
        }

        public void Reposition()
        {
            SetPosition(
                  left: (((Scene.Width / 4) * 2) - Width / 2),
                  top: ((Scene.Height / 2) - Height / 2),
                  z: 10);
        }
    }
}
