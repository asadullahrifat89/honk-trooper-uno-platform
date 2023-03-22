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

        private readonly AudioStub _audioStub;

        #endregion

        #region Ctor

        public Honk(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            _random = new Random();

            _honk_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.HONK).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.HONK);

            ConstructType = ConstructType.HONK;

            var width = size.Width;
            var height = size.Height;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var uri = ConstructExtensions.GetRandomContentUri(_honk_uris);

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(_content_image);

            _audioStub = new AudioStub((SoundType.CAR_HONK, 0.5, false));
        }

        #endregion

        #region Methods

        public void Reset()
        {
            _audioStub.Play(SoundType.CAR_HONK);
            Opacity = 1;
        }

        public void Reposition(Construct source)
        {
            var hitBox = source.GetCloseHitBox();

            SetPosition(
                left: hitBox.Left - source.Width / 3,
                top: hitBox.Top - (25),
                z: 5);
        }

        #endregion
    }
}
