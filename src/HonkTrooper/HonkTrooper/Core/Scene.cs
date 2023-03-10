using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace HonkTrooper
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

        private double _lastSpeed;
        private double _slowMotionDelay;


        #endregion

        #region Ctor

        public Scene()
        {
            Loaded += Scene_Loaded;
            Unloaded += Scene_Unloaded;
        }

        #endregion

        #region Properties

        public bool IsAnimating { get; set; }

        public double DownScaling { get; set; }

        public double Speed { get; set; }

        public bool IsSlowMotionActivated => _slowMotionDelay > 0;

        #endregion

        #region Methods

        /// <summary>
        /// Adds a construct to the scene.
        /// </summary>
        /// <param name="construct"></param>
        public void AddToScene(Construct construct)
        {
            construct.Scene = this;
            Children.Add(construct);
        }

        /// <summary>
        /// Adds a generator to the scene.
        /// </summary>
        /// <param name="generator"></param>
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
        public async void Play()
        {
            IsAnimating = true;
            _stopwatch = Stopwatch.StartNew();
            _gameViewTimer = new PeriodicTimer(_frameTime);

            while (await _gameViewTimer.WaitForNextTickAsync())
                Animate();
        }

        /// <summary>
        /// Pauses the timer for the scene.
        /// </summary>
        public void Pause()
        {
            IsAnimating = false;
            _stopwatch?.Stop();
            _gameViewTimer?.Dispose();
        }

        /// <summary>
        /// Stops the timer of the scene and clears all constructs from scene.
        /// </summary>
        public void Stop() 
        {
            IsAnimating = false;
            _stopwatch?.Stop();
            _gameViewTimer?.Dispose();

            Clear();
        }

        public void Clear()
        {
            this.Children.Clear();
            _generators.Clear();
            _destroyables.Clear();
            _gameViewTimer?.Dispose();
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

            DepleteSlowMotion();

            // Console.WriteLine($"Animating Objects: {Children.OfType<Construct>().Count(x => x.IsAnimating)} ~ Total Objects: {Children.OfType<Construct>().Count()}");            
        }

        public void ActivateSlowMotion()
        {
            _lastSpeed = Speed;
            Speed /= 3.5;

            _slowMotionDelay = 180;
        }

        private void DepleteSlowMotion()
        {
            if (_slowMotionDelay > 0)
            {
                _slowMotionDelay--;

                if (_slowMotionDelay <= 0)
                    Speed = _lastSpeed;
            }
        }

        #endregion

        #region Events

        private void Scene_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= Scene_SizeChanged;
            Pause();
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

            DownScaling = ScreenExtensions.GetDownScaling(_sceneWidth);

            // Console.WriteLine($"Down Scaling {Scaling}");

            foreach (var construct in Children.OfType<Construct>())
            {
                if (Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == construct.ConstructType) is (ConstructType ConstructType, double Height, double Width) size)
                {
                    construct.SetSize(
                        width: size.Width * DownScaling,
                        height: size.Height * DownScaling);

                }
            }
        }

        #endregion
    }
}
