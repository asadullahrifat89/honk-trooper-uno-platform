using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using Windows.Graphics.Display;

namespace HonkTrooper
{
    public sealed partial class MainPage : Page
    {
        #region Fields

        private Scene _scene;
        private Controller _controller;
        private Random _random;
        private Player _player;

        private HealthBar _playerHealthBar;
        private HealthBar _bossHealthBar;

        private ScoreBar _scoreBar;

        #endregion

        #region Ctor

        public MainPage()
        {
            this.InitializeComponent();

            _random = new Random();

            Loaded += MainPage_Loaded;
            Unloaded += MainPage_Unloaded;
        }

        #endregion

        #region Properties

        public int BossPointScoreDiff { get; set; } = 50;

        #endregion

        #region Methods

        #region Player

        public bool SpawnPlayerInScene()
        {
            _player = new(
                animateAction: AnimatePlayer,
                recycleAction: (_player) => { return true; },
                downScaling: _scene.DownScaling);

            _scene.AddToScene(_player);

            _player.Reposition(_scene);

            _player.IsAnimating = true;

            SpawnDropShadowInScene(_player);

            DropShadow playersShadow = (_scene.Children.OfType<DropShadow>().FirstOrDefault(x => x.Id == _player.Id));
            playersShadow.IsAnimating = true;

            _playerHealthBar.SetMaxiumHealth(_player.Health);
            _playerHealthBar.SetHealth(_player.Health);
            _playerHealthBar.SetIcon(_player.GetContentUri());
            _playerHealthBar.SetBarForegroundColor(color: Colors.Purple);

            return true;
        }

        public bool AnimatePlayer(Construct player)
        {
            player.Pop();

            var speed = (_scene.Speed + player.SpeedOffset) * _scene.DownScaling;

            _player.Hover();

            if (_controller.IsMoveUp)
            {
                if (_player.GetBottom() > 0 && _player.GetRight() > 0)
                    _player.MoveUp(speed);
            }
            else if (_controller.IsMoveDown)
            {
                if (_player.GetTop() < _scene.Height && _player.GetLeft() < _scene.Width)
                    _player.MoveDown(speed);
            }
            else if (_controller.IsMoveLeft)
            {
                if (_player.GetRight() > 0 && _player.GetTop() < _scene.Height)
                    _player.MoveLeft(speed);
            }
            else if (_controller.IsMoveRight)
            {
                if (_player.GetLeft() < _scene.Width && _player.GetBottom() > 0)
                    _player.MoveRight(speed);
            }
            else
            {
                if (_player.GetBottom() > 0 && _player.GetRight() > 0
                    && _player.GetTop() < _scene.Height && _player.GetLeft() < _scene.Width
                    && _player.GetRight() > 0 && _player.GetTop() < _scene.Height
                    && _player.GetLeft() < _scene.Width && _player.GetBottom() > 0)
                    _player.StopMovement();
            }

            if (_controller.IsAttacking)
            {
                if (_scene.Children.OfType<Boss>().Any(x => x.IsAnimating && x.IsAttacking))
                {
                    GeneratePlayerBombInScene();
                }
                else
                {
                    GeneratePlayerBombGroundInScene();
                }

                _controller.IsAttacking = false;
            }

            return true;
        }

        #endregion

        #region PlayerBomb

        public bool SpawnPlayerBombsInScene()
        {
            for (int i = 0; i < 3; i++)
            {
                PlayerBomb bomb = new(
                    animateAction: AnimatePlayerBomb,
                    recycleAction: RecyclePlayerBomb,
                    downScaling: _scene.DownScaling);

                bomb.SetPosition(
                    left: -500,
                    top: -500,
                    z: 7);

                _scene.AddToScene(bomb);

                SpawnDropShadowInScene(source: bomb);
            }

            return true;
        }

        public bool GeneratePlayerBombInScene()
        {
            if (_scene.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is Boss boss &&
                _scene.Children.OfType<PlayerBomb>().FirstOrDefault(x => x.IsAnimating == false) is PlayerBomb bomb)
            {
                bomb.Reset();
                bomb.IsAnimating = true;
                bomb.SetPopping();

                bomb.Reposition(
                    Player: _player,
                    downScaling: _scene.DownScaling);

                SyncDropShadow(bomb);

                bomb.IsReverseMovement = _player.GetLeft() < boss.GetLeft();

                if (bomb.IsReverseMovement)
                {
                    bomb.SetRotation(-33);
                }
                else
                {
                    bomb.SetRotation(33);
                }

                Console.WriteLine("Player Bomb dropped.");

                return true;
            }

            return false;
        }

