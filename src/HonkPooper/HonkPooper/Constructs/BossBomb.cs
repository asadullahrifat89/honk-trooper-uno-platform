using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;

namespace HonkPooper
{
    public partial class BossBomb : Construct
    {
        #region Fields

        private Random _random;

        private Uri[] _bomb_uris;
        private Uri[] _bomb_blast_uris;

        #endregion

        #region Ctor

        public BossBomb(
           Func<Construct, bool> animateAction,
           Func<Construct, bool> recycleAction,
           double downScaling)
        {
            _random = new Random();

            _bomb_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.BOSS_BOMB).Select(x => x.Uri).ToArray();
            _bomb_blast_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.BOMB_BLAST).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.BOSS_BOMB);

            ConstructType = ConstructType.BOSS_BOMB;

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var uri = _bomb_uris[_random.Next(0, _bomb_uris.Length)];

            var content = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(content);

            IsometricDisplacement = 0.5;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET + 2;
            DropShadowDistance = 60;
        }

        #endregion

        #region Properties

        public bool IsBlasting { get; set; }

        #endregion

        #region Methods

        public void Reposition(Boss boss, double downScaling)
        {
            SetPosition(
                left: (boss.GetLeft() + boss.Width / 2) - Width / 2,
                top: boss.GetBottom() - (40 * downScaling),
                z: 7);
        }

        public void Reset()
        {
            Opacity = 1;
            SetScaleTransform(1);

            IsBlasting = false;

            var uri = _bomb_uris[_random.Next(0, _bomb_uris.Length)];

            var content = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(content);
        }

        public void SetBlast()
        {
            var uri = _bomb_blast_uris[_random.Next(0, _bomb_blast_uris.Length)];

            var content = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(content);

            IsBlasting = true;
        }

        #endregion
    }

    public partial class PlayerBomb : Construct
    {
        #region Fields

        private Random _random;

        private Uri[] _bomb_uris;
        private Uri[] _bomb_blast_uris;

        #endregion

        #region Ctor

        public PlayerBomb(
           Func<Construct, bool> animateAction,
           Func<Construct, bool> recycleAction,
           double downScaling)
        {
            _random = new Random();

            _bomb_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.PLAYER_BOMB).Select(x => x.Uri).ToArray();
            _bomb_blast_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.BOMB_BLAST).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.PLAYER_BOMB);

            ConstructType = ConstructType.PLAYER_BOMB;

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var uri = _bomb_uris[_random.Next(0, _bomb_uris.Length)];

            var content = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(content);

            IsometricDisplacement = 0.5;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET + 2;
            DropShadowDistance = 60;
        }

        #endregion

        #region Properties

        public bool IsBlasting { get; set; }

        #endregion

        #region Methods

        public void Reposition(Player Player, double downScaling)
        {
            SetPosition(
                left: (Player.GetLeft() + Player.Width / 2) - Width / 2,
                top: Player.GetBottom() - (40 * downScaling),
                z: 7);
        }

        public void Reset()
        {
            Opacity = 1;
            SetScaleTransform(1);

            IsBlasting = false;

            var uri = _bomb_uris[_random.Next(0, _bomb_uris.Length)];

            var content = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(content);
        }

        public void SetBlast()
        {
            var uri = _bomb_blast_uris[_random.Next(0, _bomb_blast_uris.Length)];

            var content = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(content);

            IsBlasting = true;
        }

        #endregion
    }
}
