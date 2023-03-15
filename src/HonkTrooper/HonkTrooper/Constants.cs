using System;

namespace HonkTrooper
{
    static class Constants
    {
        public const double DEFAULT_FRAME_TIME = 19;
        public const double DEFAULT_SPEED_OFFSET = 3;
        public const double DEFAULT_DROP_SHADOW_DISTANCE = 50;
        public const double DEFAULT_SLOW_MOTION_REDUCTION_FACTOR = 3;
        public const double DEFAULT_CONTROLLER_KEY_SIZE = 55;
        public const double DEFAULT_CONTROLLER_KEY_CORNER_RADIUS = 30;
        public const double DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS = 4;
        public const double DEFAULT_CONTROLLER_DIRECTION_KEYS_MARGIN = 6;
        public const double DEFAULT_GUI_FONT_SIZE = 30;

        public static (ConstructType ConstructType, double Height, double Width)[] CONSTRUCT_SIZES = new (ConstructType, double, double)[]
        {
            new (ConstructType.PLAYER, 170, 170),
            new (ConstructType.PLAYER_ROCKET, 80, 80),
            new (ConstructType.PLAYER_FIRE_CRACKER, 80, 80),
            new (ConstructType.PLAYER_ROCKET_SEEKING, 80, 80),

            new (ConstructType.BOSS, 180, 180),
            new (ConstructType.BOSS_ROCKET, 80, 80),
            new (ConstructType.BOSS_ROCKET_SEEKING, 80, 80),

            new (ConstructType.ENEMY, 140, 140),
            new (ConstructType.ENEMY_BOMB, 60, 60),

            new (ConstructType.VEHICLE_SMALL, 160, 160),
            new (ConstructType.VEHICLE_LARGE, 190, 190),

            new (ConstructType.HONK, 90, 90),
            new (ConstructType.CLOUD, 220, 220),
            new (ConstructType.TREE, 150, 150),
            new (ConstructType.ROAD_MARK, 40, 15),
            new (ConstructType.ROAD_SLAB, 600, 600),

            new (ConstructType.DROP_SHADOW, 25, 60),

            new (ConstructType.HEALTH_PICKUP, 100, 100),
            new (ConstructType.POWERUP_PICKUP, 100, 100),

            new (ConstructType.TITLE_SCREEN, 400, DEFAULT_CONTROLLER_KEY_SIZE * 10),
            new (ConstructType.INTERIM_SCREEN, 400, DEFAULT_CONTROLLER_KEY_SIZE * 10),
        };

