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

        private readonly Sound[] _cracker_drop_sounds;
        private readonly Sound[] _cracker_blast_sounds;

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

            var uri = _bomb_uris[_random.Next(0, _bomb_uris.Length)];

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(_content_image);

            IsometricDisplacement = 0.5;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE - 10;

            _cracker_drop_sounds = Constants.SOUND_TEMPLATES.Where(x => x.SoundType == SoundType.CRACKER_DROP).Select(x => x.Uri).Select(uri => new Sound(uri: uri, volume: 0.3)).ToArray();
            _cracker_blast_sounds = Constants.SOUND_TEMPLATES.Where(x => x.SoundType == SoundType.CRACKER_BLAST).Select(x => x.Uri).Select(uri => new Sound(uri: uri)).ToArray();
        }

        #endregion

        #region Properties

        public bool IsBlasting { get; set; }

        #endregion

        #region Methods

        public void Reposition(Player player, double downScaling)
        {
            SetPosition(
                left: (player.GetLeft() + player.Width / 2) - Width / 2,
                top: player.GetBottom() - (40 * downScaling),
                z: 7);
        }

        public void Reset()
        {
            var sound = _cracker_drop_sounds[_random.Next(0, _cracker_drop_sounds.Length)];
            sound.Play();

            Opacity = 1;
            SetScaleTransform(1);

            IsBlasting = false;

            var uri = _bomb_uris[_random.Next(0, _bomb_uris.Length)];
            _content_image.Source = new BitmapImage(uri);          
        }

        public void SetBlast()
        {
            var sound = _cracker_blast_sounds[_random.Next(0, _cracker_blast_sounds.Length)];
            sound.Play();

            var uri = _bomb_blast_uris[_random.Next(0, _bomb_blast_uris.Length)];
            _content_image.Source = new BitmapImage(uri);
            IsBlasting = true;

           
        }

        #endregion
    }
}