        public bool AnimatePlayerBomb(Construct bomb)
        {
            PlayerBomb playerBomb = bomb as PlayerBomb;

            var speed = _scene.Speed + bomb.SpeedOffset;

            MoveConstruct(construct: bomb, speed: speed, isReverse: !playerBomb.IsReverseMovement);

            if (playerBomb.IsBlasting)
            {
                bomb.Expand();
                bomb.Fade(0.02);

                DropShadow dropShadow = _scene.Children.OfType<DropShadow>().First(x => x.Id == bomb.Id);
                dropShadow.Opacity = bomb.Opacity;
            }
            else
            {
                bomb.Pop();

                if (_scene.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is Boss boss)
                {
                    if (playerBomb.GetCloseHitBox().IntersectsWith(boss.GetCloseHitBox()))
                    {
                        playerBomb.SetBlast();
                        boss.SetPopping();
                        boss.Health -= 10;

                        _bossHealthBar.SetHealth(boss.Health);

                        if (boss.Health <= 0)
                        {
                            boss.IsAttacking = false;
                        }

                        Console.WriteLine($"Boss Health: {boss.Health}");
                    }
                }
            }

            return true;
        }

        public bool RecyclePlayerBomb(Construct bomb)
        {
            var hitbox = bomb.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (bomb.IsFadingComplete ||
                hitbox.Left > _scene.Width || hitbox.Top > _scene.Height ||
                hitbox.Right < 0 || hitbox.Bottom < 0)
            {
                bomb.IsAnimating = false;

                bomb.SetPosition(
                    left: -500,
                    top: -500);

                return true;
            }

            return false;
        }

        #endregion

        #region PlayerBombGround

        public bool SpawnPlayerBombGroundsInScene()
        {
            for (int i = 0; i < 3; i++)
            {
                PlayerBombGround bomb = new(
                    animateAction: AnimatePlayerBombGround,
                    recycleAction: RecyclePlayerBombGround,
                    downScaling: _scene.DownScaling);

                bomb.SetPosition(
                    left: -500,
                    top: -500,
                    z: 7);

                _scene.AddToScene(bomb);

                SpawnDropShadowInScene(source: bomb);
            }

            return true;
        }

        public bool GeneratePlayerBombGroundInScene()
        {
            if (_scene.Children.OfType<PlayerBombGround>().FirstOrDefault(x => x.IsAnimating == false) is PlayerBombGround bomb)
            {
                bomb.Reset();
                bomb.IsAnimating = true;
                bomb.IsGravitating = true;
                bomb.SetPopping();

                bomb.SetRotation(_random.Next(-30, 30));

                bomb.Reposition(
                    player: _player,
                    downScaling: _scene.DownScaling);

                SyncDropShadow(bomb);

                Console.WriteLine("Player Ground Bomb dropped.");

                return true;
            }

            return false;
        }

        public bool AnimatePlayerBombGround(Construct bomb)
        {
            PlayerBombGround PlayerBombGround = bomb as PlayerBombGround;

            var speed = _scene.Speed + bomb.SpeedOffset;

            DropShadow dropShadow = _scene.Children.OfType<DropShadow>().First(x => x.Id == bomb.Id);

            if (PlayerBombGround.IsBlasting)
            {
                MoveConstruct(construct: bomb, speed: speed);

                bomb.Expand();
                bomb.Fade(0.02);

                // make the shadow fade with the bomb blast
                dropShadow.Opacity = bomb.Opacity;

                // while in blast check if it intersects with any vehicle, if it does then the vehicle stops honking and slows down
                if (_scene.Children.OfType<Vehicle>()
                    .Where(x => x.IsAnimating && x.WillHonk)
                    .FirstOrDefault(x => x.GetCloseHitBox().IntersectsWith(bomb.GetCloseHitBox())) is Vehicle vehicle)
                {
                    vehicle.SetBlast();
                    _scoreBar.GainScore(5);
                }
            }
            else
            {
                bomb.Pop();

                bomb.SetLeft(bomb.GetLeft() + speed);
                bomb.SetTop(bomb.GetTop() + speed);

                // start blast animation when the bomb touches it's shadow
                if (dropShadow.GetCloseHitBox().IntersectsWith(bomb.GetCloseHitBox()))
                    PlayerBombGround.SetBlast();
            }

            return true;
        }

        public bool RecyclePlayerBombGround(Construct bomb)
        {
            if (bomb.IsFadingComplete)
            {
                bomb.IsAnimating = false;
                bomb.IsGravitating = false;

                bomb.SetPosition(
                    left: -500,
                    top: -500);

                return true;
            }

            return false;
        }

        #endregion

        #region Vehicle

        public bool SpawnVehiclesInScene()
        {
            for (int i = 0; i < 10; i++)
            {
                Vehicle vehicle = new(
                    animateAction: AnimateVehicle,
                    recycleAction: RecycleVehicle,
                    downScaling: _scene.DownScaling);

                _scene.AddToScene(vehicle);

                vehicle.SetPosition(
                    left: -500,
                    top: -500);
            }

            return true;
        }