        public static (ConstructType ConstructType, Uri Uri)[] CONSTRUCT_TEMPLATES = new (ConstructType, Uri)[]
        {
            new (ConstructType.TREE, new Uri("ms-appx:///HonkTrooper/Assets/Images/tree_E.png")),

            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_small_1.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_small_2.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_small_3.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_small_4.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_small_5.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_small_6.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_small_7.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_small_8.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_small_9.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_small_10.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_small_11.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_small_12.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_small_13.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_small_14.png")),
            new (ConstructType.VEHICLE_SMALL, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_small_15.png")),

            new (ConstructType.VEHICLE_LARGE, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_large_1.png")),
            new (ConstructType.VEHICLE_LARGE, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_large_2.png")),
            new (ConstructType.VEHICLE_LARGE, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_large_3.png")),
            new (ConstructType.VEHICLE_LARGE, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_large_4.png")),
            new (ConstructType.VEHICLE_LARGE, new Uri("ms-appx:///HonkTrooper/Assets/Images/vehicle_large_5.png")),

            new (ConstructType.HONK, new Uri("ms-appx:///HonkTrooper/Assets/Images/honk_1.png")),
            new (ConstructType.HONK, new Uri("ms-appx:///HonkTrooper/Assets/Images/honk_2.png")),
            new (ConstructType.HONK, new Uri("ms-appx:///HonkTrooper/Assets/Images/honk_3.png")),

            new (ConstructType.PLAYER, new Uri("ms-appx:///HonkTrooper/Assets/Images/player_1_idle.png")),

            new (ConstructType.PLAYER_IDLE, new Uri("ms-appx:///HonkTrooper/Assets/Images/player_1_idle.png")),
            new (ConstructType.PLAYER_ATTACK, new Uri("ms-appx:///HonkTrooper/Assets/Images/player_1_attack.png")),
            new (ConstructType.PLAYER_WIN, new Uri("ms-appx:///HonkTrooper/Assets/Images/player_1_win.png")),
            new (ConstructType.PLAYER_HIT, new Uri("ms-appx:///HonkTrooper/Assets/Images/player_1_hit.png")),

            new (ConstructType.PLAYER_ROCKET, new Uri("ms-appx:///HonkTrooper/Assets/Images/player_bomb_1.png")),
            new (ConstructType.PLAYER_ROCKET, new Uri("ms-appx:///HonkTrooper/Assets/Images/player_bomb_2.png")),
            new (ConstructType.PLAYER_ROCKET, new Uri("ms-appx:///HonkTrooper/Assets/Images/player_bomb_3.png")),

            new (ConstructType.PLAYER_FIRE_CRACKER, new Uri("ms-appx:///HonkTrooper/Assets/Images/cracker_1.png")),
            new (ConstructType.PLAYER_FIRE_CRACKER, new Uri("ms-appx:///HonkTrooper/Assets/Images/cracker_2.png")),

            new (ConstructType.PLAYER_ROCKET_SEEKING, new Uri("ms-appx:///HonkTrooper/Assets/Images/player_bomb_seeking_1.png")),
            new (ConstructType.PLAYER_ROCKET_SEEKING, new Uri("ms-appx:///HonkTrooper/Assets/Images/player_bomb_seeking_2.png")),

            new (ConstructType.BOMB_BLAST, new Uri("ms-appx:///HonkTrooper/Assets/Images/blast_1.png")),
            new (ConstructType.BOMB_BLAST, new Uri("ms-appx:///HonkTrooper/Assets/Images/blast_2.png")),

            new (ConstructType.CLOUD, new Uri("ms-appx:///HonkTrooper/Assets/Images/cloud_1.png")),
            new (ConstructType.CLOUD, new Uri("ms-appx:///HonkTrooper/Assets/Images/cloud_2.png")),
            new (ConstructType.CLOUD, new Uri("ms-appx:///HonkTrooper/Assets/Images/cloud_3.png")),

            new (ConstructType.BOSS, new Uri("ms-appx:///HonkTrooper/Assets/Images/boss_3_idle.png")),
            new (ConstructType.BOSS_HIT, new Uri("ms-appx:///HonkTrooper/Assets/Images/boss_3_hit.png")),

            new (ConstructType.BOSS_ROCKET, new Uri("ms-appx:///HonkTrooper/Assets/Images/boss_bomb_1.png")),
            new (ConstructType.BOSS_ROCKET, new Uri("ms-appx:///HonkTrooper/Assets/Images/boss_bomb_2.png")),
            new (ConstructType.BOSS_ROCKET, new Uri("ms-appx:///HonkTrooper/Assets/Images/boss_bomb_3.png")),

            new (ConstructType.BOSS_ROCKET_SEEKING, new Uri("ms-appx:///HonkTrooper/Assets/Images/boss_bomb_seeking.png")),

            new (ConstructType.ENEMY, new Uri("ms-appx:///HonkTrooper/Assets/Images/enemy_1.png")),
            new (ConstructType.ENEMY, new Uri("ms-appx:///HonkTrooper/Assets/Images/enemy_2.png")),

            new (ConstructType.ENEMY_BOMB, new Uri("ms-appx:///HonkTrooper/Assets/Images/enemy_bomb.png")),

            new (ConstructType.HEALTH_PICKUP, new Uri("ms-appx:///HonkTrooper/Assets/Images/health_pickup.png")),
            new (ConstructType.POWERUP_PICKUP_SEEKING_BALLS, new Uri("ms-appx:///HonkTrooper/Assets/Images/power_up_pickup_seeking_balls.png")),
            new (ConstructType.POWERUP_PICKUP_FORCE_SHIELD, new Uri("ms-appx:///HonkTrooper/Assets/Images/power_up_pickup_force_shield.png")),
        };

        public static (SoundType SoundType, Uri Uri)[] SOUND_TEMPLATES = new (SoundType, Uri)[]
        {
            new (SoundType.CRACKER_DROP, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/cracker_drop_1.mp3")),
            new (SoundType.CRACKER_DROP, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/cracker_drop_2.mp3")),

            new (SoundType.CRACKER_BLAST, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/cracker_blast_1.mp3")),
            new (SoundType.CRACKER_BLAST, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/cracker_blast_2.mp3")),
            new (SoundType.CRACKER_BLAST, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/cracker_blast_3.mp3")),
            new (SoundType.CRACKER_BLAST, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/cracker_blast_4.mp3")),

            new (SoundType.ROCKET_LAUNCH, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/rocket_launch_1.mp3")),
            new (SoundType.ROCKET_LAUNCH, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/rocket_launch_2.mp3")),
            new (SoundType.ROCKET_LAUNCH, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/rocket_launch_3.mp3")),

            new (SoundType.ROCKET_BLAST, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/rocket_blast_1.mp3")),
            new (SoundType.ROCKET_BLAST, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/rocket_blast_2.mp3")),
            new (SoundType.ROCKET_BLAST, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/rocket_blast_3.mp3")),

            new (SoundType.CAR_HONK, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/car_honk_1.mp3")),
            new (SoundType.CAR_HONK, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/car_honk_2.mp3")),
            new (SoundType.CAR_HONK, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/car_honk_3.mp3")),

            new (SoundType.SEEKER_ROCKET_LAUNCH, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/seeker_rocket_launch_1.mp3")),
            new (SoundType.SEEKER_ROCKET_LAUNCH, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/seeker_rocket_launch_2.mp3")),

            new (SoundType.AMBIENCE, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/ambience_1.mp3")),
            new (SoundType.AMBIENCE, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/ambience_2.mp3")),
            new (SoundType.AMBIENCE, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/ambience_3.mp3")),

            new (SoundType.BOSS_HOVERING, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/boss_hovering_1.mp3")),
            new (SoundType.BOSS_HOVERING, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/boss_hovering_2.mp3")),
            new (SoundType.BOSS_HOVERING, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/boss_hovering_3.mp3")),

            new (SoundType.BOSS_ENTRY, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/boss_entry_1.mp3")),
            new (SoundType.BOSS_ENTRY, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/boss_entry_2.mp3")),

            new (SoundType.BOSS_DEAD, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/boss_dead_1.mp3")),
            new (SoundType.BOSS_DEAD, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/boss_dead_2.mp3")),

            new (SoundType.POWERUP_PICKUP, new Uri("ms-appx:///HonkTrooper/Assets/Sounds/power_up_pickup_1.mp3")),
        };
    }

    public enum SoundType
    {
        CRACKER_DROP,
        CRACKER_BLAST,

        ROCKET_LAUNCH,
        ROCKET_BLAST,

        CAR_HONK,

        SEEKER_ROCKET_LAUNCH,

        AMBIENCE,

        BOSS_ENTRY,
        BOSS_HOVERING,
        BOSS_DEAD,

        POWERUP_PICKUP,
    }

    public enum ConstructType
    {
        NONE,

        PLAYER,
        PLAYER_IDLE,
        PLAYER_ATTACK,
        PLAYER_WIN,
        PLAYER_HIT,

        PLAYER_ROCKET,
        PLAYER_ROCKET_SEEKING,
        PLAYER_FIRE_CRACKER,

        VEHICLE_SMALL,
        VEHICLE_LARGE,

        ROAD_SLAB,
        ROAD_MARK,
        HONK,
        TREE,
        CLOUD,

        BOMB_BLAST,
        DROP_SHADOW,

        BOSS,
        BOSS_IDLE,
        BOSS_HIT,

        BOSS_ROCKET,
        BOSS_ROCKET_SEEKING,

        ENEMY,
        ENEMY_BOMB,

        HEALTH_PICKUP,
        POWERUP_PICKUP,
        POWERUP_PICKUP_SEEKING_BALLS,
        POWERUP_PICKUP_FORCE_SHIELD,
        COLLECTABLE_PICKUP,

        TITLE_SCREEN,
        INTERIM_SCREEN,
    }
}
