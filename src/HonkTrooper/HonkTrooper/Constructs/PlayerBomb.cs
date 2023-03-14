using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;

namespace HonkTrooper
{
    public partial class PlayerBomb : Construct
    {
        #region Fields

        private readonly Random _random;

        private readonly Uri[] _bomb_uris;
        private readonly Uri[] _bomb_blast_uris;

        private readonly Image _content_image;

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

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(_content_image);

            IsometricDisplacement = 0.5;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET + 2;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE + 10;
        }

        #endregion

        #region Properties

        public bool IsBlasting { get; set; }

        public bool AwaitMoveRight { get; set; }

        public bool AwaitMoveLeft { get; set; }

        public bool AwaitMoveUp { get; set; }

        public bool AwaitMoveDown { get; set; }

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
            _content_image.Source = new BitmapImage(uri);

            AwaitMoveLeft = false;
            AwaitMoveRight = false;

            AwaitMoveUp = false;
            AwaitMoveDown = false;
        }

        public void SetBlast()
        {
            var uri = _bomb_blast_uris[_random.Next(0, _bomb_blast_uris.Length)];
            _content_image.Source = new BitmapImage(uri);
            IsBlasting = true;
        }

        public void MoveLeft(double speed)
        {
            SetLeft(GetLeft() - speed);
            SetTop(GetTop() + speed);
        }

        public void MoveRight(double speed)
        {
            SetLeft(GetLeft() + speed);
            SetTop(GetTop() - speed);
        }

        public void MoveUp(double speed)
        {
            SetLeft(GetLeft() - speed);
            SetTop(GetTop() - speed * IsometricDisplacement);
        }

        public void MoveDown(double speed)
        {
            SetLeft(GetLeft() + speed);
            SetTop(GetTop() + speed * IsometricDisplacement);
        }

        #endregion
    }
}