        public bool GenerateVehicleInScene()
        {
            if (_scene.Children.OfType<Vehicle>().FirstOrDefault(x => x.IsAnimating == false) is Vehicle vehicle)
            {
                vehicle.IsAnimating = true;
                vehicle.Reset();

                // generate top and left corner lane wise vehicles

                var topOrLeft = _random.Next(0, 2);

                var lane = _random.Next(0, 2);

                switch (topOrLeft)
                {
                    case 0:
                        {
                            var xLaneWidth = _scene.Width / 4;

                            vehicle.SetPosition(
                                left: lane == 0 ? 0 : xLaneWidth - vehicle.Width * _scene.DownScaling,
                                top: vehicle.Height * -1);
                        }
                        break;
                    case 1:
                        {
                            var yLaneWidth = (_scene.Height / 2) / 2;

                            vehicle.SetPosition(
                                left: vehicle.Width * -1,
                                top: lane == 0 ? 0 : yLaneWidth * _scene.DownScaling);
                        }
                        break;
                    default:
                        break;
                }

                //if (_scene.Children.OfType<Vehicle>().FirstOrDefault(x => x.IsAnimating && x.SpeedOffset == vehicle.SpeedOffset && x.GetCloseHitBox().IntersectsWith(vehicle.GetCloseHitBox())) is Vehicle overlappingVehicle)
                //{
                //    overlappingVehicle.SetPosition(
                //        left: -500,
                //        top: -500);

                //    overlappingVehicle.IsAnimating = false;

                //    Console.WriteLine("Overlapping vehicle removed.");
                //}

                vehicle.SetZ(3);

                // Console.WriteLine("Vehicle generated.");
                return true;
            }

            //foreach (Vehicle sourceVehicle in _scene.Children.OfType<Vehicle>().Where(x => x.IsAnimating))
            //{
            //    if (_scene.Children.OfType<Vehicle>().Any(x => x.IsAnimating && x.GetCloseHitBox().IntersectsWith(sourceVehicle.GetCloseHitBox())))
            //    {
            //        sourceVehicle.SetPosition(
            //                    left: -500,
            //                    top: -500);

            //        sourceVehicle.IsAnimating = false;
            //    }
            //}

            return false;
        }

        private bool AnimateVehicle(Construct vehicle)
        {
            var speed = (_scene.Speed + vehicle.SpeedOffset);

            MoveConstruct(construct: vehicle, speed: speed);

            // TODO: fix hitbox for safe distance between vehicles

            var hitHox = vehicle.GetCloseHitBox();

            // prevent overlapping

            if (_scene.Children.OfType<Vehicle>()
                .FirstOrDefault(x => x.GetHitBox().IntersectsWith(hitHox)) is Construct collidingVehicle)
            {
                if (collidingVehicle.SpeedOffset < vehicle.SpeedOffset)
                {
                    collidingVehicle.SpeedOffset = vehicle.SpeedOffset;
                }
                else if (collidingVehicle.SpeedOffset > vehicle.SpeedOffset)
                {
                    vehicle.SpeedOffset = collidingVehicle.SpeedOffset;
                }
            }

            Vehicle vehicle1 = vehicle as Vehicle;

            vehicle.Pop();

            if (vehicle1.Honk())
            {
                //vehicle.SetPopping();
                GenerateHonkInScene(vehicle1);
            }

            return true;
        }

        private bool RecycleVehicle(Construct vehicle)
        {
            var hitBox = vehicle.GetHitBox();

            if (hitBox.Top > _scene.Height || hitBox.Left > _scene.Width)
            {
                vehicle.SetPosition(
                    left: -500,
                    top: -500);

                vehicle.IsAnimating = false;
            }

            return true;
        }

        #endregion

        #region RoadMark

        public bool SpawnRoadMarksInScene()
        {
            for (int i = 0; i < 20; i++)
            {
                RoadMark roadMark = new(
                    animateAction: AnimateRoadMark,
                    recycleAction: RecycleRoadMark,
                    downScaling: _scene.DownScaling);

                roadMark.SetPosition(
                    left: -500,
                    top: -500);

                _scene.AddToScene(roadMark);
            }

            return true;
        }

        public bool GenerateRoadMarkInScene()
        {
            if (_scene.Children.OfType<RoadMark>().FirstOrDefault(x => x.IsAnimating == false) is RoadMark roadMark)
            {
                roadMark.IsAnimating = true;

                roadMark.SetPosition(
                    left: 0,
                    top: 0,
                    z: 1);

                // Console.WriteLine("Road Mark generated.");

                return true;
            }

            return false;
        }

        private bool AnimateRoadMark(Construct roadMark)
        {
            var speed = (_scene.Speed + roadMark.SpeedOffset);
            MoveConstruct(construct: roadMark, speed: speed);
            return true;
        }

