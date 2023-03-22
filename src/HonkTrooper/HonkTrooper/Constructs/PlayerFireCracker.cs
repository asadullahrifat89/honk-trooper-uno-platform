using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;

namespace HonkTrooper
{
    public partial class PlayerFireCracker : Construct
    {
        #region Fields

        private readonly Random _random;

        private readonly Uri[] _bomb_uris;
        private readonly Uri[] _bomb_blast_uris;

        private readonly Image _content_image;

        private readonly AudioStub _audioStub;

        #endregion

        #region Ctor

        public PlayerFireCracker(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            _random = new Random();

            _bomb_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.PLAYER_FIRE_CRACKER).Select(x => x.Uri).ToArray();
            _bomb_blast_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.BOMB_BLAST).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.PLAYER_FIRE_CRACKER);

            ConstructType = ConstructType.PLAYER_FIRE_CRACKER;

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var uri = ConstructExtensions.GetRandomContentUri(_bomb_uris);

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(_content_image);

            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET / 1.5;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE - 15;

            _audioStub = new AudioStub((SoundType.CRACKER_DROP, 0.3, false), (SoundType.CRACKER_BLAST, 1, false));
        }

        #endregion

        #region Properties

        public bool IsBlasting { get; set; }

        #endregion

        #region Methods

        public void Reposition(PlayerBalloon player, double downScaling)
        {
            SetPosition(
                left: (player.GetLeft() + player.Width / 2) - Width / 2,
                top: player.GetBottom() - (35 * downScaling),
                z: 7);
        }

        public void Reset()
        {
            _audioStub.Play(SoundType.CRACKER_DROP);

            Opacity = 1;
            SetScaleTransform(1);

            IsBlasting = false;

            var uri = ConstructExtensions.GetRandomContentUri(_bomb_uris);
            _content_image.Source = new BitmapImage(uri);
        }

        public void SetBlast()
        {
            _audioStub.Play(SoundType.CRACKER_BLAST);

            var uri = ConstructExtensions.GetRandomContentUri(_bomb_blast_uris);
            _content_image.Source = new BitmapImage(uri);
            IsBlasting = true;
        }

        #endregion
    }
}
