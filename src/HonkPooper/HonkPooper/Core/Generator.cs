using System;

namespace HonkPooper
{
    public partial class Generator
    {
        #region Fields

        private bool _randomizeDelay = false;
        private int _generationDelay;
        private int _generationDelayInCount;

        private Random _random = new Random();

        #endregion

        #region Properties

        private Func<bool> GenerationAction { get; set; }

        #endregion

        public Generator(
            int generationDelay,
            Func<bool> generationAction,
            Func<bool> startUpAction,
            bool randomizeDelay = false)
        {
            _randomizeDelay = randomizeDelay;
            _generationDelay = generationDelay;

            _generationDelayInCount = _generationDelay;

            GenerationAction = generationAction;
            startUpAction();
        }

        public void Generate()
        {
            if (_generationDelay > 0)
            {
                _generationDelayInCount--;

                if (_generationDelayInCount <= 0)
                {
                    GenerationAction();
                    _generationDelayInCount = _randomizeDelay ? _random.Next((int)_generationDelay / 2, _generationDelay) : _generationDelay;
                }
            }
        }

        public void SetGenerationDelay(int deplay)
        {
            _generationDelay = deplay;
        }
    }
}
