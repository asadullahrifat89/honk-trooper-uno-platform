using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;

namespace HonkPooper
{
    public partial class Honk : Construct
    {
        private Random _random;
        private Uri[] _honk_uris;

        public Honk(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double scaling)
        {
            _random = new Random();

            _honk_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.HONK).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.HONK);

            ConstructType = ConstructType.HONK;

            var width = size.Width * scaling;
            var height = size.Height * scaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var uri = _honk_uris[_random.Next(0, _honk_uris.Length)];

            var content = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(content);

            //Displacement = _random.NextDouble();
            Displacement = 0.6;
        }
    }
}
