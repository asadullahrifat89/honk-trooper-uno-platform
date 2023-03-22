using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.Foundation;

namespace HonkTrooper
{
    public partial class Scene : Canvas
    {
        #region Fields

        private readonly CompositeTransform _compositeTransform = new()
        {
            CenterX = 0.5,
            CenterY = 0.5,
            Rotation = 0,
            ScaleX = 1,
            ScaleY = 1,
        };

        private PeriodicTimer _gameViewTimer;
        private readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(Constants.DEFAULT_FRAME_TIME);

        private readonly List<Construct> _destroyables = new();
        private readonly List<Generator> _generators = new();

        private double _lastSpeed;
        private double _slowMotionDelay;
        private readonly double _slowMotionDelayDefault = 160;

        #endregion

        #region Ctor

        public Scene()
        {
            RenderTransformOrigin = new Point(0, 0);
            RenderTransform = _compositeTransform;
            CanDrag = false;

            Loaded += Scene_Loaded;
            Unloaded += Scene_Unloaded;
        }

        #endregion

        #region Properties

        public bool IsAnimating { get; set; }

        public double Speed { get; set; }

        public bool IsSlowMotionActivated => _slowMotionDelay > 0;

        public SceneState SceneState { get; set; } = SceneState.GAME_STOPPED;

        #endregion

        #region Methods

        public void SetRenderTransformOrigin(double xy)
        {
            RenderTransformOrigin = new Point(xy, xy);
        }

        public void SetScaleTransform(double scaleXY)
        {
            _compositeTransform.ScaleX = scaleXY;
            _compositeTransform.ScaleY = scaleXY;
        }

        /// <summary>
        /// Adds constructs to the scene.
        /// </summary>
        /// <param name="constructs"></param>
        public void AddToScene(params Construct[] constructs)
        {
            if (constructs is not null)
            {
                foreach (var construct in constructs)
                {
                    construct.Scene = this;
                    Children.Add(construct);
                }
            }
        }

        /// <summary>
        /// Adds generators to the scene.
        /// </summary>
        /// <param name="generators"></param>
        public void AddToScene(params Generator[] generators)
        {
            if (generators is not null)
            {
                foreach (var generator in generators)
                {
                    generator.Scene = this;
                    _generators.Add(generator);
                }
            }
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

            //_stopwatch = Stopwatch.StartNew();
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
            //_stopwatch?.Stop();
            _gameViewTimer?.Dispose();
        }

        public void SetState(SceneState sceneState)
        {
            SceneState = sceneState;
        }

        /// <summary>
        /// Stops the timer of the scene and clears all constructs from scene.
        /// </summary>
        public void Stop()
        {
            IsAnimating = false;
            //_stopwatch?.Stop();
            _gameViewTimer?.Dispose();

            Clear();
        }

        public void Clear()
        {
            Children.Clear();

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

            LoggerExtensions.Log($"Animating Objects: {Children.OfType<Construct>().Count(x => x.IsAnimating)} ~ Total Objects: {Children.OfType<Construct>().Count()}");
        }

        public void ActivateSlowMotion()
        {
            if (!IsSlowMotionActivated)
            {
                _lastSpeed = Speed;
                Speed /= Constants.DEFAULT_SLOW_MOTION_REDUCTION_FACTOR;

                _slowMotionDelay = _slowMotionDelayDefault;
            }
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
            Stop();
        }

        private void Scene_Loaded(object sender, RoutedEventArgs e)
        {

        }

        #endregion
    }

    public enum SceneState
    {
        GAME_STOPPED, GAME_RUNNING,
    }
}
