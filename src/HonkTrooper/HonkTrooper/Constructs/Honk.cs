using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;

namespace HonkTrooper
{
    public partial class Honk : Construct
    {
        #region Fields

        private readonly Random _random;
        private readonly Uri[] _honk_uris;

        private readonly Image _content_image;

        private readonly Sound[] _car_honk_sounds;

        #endregion

        #region Ctor

        public Honk(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            _random = new Random();

            _honk_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.HONK).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.HONK);

            ConstructType = ConstructType.HONK;

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var uri = _honk_uris[_random.Next(0, _honk_uris.Length)];

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(_content_image);

            IsometricDisplacement = 0.6;

            _car_honk_sounds = Constants.SOUND_TEMPLATES.Where(x => x.SoundType == SoundType.CAR_HONK).Select(x => x.Uri).Select(uri => new Sound(uri: uri, volume: 0.7)).ToArray();
        }

        #endregion

        #region Methods

        public void Reset()
        {
            var sound = _car_honk_sounds[_random.Next(0, _car_honk_sounds.Length)];
            sound.Play();

            Opacity = 1;
        }

        #endregion
    }
}
