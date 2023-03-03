using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HonkPooper
{
    public partial class Generator
    {
        #region Fields
        
        private int _generationDelay;
        private int _generationDelayInCount;

        #endregion

        #region Properties

        private Func<bool> GenerationAction { get; set; }

        #endregion

        public Generator(int generationDelay, Func<bool> generationAction)
        {
            _generationDelay = generationDelay;
            GenerationAction = generationAction;

            _generationDelayInCount = _generationDelay;
        }

        public void Generate()
        {
            _generationDelayInCount--;

            if (_generationDelayInCount <= 0)
            {
                GenerationAction();
                _generationDelayInCount = _generationDelay;
            }
        }

        public void SetGenerationDelay(int deplay)
        {
            _generationDelay = deplay;
        }
    }
}
