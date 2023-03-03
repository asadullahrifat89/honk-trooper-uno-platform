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

        private Uri[] _vehicle_small_uris;
        private Uri[] _vehicle_large_uris;

        #endregion

        #region Ctor

        public MainPage()
        {
            this.InitializeComponent();

            _scene = this.MainScene;

            _random = new Random();

            _vehicle_small_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.VEHICLE_SMALL).Select(x => x.Uri).ToArray();
            _vehicle_large_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.VEHICLE_LARGE).Select(x => x.Uri).ToArray();

            Loaded += MainPage_Loaded;
            Unloaded += MainPage_Unloaded;
        }

        #endregion

        #region Methods

        #region Vehicle

        public bool GenerateVehicleInScene()
        {
            var willHonk = _random.Next(0, 2);

            Dictionary<string, object> metaData = new Dictionary<string, object>
            {
                { "WillHonk", willHonk },
                { "IsHonkBusted", willHonk },
            };

            var vehicleType = _random.Next(0, 2);

            (ConstructType ConstructType, double Height, double Width) size;
            Uri uri;
            Construct vehicle = null;
            double speedOffset = _random.Next(-4, 2);

            switch (vehicleType)
            {
                case 0:
                    {
                        size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.VEHICLE_SMALL);

                        var vehicles = _vehicle_small_uris;
                        uri = vehicles[_random.Next(0, vehicles.Length)];

                        vehicle = new(
                            constructType: ConstructType.VEHICLE_SMALL,
                            width: size.Width * _scene.Scaling,
                            height: size.Height * _scene.Scaling,
                            animateAction: AnimateVehicle,
                            recycleAction: RecycleVehicle,
                            content: new Image()
                            {
                                Source = new BitmapImage(uriSource: uri)
                            },
                            speedOffset: speedOffset,
                            metaData: metaData);
                    }
                    break;
                case 1:
                    {
                        size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.VEHICLE_LARGE);

                        var vehicles = _vehicle_large_uris;
                        uri = vehicles[_random.Next(0, vehicles.Length)];

                        vehicle = new(
                            constructType: ConstructType.VEHICLE_LARGE,
                            width: size.Width * _scene.Scaling,
                            height: size.Height * _scene.Scaling,
                            animateAction: AnimateVehicle,
                            recycleAction: RecycleVehicle,
                            content: new Image()
                            {
                                Source = new BitmapImage(uriSource: uri)
                            },
                            speedOffset: speedOffset,
                            metaData: metaData);
                    }
                    break;
                default:
                    break;
            }

            _scene.AddToScene(vehicle);

            // generate top and left corner lane wise vehicles
            var topOrLeft = _random.Next(0, 2);

            var lane = _random.Next(0, 2);

            switch (topOrLeft)
            {
                case 0:
                    {
                        var xLaneWidth = _scene.Width / 4;

                        vehicle.SetPosition(
                            left: lane == 0 ? 0 : xLaneWidth - vehicle.Width * _scene.Scaling,
                            top: vehicle.Height * -1);
                    }
                    break;
                case 1:
                    {
                        var yLaneWidth = (_scene.Height / 2) / 2;

                        vehicle.SetPosition(
                            left: vehicle.Width * -1,
                            top: lane == 0 ? 0 : yLaneWidth * _scene.Scaling);
                    }
                    break;
                default:
                    break;
            }

            Console.WriteLine("Vehicle generated.");
            return true;
        }

        private bool AnimateVehicle(Construct vehicle)
        {
            var speed = _scene.Speed + vehicle.SpeedOffset;

            MoveConstruct(vehicle, speed);

            var hitHox = vehicle.GetHitBox();

            // prevent overlapping

            if (_scene.Children.OfType<Construct>()
                .Where(x => x.ConstructType == ConstructType.VEHICLE_SMALL || x.ConstructType == ConstructType.VEHICLE_LARGE)
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

            return true;
        }

        private bool RecycleVehicle(Construct vehicle)
        {
            var hitBox = vehicle.GetHitBox();

            if (hitBox.Top > _scene.Height || hitBox.Left > _scene.Width)
                _scene.DisposeFromScene(vehicle);

            return true;
        }

        #endregion

        #region RoadMark

        public bool GenerateRoadMarkInScene()
        {
            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.ROAD_MARK);

            Construct roadMark = new(
                constructType: ConstructType.ROAD_MARK,
                width: size.Width * _scene.Scaling,
                height: size.Height * _scene.Scaling,
                animateAction: AnimateRoadMark,
                recycleAction: RecycleRoadMark,
                speedOffset: 3)
            {
                Background = new SolidColorBrush(Colors.White),
                CornerRadius = new CornerRadius(5),
            };

            roadMark.SetSkewY(42);
            roadMark.SetRotation(-63.5);

            _scene.AddToScene(roadMark);

            roadMark.SetPosition(
              left: 0,
              top: 0);

            Console.WriteLine("Road Mark generated.");

            return true;
        }

        private bool AnimateRoadMark(Construct roadMark)
        {
            var speed = _scene.Speed + roadMark.SpeedOffset;
            MoveConstruct(roadMark, speed);
            return true;
        }

        private bool RecycleRoadMark(Construct roadMark)
        {
            var hitBox = roadMark.GetHitBox();

            if (hitBox.Top > _scene.Height || hitBox.Left > _scene.Width)
                _scene.DisposeFromScene(roadMark);

            return true;
        }

        #endregion

        #region Tree

        private bool GenerateTreeInSceneTop()
        {
            Construct tree = GenerateTree();

            _scene.AddToScene(tree);

            tree.SetPosition(
              left: _scene.Width / 2 - tree.Width * _scene.Scaling,
              top: tree.Height * -1);

            Console.WriteLine("Tree generated.");

            return true;
        }

        private bool GenerateTreeInSceneBottom()
        {
            Construct tree = GenerateTree();

            _scene.AddToScene(tree);

            tree.SetPosition(
                left: -1 * tree.Width * _scene.Scaling,
                top: (_scene.Height / 2 * _scene.Scaling));

            Console.WriteLine("Tree generated.");

            return true;
        }

        private Construct GenerateTree()
        {
            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.TREE);

            Construct tree = new(
                   constructType: ConstructType.TREE,
                   width: size.Width * _scene.Scaling,
                   height: size.Height * _scene.Scaling,
                   animateAction: AnimateTree,
                   recycleAction: RecycleTree,
                   content: new Image()
                   {
                       Source = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.TREE).Uri)
                   },
                   speedOffset: 3);

            return tree;
        }

        private bool AnimateTree(Construct tree)
        {
            var speed = _scene.Speed + tree.SpeedOffset;
            MoveConstruct(tree, speed);
            return true;
        }

        private bool RecycleTree(Construct tree)
        {
            var hitBox = tree.GetHitBox();

            if (hitBox.Top > _scene.Height || hitBox.Left > _scene.Width)
                _scene.DisposeFromScene(tree);

            return true;
        }

        #endregion

        #region Honk

        public bool GenerateHonkInScene()
        {
            return true;
        }

        #endregion

        #region Construct

        private void MoveConstruct(Construct construct, double speed)
        {
            construct.SetLeft(construct.GetLeft() + speed);
            construct.SetTop(construct.GetTop() + speed * 0.5);
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
            Generator treeBottom = new(generationDelay: 40, generationAction: GenerateTreeInSceneBottom);
            Generator treeTop = new(generationDelay: 40, generationAction: GenerateTreeInSceneTop);

            Generator roadMark = new(generationDelay: 30, generationAction: GenerateRoadMarkInScene);
            Generator vehicle = new(generationDelay: 80, generationAction: GenerateVehicleInScene);

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
