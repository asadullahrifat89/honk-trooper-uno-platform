using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;

namespace HonkTrooper
{
    public partial class EnemyBomb : Construct
    {
        #region Fields

        private readonly Random _random;

        private readonly Uri[] _bomb_uris;
        private readonly Uri[] _bomb_blast_uris;

        private readonly Image _content_image;

        #endregion

        #region Ctor

        public EnemyBomb(
           Func<Construct, bool> animateAction,
           Func<Construct, bool> recycleAction,
           double downScaling)
        {
            _random = new Random();

            _bomb_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.ENEMY_BOMB).Select(x => x.Uri).ToArray();
            _bomb_blast_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.BOMB_BLAST).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.ENEMY_BOMB);

            ConstructType = ConstructType.ENEMY_BOMB;

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

        #endregion

        #region Methods

        public void Reposition(Enemy Enemy, double downScaling)
        {
            SetPosition(
                left: (Enemy.GetLeft() + Enemy.Width / 2) - Width / 2,
                top: Enemy.GetBottom() - (50 * downScaling),
                z: 7);
        }

        public void Reset()
        {
            Opacity = 1;
            SetScaleTransform(1);

            IsBlasting = false;

            var uri = _bomb_uris[_random.Next(0, _bomb_uris.Length)];
            _content_image.Source = new BitmapImage(uri);
        }

        public void SetBlast()
        {
            var uri = _bomb_blast_uris[_random.Next(0, _bomb_blast_uris.Length)];
            _content_image.Source = new BitmapImage(uri);

            IsBlasting = true;
        }

        public void MoveDown(double speed)
        {
            SetLeft(GetLeft() + speed);
            SetTop(GetTop() + speed * IsometricDisplacement);
        }

        #endregion
    }
}
