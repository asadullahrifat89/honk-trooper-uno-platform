using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class PlayerRocketSeeking : BombSeeking
    {
        #region Fields

        private readonly Random _random;
        private readonly Uri[] _bomb_uris;
        private readonly Uri[] _bomb_blast_uris;

        private readonly Image _content_image;

        #endregion

        #region Ctor

        public PlayerRocketSeeking(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            _random = new Random();

            _bomb_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.PLAYER_ROCKET_SEEKING).Select(x => x.Uri).ToArray();
            _bomb_blast_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.BOMB_BLAST).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.PLAYER_ROCKET_SEEKING);

            ConstructType = ConstructType.PLAYER_ROCKET_SEEKING;

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

            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET - 4;
            IsometricDisplacement = 0.5;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE;
        }

        #endregion

        #region Properties

        public bool IsBlasting { get; set; }

        public double TimeLeftUntilBlast { get; set; }

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
            Opacity = 1;
            SetScaleTransform(1);

            var uri = _bomb_uris[_random.Next(0, _bomb_uris.Length)];
            _content_image.Source = new BitmapImage(uri);

            IsBlasting = false;
            TimeLeftUntilBlast = 25;
        }

        public bool RunOutOfTimeToBlast()
        {
            TimeLeftUntilBlast -= 0.1;

            if (TimeLeftUntilBlast <= 0)
                return true;

            return false;
        }

        public void SetBlast()
        {
            var uri = _bomb_blast_uris[_random.Next(0, _bomb_blast_uris.Length)];
            _content_image.Source = new BitmapImage(uri);
            IsBlasting = true;
        }

        #endregion
    }
}
