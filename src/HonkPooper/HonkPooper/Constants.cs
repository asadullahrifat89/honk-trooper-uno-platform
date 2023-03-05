﻿using System;

namespace HonkPooper
{
    static class Constants
    {
        public const double DEFAULT_FRAME_TIME = 19;
        public const double DEFAULT_SPEED_OFFSET = 3;

        public static (ConstructType ConstructType, double Height, double Width)[] CONSTRUCT_SIZES = new (ConstructType, double, double)[]
        {
            new (ConstructType.PLAYER, 200, 200),
            new (ConstructType.PLAYER_BOMB, 80, 80),
            new (ConstructType.PLAYER_BOMB_GROUND, 80, 80),

            new (ConstructType.BOSS, 200, 200),
            new (ConstructType.BOSS_BOMB, 80, 80),

            new (ConstructType.VEHICLE_SMALL, 150, 150),
            new (ConstructType.VEHICLE_LARGE, 180, 180),

            new (ConstructType.HONK, 90, 90),
            new (ConstructType.CLOUD, 220, 220),
            new (ConstructType.TREE, 150, 150),
            new (ConstructType.ROAD_MARK, 40, 15),

            new (ConstructType.DROP_SHADOW, 25, 60),
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

            new (ConstructType.HONK, new Uri("ms-appx:///HonkPooper/Assets/Images/honk_1.png")),
            new (ConstructType.HONK, new Uri("ms-appx:///HonkPooper/Assets/Images/honk_2.png")),
            new (ConstructType.HONK, new Uri("ms-appx:///HonkPooper/Assets/Images/honk_3.png")),

            new (ConstructType.PLAYER, new Uri("ms-appx:///HonkPooper/Assets/Images/player_1.png")),
            new (ConstructType.PLAYER, new Uri("ms-appx:///HonkPooper/Assets/Images/player_2.png")),

            new (ConstructType.PLAYER_BOMB, new Uri("ms-appx:///HonkPooper/Assets/Images/player_bomb_1.png")),
            new (ConstructType.PLAYER_BOMB, new Uri("ms-appx:///HonkPooper/Assets/Images/player_bomb_2.png")),
            new (ConstructType.PLAYER_BOMB, new Uri("ms-appx:///HonkPooper/Assets/Images/player_bomb_3.png")),

            new (ConstructType.PLAYER_BOMB_GROUND, new Uri("ms-appx:///HonkPooper/Assets/Images/cracker_1.png")),
            new (ConstructType.PLAYER_BOMB_GROUND, new Uri("ms-appx:///HonkPooper/Assets/Images/cracker_2.png")),

            new (ConstructType.BOMB_BLAST, new Uri("ms-appx:///HonkPooper/Assets/Images/blast_1.png")),
            new (ConstructType.BOMB_BLAST, new Uri("ms-appx:///HonkPooper/Assets/Images/blast_2.png")),

            new (ConstructType.CLOUD, new Uri("ms-appx:///HonkPooper/Assets/Images/cloud_1.png")),
            new (ConstructType.CLOUD, new Uri("ms-appx:///HonkPooper/Assets/Images/cloud_2.png")),
            new (ConstructType.CLOUD, new Uri("ms-appx:///HonkPooper/Assets/Images/cloud_3.png")),

            new (ConstructType.BOSS, new Uri("ms-appx:///HonkPooper/Assets/Images/boss_1.png")),
            new (ConstructType.BOSS, new Uri("ms-appx:///HonkPooper/Assets/Images/boss_2.png")),
            new (ConstructType.BOSS, new Uri("ms-appx:///HonkPooper/Assets/Images/boss_3.png")),

            new (ConstructType.BOSS_BOMB, new Uri("ms-appx:///HonkPooper/Assets/Images/boss_bomb_1.png")),
            new (ConstructType.BOSS_BOMB, new Uri("ms-appx:///HonkPooper/Assets/Images/boss_bomb_2.png")),
            new (ConstructType.BOSS_BOMB, new Uri("ms-appx:///HonkPooper/Assets/Images/boss_bomb_3.png")),
        };
    }

    public enum ConstructType
    {
        NONE,
        PLAYER,
        PLAYER_BOMB,
        PLAYER_BOMB_GROUND,
        BOMB_BLAST,
        VEHICLE_SMALL,
        VEHICLE_LARGE,
        COLLECTABLE,
        POWERUP,
        ROAD_MARK,
        HONK,
        COLLECTIBLE,
        TREE,
        HONKING_EMOJI,
        HONKING_BUSTED_EMOJI,
        CLOUD,
        DROP_SHADOW,
        BOSS,
        BOSS_BOMB,
    }
}