        private bool RecycleRoadMark(Construct roadMark)
        {
            var hitBox = roadMark.GetHitBox();

            if (hitBox.Top > _scene.Height || hitBox.Left > _scene.Width)
            {
                roadMark.SetPosition(
                    left: -500,
                    top: -500);

                roadMark.IsAnimating = false;
            }

            return true;
        }

        #endregion

        #region Tree

        public bool SpawnTreesInScene()
        {
            for (int i = 0; i < 10; i++)
            {
                Tree tree = new(
                    animateAction: AnimateTree,
                    recycleAction: RecycleTree,
                    downScaling: _scene.DownScaling);

                tree.SetPosition(
                    left: -500,
                    top: -500,
                    z: 2);

                _scene.AddToScene(tree);
            }

            return true;
        }

        private bool GenerateTreeInSceneTop()
        {
            if (_scene.Children.OfType<Tree>().FirstOrDefault(x => x.IsAnimating == false) is Tree tree)
            {
                tree.IsAnimating = true;

                tree.SetPosition(
                    left: _scene.Width / 2 - tree.Width * _scene.DownScaling,
                    top: tree.Height * -1,
                    z: 2);

                // Console.WriteLine("Tree generated.");

                return true;
            }

            return false;
        }

        private bool GenerateTreeInSceneBottom()
        {
            if (_scene.Children.OfType<Tree>().FirstOrDefault(x => x.IsAnimating == false) is Tree tree)
            {
                tree.IsAnimating = true;

                tree.SetPosition(
                    left: -1 * tree.Width * _scene.DownScaling,
                    top: _scene.Height / 2 * _scene.DownScaling,
                    z: 4);

                // Console.WriteLine("Tree generated.");

                return true;
            }

            return false;
        }

        private bool AnimateTree(Construct tree)
        {
            var speed = (_scene.Speed + tree.SpeedOffset);
            MoveConstruct(construct: tree, speed: speed);
            return true;
        }

        private bool RecycleTree(Construct tree)
        {
            var hitBox = tree.GetHitBox();

            if (hitBox.Top > _scene.Height || hitBox.Left > _scene.Width)
            {
                tree.SetPosition(
                    left: -500,
                    top: -500);

                tree.IsAnimating = false;
            }

            return true;
        }

        #endregion

        #region HealthPickup

        public bool SpawnHealthPickupsInScene()
        {
            for (int i = 0; i < 3; i++)
            {
                HealthPickup HealthPickup = new(
                    animateAction: AnimateHealthPickup,
                    recycleAction: RecycleHealthPickup,
                    downScaling: _scene.DownScaling);

                HealthPickup.SetPosition(
                    left: -500,
                    top: -500,
                    z: 6);

                _scene.AddToScene(HealthPickup);
            }

            return true;
        }

        private bool GenerateHealthPickupsInScene()
        {
            if (HealthPickup.ShouldGenerate(_player.Health) &&
                _scene.Children.OfType<HealthPickup>().FirstOrDefault(x => x.IsAnimating == false) is HealthPickup healthPickup)
            {
                healthPickup.IsAnimating = true;
                healthPickup.Reset();

                var topOrLeft = _random.Next(0, 2);

                var lane = _random.Next(0, 2);

                switch (topOrLeft)
                {
                    case 0:
                        {
                            var xLaneWidth = _scene.Width / 4;
                            healthPickup.SetPosition(
                                left: _random.Next(0, Convert.ToInt32(xLaneWidth - healthPickup.Width)) * _scene.DownScaling,
                                top: healthPickup.Height * -1);
                        }
                        break;
                    case 1:
                        {
                            var yLaneWidth = (_scene.Height / 2) / 2;
                            healthPickup.SetPosition(
                                left: healthPickup.Width * -1,
                                top: _random.Next(0, Convert.ToInt32(yLaneWidth)) * _scene.DownScaling);
                        }
                        break;
                    default:
                        break;
                }

                SyncDropShadow(healthPickup);

                return true;
            }

            return false;
        }

        private bool AnimateHealthPickup(Construct healthPickup)
        {
            var speed = _scene.Speed + healthPickup.SpeedOffset;

            HealthPickup healthPickup1 = healthPickup as HealthPickup;

            if (healthPickup1.IsPickedUp)
            {
                healthPickup1.Shrink();
            }
            else
            {
                MoveConstruct(construct: healthPickup, speed: speed);

                var hitbox = healthPickup.GetCloseHitBox();

                if (_player.GetCloseHitBox().IntersectsWith(hitbox))
                {
                    _player.Health += 10;
                    _playerHealthBar.SetHealth(_player.Health);
                    healthPickup1.IsPickedUp = true;
                }
            }

            return true;
        }

        private bool RecycleHealthPickup(Construct healthPickup)
        {
            var hitBox = healthPickup.GetHitBox();

            if (hitBox.Top > _scene.Height || hitBox.Left > _scene.Width || healthPickup.IsShrinkingComplete)
            {
                healthPickup.SetPosition(
                    left: -500,
                    top: -500);

                healthPickup.IsAnimating = false;
            }

            return true;
        }

