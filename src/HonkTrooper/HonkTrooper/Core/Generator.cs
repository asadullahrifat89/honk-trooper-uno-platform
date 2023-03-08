using System;

namespace HonkTrooper
{
    public partial class Generator
    {
        #region Fields

        private bool _randomizeGenerationDelay = false;
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
            bool randomizeGenerationDelay = false)
        {
            _randomizeGenerationDelay = randomizeGenerationDelay;
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
                    _generationDelayInCount = _randomizeGenerationDelay ? _random.Next(_generationDelay / 2, _generationDelay) : _generationDelay;
                }
            }
        }

        public void SetGenerationDelay(int deplay)
        {
            _generationDelay = deplay;
        }
    }
}
