using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.Foundation;

namespace HonkPooper
{
    public partial class Scene : Canvas
    {
        #region Fields

        private double _windowWidth, _windowHeight;

        private PeriodicTimer _gameViewTimer;
        private readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(Constants.DEFAULT_FRAME_TIME);

        private readonly List<Construct> _destroyables = new();

        #endregion

        #region Ctor

        public Scene()
        {
            Speed = 1.5;

            Loaded += Scene_Loaded;
            Unloaded += Scene_Unloaded;
        }

        #endregion

        #region Properties

        public double Speed { get; set; }

        public double Scale { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a construct to the scene
        /// </summary>
        /// <param name="construct"></param>
        public void AddToScene(Construct construct)
        {
            construct.Scene = this;
            Children.Add(construct);
        }

        /// <summary>
        /// Starts the timer for the scene and starts the scene loop.
        /// </summary>
        public async void Animate()
        {
            _gameViewTimer = new PeriodicTimer(_frameTime);

            while (await _gameViewTimer.WaitForNextTickAsync())
                SceneLoop();
        }

        /// <summary>
        /// Stops the timer for the scene.
        /// </summary>
        public void Stop()
        {
            _gameViewTimer?.Dispose();
        }

        /// <summary>
        /// Gets the scaling factor according to window size.
        /// </summary>
        /// <param name="windowWidth"></param>
        /// <returns></returns>
        private double GetGameObjectScale(double windowWidth)
        {
            return windowWidth switch
            {
                <= 300 => 0.60,
                <= 400 => 0.65,
                <= 500 => 0.70,
                <= 700 => 0.75,
                <= 900 => 0.80,
                <= 1000 => 0.85,
                <= 1400 => 0.90,
                <= 2000 => 0.95,
                _ => 1,
            };
        }

        /// <summary>
        /// Executes actions of the constructs.
        /// </summary>
        private void SceneLoop()
        {
            // run action for each construct and add to destroyable if destroyable function returns true
            foreach (Construct construct in Children.OfType<Construct>())
            {
                construct.Animate();

                if (CheckDestructionRule(construct))
                {
                    switch (construct.DestructionImpact)
                    {
                        case DestructionImpact.Remove:
                            _destroyables.Add(construct);
                            break;
                        case DestructionImpact.Recycle:
                            construct.Recycle();
                            break;
                        default:
                            break;
                    }
                }
            }

            // remove the destroyables from the scene
            foreach (Construct destroyable in _destroyables)
            {
                Children.Remove(destroyable);
            }
        }

        private bool CheckDestructionRule(Construct construct)
        {
            switch (construct.DestructionRule)
            {
                case DestructionRule.ExitsRightBorder:
                    {
                        if (construct.GetLeft() > _windowWidth)
                            return true;
                    }
                    break;
                case DestructionRule.ExitsLeftBorder:
                    {
                        if (construct.GetRight() < 0)
                            return true;
                    }
                    break;
                case DestructionRule.ExitsTopBorder:
                    {
                        if (construct.GetBottom() < 0)
                            return true;
                    }
                    break;
                case DestructionRule.ExitsBottomBorder:
                    {
                        if (construct.GetTop() > _windowHeight)
                            return true;
                    }
                    break;
            }

            return false;
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
            _windowWidth = args.NewSize.Width;
            _windowHeight = args.NewSize.Height;

            Console.WriteLine($"{_windowWidth}x{_windowHeight}");

            Scale = GetGameObjectScale(_windowWidth);

            foreach (var construct in Children.OfType<Construct>())
            {
                var size = Constants.ELEMENT_SIZES.FirstOrDefault(x => x.ConstructType == (ConstructType)construct.Tag);
                construct.SetSize(size.Width * Scale, size.Height * Scale);
            }
        }

        #endregion
    }
}
