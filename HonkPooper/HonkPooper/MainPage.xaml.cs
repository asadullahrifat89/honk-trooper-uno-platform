using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Linq;

namespace HonkPooper
{
    public sealed partial class MainPage : Page
    {
        #region Fields

        private Scene _scene;
        private Controller _controller;
        private Random _random;
        private Player _player;
        private DropShadow _DropShadow;

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

            return true;
        }

        public bool AnimatePlayer(Construct player)
        {
            var speed = _scene.Speed;

            _player.Hover();

            if (_controller.IsMoveUp)
            {
                _player.MoveUp(speed, _scene.DownScaling);

            }
            else if (_controller.IsMoveDown)
            {
                _player.MoveDown(speed, _scene.DownScaling);
            }
            else
            {
                _player.StopMovement(_scene.DownScaling);
            }

            if (_controller.IsAttacking)
            {
                //if (!_scene.Children.OfType<PlayerBomb>().Any(x => x.IsAnimating))
                //{
                GenerateBombInScene();
                _controller.IsAttacking = false;
                //}
            }

            return true;
        }

        #endregion

        #region PlayerBomb

        public bool SpawnPlayerBombsInScene()
        {
            for (int i = 0; i < 5; i++)
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
            }

            return true;
        }

        public bool GenerateBombInScene()
        {
            if (_scene.Children.OfType<PlayerBomb>().FirstOrDefault(x => x.IsAnimating == false) is PlayerBomb bomb)
            {
                bomb.Reset();
                bomb.IsAnimating = true;

                bomb.Reposition(
                    player: _player,
                    downScaling: _scene.DownScaling);

                Console.WriteLine("Bomb dropped.");

                return true;
            }

            return false;
        }

        public bool AnimatePlayerBomb(Construct bomb)
        {
            PlayerBomb playerBomb = bomb as PlayerBomb;

            //var speed = (_scene.Speed + bomb.SpeedOffset);
            //MoveConstruct(bomb, speed);

            playerBomb.Gravitate(_DropShadow, _scene.DownScaling);

            return true;
        }

        public bool RecyclePlayerBomb(Construct bomb)
        {
            if (bomb.IsFadingComplete)
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

        #region PlayerDropShadow

        public bool SpawnPlayerDropShadowInScene()
        {
            _DropShadow = new(
                animateAction: AnimatePlayerDropShadow,
                recycleAction: (_player) => { return true; },
                downScaling: _scene.DownScaling);

            _scene.AddToScene(_DropShadow);

            _DropShadow.Move(
               player: _player,
               downScaling: _scene.DownScaling);

            _DropShadow.SetZ(6);

            _DropShadow.IsAnimating = true;

            return true;
        }

        public bool AnimatePlayerDropShadow(Construct DropShadow)
        {
            _DropShadow.Move(
                player: _player,
                downScaling: _scene.DownScaling);

            return true;
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

                vehicle.WillHonk = Convert.ToBoolean(_random.Next(0, 2));

                if (vehicle.WillHonk)
                {
                    vehicle.SetHonkDelay();
                }

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

                vehicle.SetZ(3);

                // Console.WriteLine("Vehicle generated.");
                return true;
            }

            return false;
        }

        private bool AnimateVehicle(Construct vehicle)
        {
            var speed = (_scene.Speed + vehicle.SpeedOffset);

            MoveConstruct(vehicle, speed);

            // TODO: fix hitbox for safe distance between vehicles

            var hitHox = vehicle.GetCloseHitBox();

            // prevent overlapping

            if (_scene.Children.OfType<Vehicle>()
                .FirstOrDefault(x => x.GetCloseHitBox().IntersectsWith(hitHox))
                is Construct collidingVehicle)
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

            // only honk when vehicle is fully inside view

            Vehicle vehicle1 = vehicle as Vehicle;

            if (vehicle1.Honk())
            {
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
            MoveConstruct(roadMark, speed);
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
                Construct tree = GenerateTree();

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

        private Construct GenerateTree()
        {
            Tree tree = new(
                animateAction: AnimateTree,
                recycleAction: RecycleTree,
                downScaling: _scene.DownScaling);

            return tree;
        }

        private bool AnimateTree(Construct tree)
        {
            var speed = (_scene.Speed + tree.SpeedOffset);
            MoveConstruct(tree, speed);
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
            if (_scene.Children.OfType<Honk>().FirstOrDefault(x => x.IsAnimating == false) is Honk honk)
            {
                honk.IsAnimating = true;
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
            var speed = (_scene.Speed + honk.SpeedOffset);
            MoveConstruct(honk, speed);
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

        #region Construct

        private void MoveConstruct(Construct construct, double speed)
        {
            speed *= _scene.DownScaling;
            construct.SetLeft(construct.GetLeft() + speed);
            construct.SetTop(construct.GetTop() + speed * construct.IsometricDisplacement);
        }

        #endregion

        #region Page

        private void PrepareScene()
        {
            // first add road marks
            Generator roadMark = new(
                generationDelay: 30,
                generationAction: GenerateRoadMarkInScene,
                spawnAction: SpawnRoadMarksInScene);

            // then add the top trees
            Generator treeTop = new(
                generationDelay: 40,
                generationAction: GenerateTreeInSceneTop,
                spawnAction: SpawnTreesInScene);

            // then add the vehicles which will appear forward in z wrt the top trees
            Generator vehicle = new(
                generationDelay: 80,
                generationAction: GenerateVehicleInScene,
                spawnAction: SpawnVehiclesInScene);

            // then add the bottom trees which will appear forward in z wrt to the vehicles
            Generator treeBottom = new(
                generationDelay: 40,
                generationAction: GenerateTreeInSceneBottom,
                spawnAction: SpawnTreesInScene);

            // add the honks which will appear forward in z wrt to everything on the road
            Generator honk = new(
                generationDelay: 0,
                generationAction: () => { return true; },
                spawnAction: SpawnHonksInScene);

            // add the player in scene which will appear forward in z wrt to all else
            Generator player = new(
               generationDelay: 0,
               generationAction: () => { return true; },
               spawnAction: SpawnPlayerInScene);

            // add the player drop zone in scene which will appear forward in z wrt to all else
            Generator playerDropShadow = new(
               generationDelay: 0,
               generationAction: () => { return true; },
               spawnAction: SpawnPlayerDropShadowInScene);

            Generator bomb = new(
              generationDelay: 0,
              generationAction: () => { return true; },
              spawnAction: SpawnPlayerBombsInScene);

            _scene.AddToScene(treeBottom);
            _scene.AddToScene(treeTop);

            _scene.AddToScene(roadMark);
            _scene.AddToScene(vehicle);

            _scene.AddToScene(player);
            _scene.AddToScene(playerDropShadow);

            _scene.AddToScene(bomb);

            _scene.Speed = 5;
        }

        private void SetController()
        {
            _controller.SetScene(_scene, _player);
        }

        #endregion

        #endregion

        #region Events

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _scene = this.MainScene;
            _controller = this.MainController;

            _scene.Width = 1920;
            _scene.Height = 1080;

            PrepareScene();
            SetController();

            SizeChanged += MainPage_SizeChanged;
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            var _windowWidth = args.NewSize.Width;
            var _windowHeight = args.NewSize.Height;

            _scene.Width = _windowWidth;
            _scene.Height = _windowHeight;

            _player.Reposition(_scene);

            _DropShadow.Move(
                player: _player,
                downScaling: _scene.DownScaling);
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= MainPage_SizeChanged;
        }

        #endregion      
    }
}
