using System;

namespace HonkTrooper
{
    public partial class VehicleBase : AnimableConstruct
    {
        #region Fields

        private readonly Random _random;
        private double _honkDelay;

        #endregion

        #region Ctor

        public VehicleBase()
        {
            _random = new Random();
        }

        #endregion

        #region Properties

        public bool WillHonk { get; set; }

        #endregion

        #region Methods

        public void Reposition()
        {
            var topOrLeft = _random.Next(2); // generate top and left corner lane wise vehicles
            var lane = _random.Next(2); // generate number of lanes based of screen height
            var randomY = _random.Next(-10, 10);

            switch (topOrLeft)
            {
                case 0:
                    {
                        var xLaneWidth = Constants.DEFAULT_SCENE_WIDTH / 4;

                        SetPosition(
                            left: lane == 0 ? 0 - Width / 2 : (xLaneWidth - Width / 1.5),
                            top: (Height * -1) + randomY,
                            z: 3);
                    }
                    break;
                case 1:
                    {
                        var yLaneHeight = Constants.DEFAULT_SCENE_HEIGHT / 6;

                        SetPosition(
                            left: Width * -1,
                            top: (lane == 0 ? 0 - Height / 2 : yLaneHeight - Height / 3) + randomY,
                            z: 3);
                    }
                    break;
                default:
                    break;
            }
        }
      
        public bool Honk()
        {
            if (WillHonk)
            {
                _honkDelay--;

                if (_honkDelay < 0)
                {
                    SetHonkDelay();
                    return true;
                }
            }

            return false;
        }

        public void SetHonkDelay()
        {
            _honkDelay = _random.Next(30, 80);
        }

        #endregion
    }
}