        #endregion

        #region Honk

        public bool SpawnHonksInScene()
        {
            for (int i = 0; i < 10; i++)
            {
                Honk honk = new(
                    animateAction: AnimateHonk,
                    recycleAction: RecycleHonk,
                    downScaling: _scene.DownScaling);

                honk.SetPosition(
                    left: -500,
                    top: -500);

                _scene.AddToScene(honk);
            }

            return true;
        }

        public bool GenerateHonkInScene(Vehicle vehicle)
        {
            // if there are no bosses in the scene the vehicles will honk
            if (_scene.Children.OfType<Honk>().FirstOrDefault(x => x.IsAnimating == false) is Honk honk &&
                !_scene.Children.OfType<Boss>().Any(x => x.IsAnimating && x.IsAttacking))
            {
                honk.IsAnimating = true;
                honk.SetPopping();

                honk.Reset();

                var hitBox = vehicle.GetCloseHitBox();

                honk.SetPosition(
                    left: hitBox.Left - vehicle.Width / 2,
                    top: hitBox.Top - (25 * _scene.DownScaling),
                    z: 5);

                honk.SetRotation(_random.Next(-30, 30));
                honk.SetZ(vehicle.GetZ() + 1);

                return true;
            }

            return false;
        }

        public bool AnimateHonk(Construct honk)
        {
            honk.Pop();
            var speed = (_scene.Speed + honk.SpeedOffset);
            MoveConstruct(construct: honk, speed: speed);
            return true;
        }

        private bool RecycleHonk(Construct honk)
        {
            Honk honk1 = honk as Honk;

            honk1.Fade();

            if (honk1.IsFadingComplete)
            {
                honk.IsAnimating = false;

                honk.SetPosition(
                    left: -500,
                    top: -500);
            }
            //_scene.DisposeFromScene(honk);

            return true;
        }

        #endregion

        #region Cloud

        public bool SpawnCloudsInScene()
        {
            for (int i = 0; i < 10; i++)
            {
                Cloud cloud = new(
                    animateAction: AnimateCloud,
                    recycleAction: RecycleCloud,
                    downScaling: _scene.DownScaling);

                cloud.SetPosition(
                    left: -500,
                    top: -500,
                    z: 9);

                _scene.AddToScene(cloud);

                SpawnDropShadowInScene(source: cloud);
            }

            return true;
        }

        private bool GenerateCloudInScene()
        {
            if (_scene.Children.OfType<Cloud>().FirstOrDefault(x => x.IsAnimating == false) is Cloud cloud)
            {
                cloud.IsAnimating = true;
                cloud.Reset();

                var topOrLeft = _random.Next(0, 2);

                var lane = _random.Next(0, 2);

                switch (topOrLeft)
                {
                    case 0:
                        {
                            var xLaneWidth = _scene.Width / 4;
                            cloud.SetPosition(
                                left: _random.Next(0, Convert.ToInt32(xLaneWidth - cloud.Width)) * _scene.DownScaling,
                                top: cloud.Height * -1);
                        }
                        break;
                    case 1:
                        {
                            var yLaneWidth = (_scene.Height / 2) / 2;
                            cloud.SetPosition(
                                left: cloud.Width * -1,
                                top: _random.Next(0, Convert.ToInt32(yLaneWidth)) * _scene.DownScaling);
                        }
                        break;
                    default:
                        break;
                }

                SyncDropShadow(cloud);

                return true;
            }

            return false;
        }

        public bool AnimateCloud(Construct cloud)
        {
            var speed = (_scene.Speed + cloud.SpeedOffset);
            MoveConstruct(construct: cloud, speed: speed);
            return true;
        }

        private bool RecycleCloud(Construct cloud)
        {
            var hitBox = cloud.GetHitBox();

            if (hitBox.Top > _scene.Height || hitBox.Left > _scene.Width)
            {
                cloud.SetPosition(
                    left: -500,
                    top: -500);

                cloud.IsAnimating = false;
            }

            return true;
        }

        #endregion

        #region Boss

        public bool SpawnBossesInScene()
        {
            for (int i = 0; i < 3; i++)
            {
                Boss boss = new(
                    animateAction: AnimateBoss,
                    recycleAction: RecycleBoss,
                    downScaling: _scene.DownScaling);

                boss.SetPosition(
                    left: -500,
                    top: -500,
                    z: 8);

                _scene.AddToScene(boss);

                SpawnDropShadowInScene(source: boss);
            }

            return true;
        }

