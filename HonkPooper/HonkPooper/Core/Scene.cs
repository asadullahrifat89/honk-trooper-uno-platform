using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.Foundation;

namespace HonkPooper
{
    public partial class Scene : Canvas
    {
        #region Fields

        private double _sceneWidth, _sceneHeight;

        private Stopwatch _stopwatch;

        private PeriodicTimer _gameViewTimer;
        private readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(Constants.DEFAULT_FRAME_TIME);

        private readonly List<Construct> _destroyables = new();
        private readonly List<Generator> _generators = new();

        #endregion

        #region Ctor

        public Scene()
        {
            Loaded += Scene_Loaded;
            Unloaded += Scene_Unloaded;
        }

        #endregion

        #region Properties

        public double Scaling { get; set; }

        public double Speed { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a construct to the scene
        /// </summary>
        /// <param name="construct"></param>
        public void AddToScene(Construct construct)
        {
            Children.Add(construct);
        }

        public void AddToScene(Generator generator)
        {
            _generators.Add(generator);
        }

        public void DisposeFromScene(Construct construct)
        {
            _destroyables.Add(construct);
        }

        /// <summary>
        /// Starts the timer for the scene and starts the scene loop.
        /// </summary>
        public async void Start()
        {
            _stopwatch = Stopwatch.StartNew();
            _gameViewTimer = new PeriodicTimer(_frameTime);

            while (await _gameViewTimer.WaitForNextTickAsync())
                Animate();
        }

        /// <summary>
        /// Stops the timer for the scene.
        /// </summary>
        public void Stop()
        {
            _stopwatch?.Stop();
            _gameViewTimer?.Dispose();
        }

        /// <summary>
        /// Gets the scaling factor according to window size.
        /// </summary>
        /// <param name="windowWidth"></param>
        /// <returns></returns>
        private double GetScalingFactor(double windowWidth)
        {
            return windowWidth switch
            {
                <= 300 => 0.50,
                <= 400 => 0.55,
                <= 500 => 0.60,
                <= 700 => 0.65,
                <= 900 => 0.70,
                <= 950 => 0.75,
                <= 1000 => 0.80,
                <= 1400 => 0.85,
                <= 2000 => 0.90,
                _ => 1,
            };
        }

        /// <summary>
        /// Executes actions of the constructs.
        /// </summary>
        private void Animate()
        {
            // generate new constructs in scene from generators
            foreach (Generator generator in _generators)
            {
                generator.Generate();
            }

            // run action for each construct and add to destroyable if destroyable function returns true
            foreach (Construct construct in Children.OfType<Construct>().Where(x => x.IsAnimating))
            {
                construct.Animate();
                construct.Recycle();
            }

            // remove the destroyables from the scene
            foreach (Construct destroyable in _destroyables)
            {
                Children.Remove(destroyable);
            }

            _destroyables.Clear();

            Console.WriteLine($"Object Count: {Children.OfType<Construct>().Count(x => x.IsAnimating)}");
        }

        #endregion

        #region Events

        private void Scene_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= Scene_SizeChanged;
            Stop();
        }

        private void Scene_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += Scene_SizeChanged;
        }

        private void Scene_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            _sceneWidth = args.NewSize.Width;
            _sceneHeight = args.NewSize.Height;

            // Console.WriteLine($"{_sceneWidth}x{_sceneHeight}");

            Scaling = GetScalingFactor(_sceneWidth);

            // Console.WriteLine($"Scaling {Scaling}");

            foreach (var construct in Children.OfType<Construct>())
            {
                var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == construct.ConstructType);
                construct.SetSize(width: size.Width * Scaling, height: size.Height * Scaling);
            }
        }

        #endregion
    }
}
