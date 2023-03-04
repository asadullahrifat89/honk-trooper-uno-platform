using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Linq;

namespace HonkPooper
{
    public partial class Player : Construct
    {
        #region Fields

        private Random _random;
        private Uri[] _player_uris;

        #endregion

        public Player(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double scaling)
        {
            _random = new Random();

            _player_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.PLAYER).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.PLAYER);

            ConstructType = ConstructType.HONK;

            var width = size.Width * scaling;
            var height = size.Height * scaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var uri = _player_uris[_random.Next(0, _player_uris.Length)];

            var content = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(content);

            IsometricDisplacement = 1.5;
        }
    }
}
