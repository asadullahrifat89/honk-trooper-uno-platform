using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HonkPooper
{
    static class Constants
    {
        public const double DEFAULT_FRAME_TIME = 18;
        public const double TREE_SIZE = 200;

        public static (ConstructType ConstructType, double Height, double Width)[] CONSTRUCT_SIZES = new (ConstructType, double, double)[]
        {
             new (ConstructType.TREE, 150, 150),
             new (ConstructType.ROAD_MARK, 40, 15),
             new (ConstructType.VEHICLE_SMALL, 150, 150),
             new (ConstructType.VEHICLE_LARGE, 180, 180),
        };

        public static (ConstructType ConstructType, Uri Uri)[] CONSTRUCT_TEMPLATES = new (ConstructType, Uri)[]
        {
            new (ConstructType.TREE, new Uri("ms-appx:///HonkPooper/Assets/Images/tree_E.png")),

            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_small_1.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_small_2.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_small_3.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_small_4.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_small_5.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_small_6.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_small_7.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_small_8.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_small_9.png")),

            new (ConstructType.VEHICLE_LARGE, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_large_1.png")),
            new (ConstructType.VEHICLE_LARGE, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_large_2.png")),
            new (ConstructType.VEHICLE_LARGE, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_large_3.png")),
            new (ConstructType.VEHICLE_LARGE, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_large_4.png")),
            new (ConstructType.VEHICLE_LARGE, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_large_5.png")),
        };
    }

    public enum ConstructType
    {
        NONE,
        PLAYER,
        PLAYER_UPWARD,
        PLAYER_DOWNWARD,
        VEHICLE_SMALL,
        VEHICLE_LARGE,
        STICKER,
        POWERUP,
        ROAD_MARK,
        HONK,
        COLLECTIBLE,
        TREE,
        HONKING_EMOJI,
        HONKING_BUSTED_EMOJI,
    }
}
