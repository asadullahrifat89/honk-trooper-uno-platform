using System;

namespace HonkTrooper
{
    public partial class VehicleBase : DirectionalMovingConstruct
    {
        private readonly Random _random;

        public VehicleBase()
        {
            _random = new Random();
        }

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
                            left: lane == 0 ? 0 - Width / 3 : (xLaneWidth - Width / 1.5),
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
    }
}
