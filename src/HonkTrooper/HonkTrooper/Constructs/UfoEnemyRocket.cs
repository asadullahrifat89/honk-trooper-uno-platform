using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using CommunityToolkit.WinUI.UI.Controls;

namespace HonkTrooper
{
    public partial class UfoEnemyRocket : AnimableConstruct
    {
        #region Fields

        private readonly Uri[] _bomb_uris;
        private readonly Uri[] _bomb_blast_uris;

        private readonly ImageEx _content_image;
        private readonly BitmapImage _bitmapImage;

        private double _autoBlastDelay;
        private readonly double _autoBlastDelayDefault = 12;

        private readonly AudioStub _audioStub;

        #endregion

        #region Ctor

        public UfoEnemyRocket(
           Action<Construct> animateAction,
           Action<Construct> recycleAction)
        {
            ConstructType = ConstructType.UFO_ENEMY_ROCKET;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            _bomb_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.UFO_ENEMY_ROCKET).Select(x => x.Uri).ToArray();
            _bomb_blast_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.BLAST).Select(x => x.Uri).ToArray();

            SetConstructSize();

            var uri = ConstructExtensions.GetRandomContentUri(_bomb_uris);
            _bitmapImage = new BitmapImage(uriSource: uri);

            _content_image = new()
            {
                Source = _bitmapImage,
                Height = this.Height,
                Width = this.Width,
                IsCacheEnabled = true,
            };

            SetChild(_content_image);

            BorderThickness = new Microsoft.UI.Xaml.Thickness(Constants.DEFAULT_BLAST_RING_BORDER_THICKNESS);
            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(Constants.DEFAULT_BLAST_RING_CORNER_RADIUS);

            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE + 10;

            _audioStub = new AudioStub((SoundType.ORB_LAUNCH, 0.5, false), (SoundType.ROCKET_BLAST, 1, false));

        }

        #endregion

        #region Methods

        public void Reset()
        {
            _audioStub.Play(SoundType.ORB_LAUNCH);

            Opacity = 1;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET + 2;
            SetScaleTransform(1);

            BorderBrush = new SolidColorBrush(Colors.Transparent);

            IsBlasting = false;

            var uri = ConstructExtensions.GetRandomContentUri(_bomb_uris);
            _bitmapImage.UriSource = uri;

            _autoBlastDelay = _autoBlastDelayDefault;
        }

        public void Reposition(UfoEnemy ufoEnemy)
        {
            SetPosition(
                left: (ufoEnemy.GetLeft() + ufoEnemy.Width / 2) - Width / 2,
                top: ufoEnemy.GetBottom() - (50));
        }

        public void SetBlast()
        {
            _audioStub.Play(SoundType.ROCKET_BLAST);
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET - 1;

            SetScaleTransform(Constants.DEFAULT_BLAST_SHRINK_SCALE);

            BorderBrush = new SolidColorBrush(Colors.Goldenrod);

            var uri = ConstructExtensions.GetRandomContentUri(_bomb_blast_uris);
            _bitmapImage.UriSource = uri;

            IsBlasting = true;
        }

        public bool AutoBlast()
        {
            _autoBlastDelay -= 0.1;

            if (_autoBlastDelay <= 0)
                return true;

            return false;
        }

        #endregion
    }
}
