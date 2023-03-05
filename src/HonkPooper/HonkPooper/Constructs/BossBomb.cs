using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Automation.Peers;
using Windows.Foundation;
using System.Diagnostics.Contracts;

namespace HonkPooper
{
    public partial class BossBomb : Construct
    {
        #region Fields

        private Random _random;

        private Uri[] _bomb_uris;
        private Uri[] _bomb_blast_uris;

        private Rect _origin = Rect.Empty;
        private Rect _target = Rect.Empty;

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
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
            DropShadowDistance = 50;
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

            _origin = boss.GetCloseHitBox();
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

            _target = Rect.Empty;
            _origin = Rect.Empty;
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

        public void SetTargetPoint(Rect rect)
        {
            _target = rect;
        }

        public void FollowTargetPoint(double speed)
        {
            if (_target != Rect.Empty && _origin != Rect.Empty)
            {
                SetLeft(GetLeft() + speed);
                SetTop(GetTop() + speed * IsometricDisplacement);
            }

        }

        #endregion
    }
}
