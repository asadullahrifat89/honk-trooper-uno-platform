using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.WinUI.UI.Controls;

namespace HonkTrooper
{
    public partial class Honk : Construct
    {
        #region Fields

        private readonly Uri[] _honk_uris;

        private readonly ImageEx _content_image;
        private readonly BitmapImage _bitmapImage;

        private readonly AudioStub _audioStub;

        #endregion

        #region Ctor

        public Honk(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.HONK;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            _honk_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.HONK).Select(x => x.Uri).ToArray();

            SetConstructSize();

            var uri = ConstructExtensions.GetRandomContentUri(_honk_uris);
            _bitmapImage = new BitmapImage(uriSource: uri);

            _content_image = new ()
            {
                Source = _bitmapImage,
                Height = this.Height,
                Width = this.Width,
            };

            SetChild(_content_image);

            _audioStub = new AudioStub((SoundType.HONK, 0.5, false));
        }

        #endregion

        #region Methods

        public void Reset()
        {
            _audioStub.Play(SoundType.HONK);
            Opacity = 1;
        }

        public void Reposition(Construct source)
        {
            var hitBox = source.GetCloseHitBox();

            SetPosition(
                left: hitBox.Left - source.Width / 3,
                top: hitBox.Top - (25));
        }

        #endregion
    }
}
