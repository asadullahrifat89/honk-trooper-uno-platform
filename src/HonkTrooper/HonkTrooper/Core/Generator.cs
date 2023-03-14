using System;

namespace HonkTrooper
{
    public partial class Generator
    {
        #region Fields

        private readonly bool _randomizeGenerationDelay = false;
        private double _generationDelay;
        private double _generationDelayInCount;

        private readonly Random _random = new Random();

        #endregion

        #region Properties

        private Func<bool> GenerationAction { get; set; }

        public Scene Scene { get; set; }

        #endregion

        public Generator(
            int generationDelay,
            Func<bool> generationAction,
            Func<bool> startUpAction,
            bool randomizeGenerationDelay = false)
        {
            _randomizeGenerationDelay = randomizeGenerationDelay;
            _generationDelay = generationDelay;

            _generationDelayInCount = _generationDelay;

            GenerationAction = generationAction;
            startUpAction();
        }

        #region Methods

        public void Generate()
        {
            if (_generationDelay > 0)
            {
                _generationDelayInCount -= Scene.IsSlowMotionActivated ? 1 / Constants.DEFAULT_SLOW_MOTION_REDUCTION_FACTOR : 1;

                if (_generationDelayInCount <= 0)
                {
                    GenerationAction();
                    _generationDelayInCount = _randomizeGenerationDelay ? _random.Next((int)(_generationDelay / 2), (int)_generationDelay) : _generationDelay;
                }
            }
        }

        public void SetGenerationDelay(int deplay)
        {
            _generationDelay = deplay;
        }

        #endregion
    }
}