        private bool GenerateBossInScene()
        {
            //TODO: _scoreBar.IsBossPointScore(BossPointScoreDiff) &&

            // if scene doesn't contain a boss then pick a random boss and add to scene
            if (_scoreBar.IsBossPointScore(BossPointScoreDiff) &&
                !_scene.Children.OfType<Boss>().Any(x => x.IsAnimating) &&
                _scene.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating == false) is Boss boss)
            {
                boss.IsAnimating = true;
                boss.Reset();
                boss.SetPosition(
                    left: 0,
                    top: boss.Height * -1);

                SyncDropShadow(boss);

                // set boss health
                boss.Health = BossPointScoreDiff * 1.5;

                _bossHealthBar.SetMaxiumHealth(boss.Health);
                _bossHealthBar.SetHealth(boss.Health);
                _bossHealthBar.SetIcon(boss.GetContentUri());
                _bossHealthBar.SetBarForegroundColor(color: Colors.Crimson);

                // next boss will appear at a slightly higher score
                BossPointScoreDiff += 5;

                return true;
            }

            return false;
        }

        public bool AnimateBoss(Construct boss)
        {
            Boss boss1 = boss as Boss;

            if (boss1.Health <= 0)
            {
                boss.Shrink();

                //Console.WriteLine($"Boss ScaleX: {boss.GetScaleX()} ScaleY: {boss.GetScaleY()}");
            }
            else
            {
                boss.Pop();

                var speed = (_scene.Speed + boss.SpeedOffset) * _scene.DownScaling;

                // bring boss to a suitable distance from player and then start attacking

                if (!boss1.IsAttacking /*&& boss.GetLeft() < _scene.Width / 2*/)
                {
                    MoveConstruct(construct: boss, speed: speed);
                }

                if (boss.GetRight() > _scene.Width / 2)
                {
                    if (boss1.IsAttacking &&
                        !boss1.AwaitMoveLeft && !boss1.AwaitMoveRight &&
                        !boss1.AwaitMoveUp && !boss1.AwaitMoveDown)
                    {
                        boss1.AwaitMoveLeft = true;
                    }
                    else
                    {
                        boss1.IsAttacking = true;
                    }
                }

                if (boss1.IsAttacking)
                {
                    if (boss1.AwaitMoveLeft)
                    {
                        boss1.MoveLeft(speed);

                        if (boss.GetLeft() < 0 || boss.GetBottom() > _scene.Height)
                        {
                            boss1.AwaitMoveLeft = false;
                            boss1.AwaitMoveRight = true;
                        }
                    }
                    else
                    {
                        if (boss1.AwaitMoveRight)
                        {
                            boss1.MoveRight(speed);

                            if (boss.GetTop() < 0)
                            {
                                boss1.AwaitMoveRight = false;
                                boss1.AwaitMoveDown = true;
                            }
                        }
                        else
                        {
                            if (boss1.AwaitMoveDown)
                            {
                                boss1.MoveDown(speed);

                                if (boss1.GetRight() > _scene.Width || boss1.GetBottom() > _scene.Height)
                                {
                                    boss1.AwaitMoveUp = true;
                                    boss1.AwaitMoveDown = false;
                                }
                            }
                            else
                            {
                                if (boss1.AwaitMoveUp)
                                {
                                    boss1.MoveUp(speed);

                                    if (boss1.GetTop() < 0 || boss1.GetLeft() < 0)
                                    {
                                        boss1.AwaitMoveUp = false;
                                        boss1.AwaitMoveLeft = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            boss1.Hover();

            return true;
        }

        private bool RecycleBoss(Construct boss)
        {
            if (boss.IsShrinkingComplete)
            {
                boss.SetPosition(
                    left: -500,
                    top: -500);

                boss.IsAnimating = false;

                _scoreBar.GainScore(5);
            }

            return true;
        }

        #endregion

        #region BossBomb

        public bool SpawnBossBombsInScene()
        {
            for (int i = 0; i < 5; i++)
            {
                BossBomb bomb = new(
                    animateAction: AnimateBossBomb,
                    recycleAction: RecycleBossBomb,
                    downScaling: _scene.DownScaling);

                bomb.SetPosition(
                    left: -500,
                    top: -500,
                    z: 7);

                _scene.AddToScene(bomb);

                SpawnDropShadowInScene(source: bomb);
            }

            return true;
        }

        public bool GenerateBossBombInScene()
        {
            if (_scene.Children.OfType<Boss>().FirstOrDefault(x => x.IsAnimating && x.IsAttacking) is Boss boss &&
                _scene.Children.OfType<BossBomb>().FirstOrDefault(x => x.IsAnimating == false) is BossBomb bossBomb)
            {
                bossBomb.Reset();
                bossBomb.IsAnimating = true;
                bossBomb.SetPopping();

                bossBomb.Reposition(
                    boss: boss,
                    downScaling: _scene.DownScaling);

                SyncDropShadow(bossBomb);

                if (boss.AwaitMoveLeft || boss.AwaitMoveRight)
                {
                    // player is on the right side of the boss
                    if (_player.GetLeft() > boss.GetRight())
                    {
                        bossBomb.AwaitMoveDown = true;
                        bossBomb.SetRotation(33);
                    }
                    else
                    {
                        bossBomb.AwaitMoveUp = true;
                        bossBomb.SetRotation(125);
                    }
                }
                else if (boss.AwaitMoveUp || boss.AwaitMoveDown)
                {
                    // player is above the boss
                    if (_player.GetBottom() < boss.GetTop())
                    {
                        bossBomb.AwaitMoveRight = true;
                        bossBomb.SetRotation(-33);
                    }
                    else
                    {
                        bossBomb.AwaitMoveLeft = true;
                        bossBomb.SetRotation(125);
                    }
                }

                Console.WriteLine("Boss Bomb dropped.");

                return true;
            }

            return false;
        }

        public bool AnimateBossBomb(Construct bomb)
        {
            BossBomb bossBomb = bomb as BossBomb;

            var speed = (_scene.Speed + bomb.SpeedOffset) * _scene.DownScaling;

            if (bossBomb.AwaitMoveLeft)
            {
                bossBomb.MoveLeft(speed);
            }
            else if (bossBomb.AwaitMoveRight)
            {
                bossBomb.MoveRight(speed);
            }
            else if (bossBomb.AwaitMoveUp)
            {
                bossBomb.MoveUp(speed);
            }
            else
            {
                bossBomb.MoveDown(speed);
            }

            if (bossBomb.IsBlasting)
            {
                bomb.Expand();
                bomb.Fade(0.02);

                DropShadow dropShadow = _scene.Children.OfType<DropShadow>().First(x => x.Id == bomb.Id);
                dropShadow.Opacity = bomb.Opacity;
            }
            else
            {
                bomb.Pop();

                if (bossBomb.GetCloseHitBox().IntersectsWith(_player.GetCloseHitBox()))
                {
                    bossBomb.SetBlast();
                    _player.SetPopping();
                    _player.Health -= 5;
                    _playerHealthBar.SetHealth(_player.Health);
                }
            }

            return true;
        }

        public bool RecycleBossBomb(Construct bomb)
        {
            var hitbox = bomb.GetHitBox();

            // if bomb is blasted and faed or goes out of scene bounds
            if (bomb.IsFadingComplete || hitbox.Left > _scene.Width || hitbox.Right < 0 || hitbox.Top < 0 || hitbox.Bottom > _scene.Height)
            {
                bomb.IsAnimating = false;

                bomb.SetPosition(
                    left: -500,
                    top: -500);

                return true;
            }

            return false;
        }

        #endregion

        #region DropShadow

        public bool SpawnDropShadowInScene(Construct source)
        {
            DropShadow dropShadow = new(
                animateAction: AnimateDropShadow,
                recycleAction: RecycleDropShadow,
                downScaling: _scene.DownScaling);

            _scene.AddToScene(dropShadow);

            dropShadow.SetParent(construct: source);
            dropShadow.Move();
            dropShadow.SetZ(source.GetZ());

            return true;
        }

        public bool AnimateDropShadow(Construct construct)
        {
            DropShadow dropShadow = construct as DropShadow;

            // adjust shadow with with the source
            if (dropShadow.Width != dropShadow.ParentConstruct.Width * 0.7)
                dropShadow.Width = dropShadow.ParentConstruct.Width * 0.7;

            dropShadow.Move();

            return true;
        }

        private bool RecycleDropShadow(Construct dropShadow)
        {
            DropShadow dropShadow1 = dropShadow as DropShadow;

            if (!dropShadow1.ParentConstruct.IsAnimating)
            {
                dropShadow.IsAnimating = false;

                dropShadow.SetPosition(
                    left: -500,
                    top: -500);

                return true;
            }

            return false;
        }

        private void SyncDropShadow(Construct source)
        {
            if (_scene.Children.OfType<DropShadow>().FirstOrDefault(x => x.Id == source.Id) is DropShadow dropShadow)
            {
                dropShadow.Opacity = 1;
                dropShadow.ParentConstructSpeed = _scene.Speed + source.SpeedOffset;
                dropShadow.IsAnimating = true;

                dropShadow.Reset();
            }
        }

        #endregion

        #region Construct

        private void MoveConstruct(Construct construct, double speed, bool isReverse = false)
        {
            speed *= _scene.DownScaling;

            if (isReverse)
            {
                construct.SetLeft(construct.GetLeft() - speed);
                construct.SetTop(construct.GetTop() - speed * construct.IsometricDisplacement);
            }
            else
            {
                construct.SetLeft(construct.GetLeft() + speed);
                construct.SetTop(construct.GetTop() + speed * construct.IsometricDisplacement);
            }
        }

        #endregion

        #region Scene

        private void PrepareScene()
        {
            _scene.Children.Clear();

            // first add road marks
            Generator roadMarks = new(
                generationDelay: 30,
                generationAction: GenerateRoadMarkInScene,
                startUpAction: SpawnRoadMarksInScene);

            // then add the top trees
            Generator treeTops = new(
                generationDelay: 35,
                generationAction: GenerateTreeInSceneTop,
                startUpAction: SpawnTreesInScene);

            // then add the vehicles which will appear forward in z wrt the top trees
            Generator vehicles = new(
                generationDelay: 80,
                generationAction: GenerateVehicleInScene,
                startUpAction: SpawnVehiclesInScene);

            // then add the bottom trees which will appear forward in z wrt to the vehicles
            Generator treeBottoms = new(
                generationDelay: 35,
                generationAction: GenerateTreeInSceneBottom,
                startUpAction: SpawnTreesInScene);

            // add the honks which will appear forward in z wrt to everything on the road
            Generator honk = new(
                generationDelay: 0,
                generationAction: () => { return true; },
                startUpAction: SpawnHonksInScene);

            // add the player in scene which will appear forward in z wrt to all else
            Generator player = new(
                generationDelay: 0,
                generationAction: () => { return true; },
                startUpAction: SpawnPlayerInScene);

            Generator playerBombs = new(
              generationDelay: 0,
              generationAction: () => { return true; },
              startUpAction: SpawnPlayerBombsInScene);

            Generator playerGroundBombs = new(
                generationDelay: 0,
                generationAction: () => { return true; },
                startUpAction: SpawnPlayerBombGroundsInScene);

            // add the clouds which are abve the player z
            Generator clouds = new(
                generationDelay: 100,
                generationAction: GenerateCloudInScene,
                startUpAction: SpawnCloudsInScene);

            Generator bosses = new(
               generationDelay: 100,
               generationAction: GenerateBossInScene,
               startUpAction: SpawnBossesInScene);

            Generator bossBombs = new(
               generationDelay: 30,
               generationAction: GenerateBossBombInScene,
               startUpAction: SpawnBossBombsInScene,
               randomizeDelay: true);

            Generator healthPickups = new(
             generationDelay: 300,
             generationAction: GenerateHealthPickupsInScene,
             startUpAction: SpawnHealthPickupsInScene);

            _scene.AddToScene(treeBottoms);
            _scene.AddToScene(treeTops);

            _scene.AddToScene(roadMarks);
            _scene.AddToScene(vehicles);

            _scene.AddToScene(player);
            _scene.AddToScene(playerBombs);
            _scene.AddToScene(playerGroundBombs);

            _scene.AddToScene(clouds);

            _scene.AddToScene(bosses);
            _scene.AddToScene(bossBombs);

            _scene.AddToScene(healthPickups);

            _scene.Speed = 5;
        }

        private void SetController()
        {
            _controller.SetScene(_scene);
            _controller.RequiresScreenOrientationChange += Controller_RequiresScreenOrientationChange;
        }

        #endregion

        #endregion

        #region Events

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _scene = this.MainScene;
            _playerHealthBar = this.PlayerHealthBar;
            _bossHealthBar = this.BossHealthBar;

            _controller = this.KeyboardController;
            _scoreBar = this.GameScoreBar;

            _scene.Width = 1920;
            _scene.Height = 1080;

            BossPointScoreDiff = 50;

            PrepareScene();
            SetController();

            SizeChanged += MainPage_SizeChanged;

            ScreenExtensions.DisplayInformation.OrientationChanged += DisplayInformation_OrientationChanged;
            ScreenExtensions.RequiredDisplayOrientation = DisplayOrientations.Landscape;

            if (ScreenExtensions.GetDisplayOrienation() != ScreenExtensions.RequiredDisplayOrientation)
            {
                ScreenExtensions.SetDisplayOrientation(ScreenExtensions.RequiredDisplayOrientation);
            }
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            var _windowWidth = args.NewSize.Width;
            var _windowHeight = args.NewSize.Height;

            _scene.Width = _windowWidth;
            _scene.Height = _windowHeight;

            _player.Reposition(_scene);

            DropShadow playersShadow = (_scene.Children.OfType<DropShadow>().FirstOrDefault(x => x.Id == _player.Id));
            playersShadow.Move();
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= MainPage_SizeChanged;
            _controller.RequiresScreenOrientationChange -= Controller_RequiresScreenOrientationChange;
            ScreenExtensions.DisplayInformation.OrientationChanged -= DisplayInformation_OrientationChanged;
        }

        private void Controller_RequiresScreenOrientationChange(object sender, DisplayOrientations e)
        {
            Console.WriteLine($"Required Orientation {e}");
        }

        private void DisplayInformation_OrientationChanged(Windows.Graphics.Display.DisplayInformation sender, object args)
        {
            if (_scene.IsAnimating)
                _scene.Stop();

            Console.WriteLine($"{sender.CurrentOrientation}");
        }

        #endregion      
    }
}
