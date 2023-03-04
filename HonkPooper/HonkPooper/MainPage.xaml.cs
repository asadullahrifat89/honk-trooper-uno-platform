using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace HonkPooper
{
    public sealed partial class MainPage : Page
    {
        #region Fields

        private Scene _scene;
        private Random _random;

        #endregion

        #region Ctor

        public MainPage()
        {
            this.InitializeComponent();

            _scene = this.MainScene;
            _random = new Random();

            Loaded += MainPage_Loaded;
            Unloaded += MainPage_Unloaded;
        }

        #endregion

        #region Methods

        #region Vehicle

        public bool SpawnVehiclesInScene()
        {
            for (int i = 0; i < 10; i++)
            {
                Vehicle vehicle = new(
                    animateAction: AnimateVehicle,
                    recycleAction: RecycleVehicle,
                    scaling: _scene.Translation);

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
                                left: lane == 0 ? 0 : xLaneWidth - vehicle.Width * _scene.Translation,
                                top: vehicle.Height * -1);
                        }
                        break;
                    case 1:
                        {
                            var yLaneWidth = (_scene.Height / 2) / 2;

                            vehicle.SetPosition(
                                left: vehicle.Width * -1,
                                top: lane == 0 ? 0 : yLaneWidth * _scene.Translation);
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
                .FirstOrDefault(x => x.GetCloseHitBox().IntersectsWith(hitHox)) is Construct collidingVehicle)
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

            // only honk when vehicle is fully inside view

            if (/*hitHox.Left > 0 && hitHox.Top > 0 &&*/ vehicle1.Honk())
                GenerateHonkInScene(vehicle1);

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
                    scaling: _scene.Translation);

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
                    left: _scene.Width / 2 - tree.Width * _scene.Translation,
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
                    left: -1 * tree.Width * _scene.Translation,
                    top: _scene.Height / 2 * _scene.Translation,
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
                scaling: _scene.Translation);

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
                    scaling: _scene.Translation);

                honk.SetPosition(
                    left: -500,
                    top: -500);

                _scene.AddToScene(honk);
            }

            return true;
        }

        public bool GenerateHonkInScene(Vehicle vehicle)
        {
            //Honk honk = new(
            //    animateAction: AnimateHonk,
            //    recycleAction: RecycleHonk,
            //    scaling: _scene.Scaling)
            //{
            //    SpeedOffset = vehicle.SpeedOffset * 1.3,
            //    IsAnimating = true,
            //};

            //_scene.AddToScene(honk);

            if (_scene.Children.OfType<Honk>().FirstOrDefault(x => x.IsAnimating == false) is Honk honk)
            {
                honk.IsAnimating = true;
                honk.Opacity = 1;

                var hitBox = vehicle.GetCloseHitBox();

                honk.SetPosition(
                    left: hitBox.Left - vehicle.Width / 2,
                    top: hitBox.Top - (25 * _scene.Translation),
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
            speed *= _scene.Translation;
            construct.SetLeft(construct.GetLeft() + speed);
            construct.SetTop(construct.GetTop() + speed * construct.IsometricDisplacement);
        }

        #endregion

        #endregion

        #region Events

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += MainPage_SizeChanged;
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            var _windowWidth = args.NewSize.Width;
            var _windowHeight = args.NewSize.Height;

            _scene.Width = _windowWidth;
            _scene.Height = _windowHeight;

            //_scene.Width = 1920;
            //_scene.Height = 1080;
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= MainPage_SizeChanged;
        }

        private void InputView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {

        }

        private void InputView_PointerPressed(object sender, PointerRoutedEventArgs e)
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

            // add the honks which stay above all in z
            Generator honk = new(
                generationDelay: 0,
                generationAction: () => { return true; },
                spawnAction: SpawnHonksInScene);

            _scene.AddToScene(treeBottom);
            _scene.AddToScene(treeTop);

            _scene.AddToScene(roadMark);
            _scene.AddToScene(vehicle);

            _scene.Speed = 5;
            _scene.Start();
        }

        #endregion
    }
}
