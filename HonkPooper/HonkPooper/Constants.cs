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

        public static (ConstructType ElementType, Uri Uri)[] ELEMENT_TEMPLATES = new (ConstructType, Uri)[]
        {
            new (ConstructType.TREE, new Uri("ms-appx:///HonkPooper/Assets/Images/tree_E.png")),
            new (ConstructType.VEHICLE, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_1.png")),
            new (ConstructType.VEHICLE, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_2.png")),
            new (ConstructType.VEHICLE, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_3.png")),
            new (ConstructType.VEHICLE, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_4.png")),
            new (ConstructType.VEHICLE, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_5.png")),
            new (ConstructType.VEHICLE, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_6.png")),
            new (ConstructType.VEHICLE, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_7.png")),
            new (ConstructType.VEHICLE, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_8.png")),
            new (ConstructType.VEHICLE, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_9.png")),
            new (ConstructType.VEHICLE, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_10.png")),
            new (ConstructType.VEHICLE, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_11.png")),
            new (ConstructType.VEHICLE, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_12.png")),
            new (ConstructType.VEHICLE, new Uri("ms-appx:///HonkPooper/Assets/Images/vehicle_13.png")),
        };        
    }

    public enum ConstructType
    {
        NONE,
        PLAYER,
        PLAYER_UPWARD,
        PLAYER_DOWNWARD,
        VEHICLE,
        BOSS_VEHICLE,
        BOSS_VEHICLE_UPWARD,
        BOSS_VEHICLE_DOWNWARD,
        STICKER,
        POWERUP,
        HONK,
        COLLECTIBLE,
        TREE,
        HONKING_EMOJI,
        HONKING_BUSTED_EMOJI,
    }

    public enum DestructionRule
    {
        None,

        ExitsRightBorder,
        ExitsTopRightBorder,
        ExitsBottomRightBorder,

        ExitsLeftBorder,
        ExitsTopLeftBorder,
        ExitsBottomLeftBorder,

        ExitsTopBorder,
        ExitsBottomBorder,

        FadesAway,
        Shrinks,
        Explodes,
    }

    public enum DestructionImpact
    {
        Remove,
        Recycle,
    }
}
