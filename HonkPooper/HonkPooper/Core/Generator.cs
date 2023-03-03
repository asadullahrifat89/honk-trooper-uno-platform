using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HonkPooper
{
    public partial class Generator
    {
        private int _generationDelay;
        private int _generationDelayInCount;
        private Func<bool> _generationAction;

        public Generator(int generationDelay, Func<bool> generationAction)
        {
            _generationDelay = generationDelay;
            _generationAction = generationAction;

            _generationDelayInCount = _generationDelay;
        }

        public void Generate()
        {
            _generationDelayInCount--;

            if (_generationDelayInCount <= 0)
            {
                _generationAction();
                _generationDelayInCount = _generationDelay;
            }
        }
    }
}
