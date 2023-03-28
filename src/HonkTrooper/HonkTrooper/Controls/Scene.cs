using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.Foundation;
using Microsoft.UI.Xaml.Media.Animation;
using System.Diagnostics;

namespace HonkTrooper
{
    public partial class Scene : Canvas
    {
        #region Fields

        private readonly Storyboard _storyboard;
        private readonly DoubleAnimation _doubleAnimation;

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

        //private readonly List<Construct> _destroyables = new();
        private readonly List<Generator> _generators = new();

        private double _slowMotionDelay;
        private readonly double _slowMotionDelayDefault = 160;

#if DEBUG
        private Stopwatch _stopwatch;
        private TimeSpan _lastTicks;
        private int _famesCount;
#endif
        #endregion

        #region Ctor

        public Scene()
        {
            RenderTransformOrigin = new Point(0, 0);
            RenderTransform = _compositeTransform;
            CanDrag = false;
            Speed = Constants.DEFAULT_SCENE_SPEED;

            _doubleAnimation = new DoubleAnimation()
            {
                Duration = new Duration(TimeSpan.FromSeconds(7)),
                From = 0,
                To = 1,
            };

            Storyboard.SetTarget(_doubleAnimation, this);
            Storyboard.SetTargetProperty(_doubleAnimation, "Opacity");

            _storyboard = new Storyboard();
            _storyboard.Children.Add(_doubleAnimation);

            Loaded += Scene_Loaded;
            Unloaded += Scene_Unloaded;
        }

        #endregion

        #region Properties

        public bool IsAnimating { get; set; }

        public bool IsSlowMotionActivated => _slowMotionDelay > 0;

        public SceneState SceneState { get; set; } = SceneState.GAME_STOPPED;

        public double Speed { get; set; }

        #endregion

        #region Events

        private void Scene_Loaded(object sender, RoutedEventArgs e)
        {
            _storyboard.Begin();
        }

        private void Scene_Unloaded(object sender, RoutedEventArgs e)
        {
            Stop();
        }

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

        //public void DisposeFromScene(Construct construct)
        //{
        //    _destroyables.Add(construct);
        //}

        public async void Play()
        {
            if (!IsAnimating)
            {
                IsAnimating = true;
#if DEBUG
                _stopwatch = Stopwatch.StartNew();
                _famesCount = 0;
#endif
                _lastTicks = TimeSpan.FromSeconds(0);

                _gameViewTimer = new PeriodicTimer(_frameTime);

                while (await _gameViewTimer.WaitForNextTickAsync())
                {
                    Run();
                }
            }
        }

        public void Pause()
        {
            if (IsAnimating)
            {
                IsAnimating = false;
#if DEBUG
                _stopwatch?.Reset();
#endif
                _gameViewTimer?.Dispose();
            }
        }

        public void SetState(SceneState sceneState)
        {
            SceneState = sceneState;
        }

        public void Stop()
        {
            IsAnimating = false;
#if DEBUG
            _stopwatch?.Stop();
#endif
            _gameViewTimer?.Dispose();

            Clear();
        }

        public void Clear()
        {
            Children.Clear();

            _generators.Clear();
            //_destroyables.Clear();

#if DEBUG
            _stopwatch?.Stop();
#endif
            _gameViewTimer?.Dispose();
        }

        private void Run()
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
            //foreach (Construct destroyable in _destroyables)
            //{
            //    Children.Remove(destroyable);
            //}

            //_destroyables.Clear();

            DepleteSlowMotion();

#if DEBUG
            _famesCount++;
            if (_stopwatch.Elapsed - _lastTicks > TimeSpan.FromSeconds(2))
            {
                var fps = _famesCount / 2;
                _famesCount = 0;
                _lastTicks = _stopwatch.Elapsed;
                LoggerExtensions.Log($"Scene: {Name} ~ Animating Objects: {Children.OfType<Construct>().Count(x => x.IsAnimating)} ~ Total Objects: {Children.OfType<Construct>().Count()} ~ FPS: {fps}");
            }
#endif
        }

        public void ActivateSlowMotion()
        {
            if (!IsSlowMotionActivated)
            {
                _slowMotionDelay = _slowMotionDelayDefault;
            }
        }

        private void DepleteSlowMotion()
        {
            if (_slowMotionDelay > 0)
            {
                _slowMotionDelay--;
            }
        }

        #endregion
    }

    public enum SceneState
    {
        GAME_STOPPED, GAME_RUNNING,
    }
}
