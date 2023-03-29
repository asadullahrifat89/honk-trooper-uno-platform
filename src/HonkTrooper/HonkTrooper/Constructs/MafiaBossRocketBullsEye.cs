using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;
using Windows.Foundation;

namespace HonkTrooper
{
    public partial class MafiaBossRocketBullsEye : RocketSeeking
    {
        #region Fields

        private readonly Uri[] _bomb_uris;
        private readonly Uri[] _bomb_blast_uris;

        private readonly Image _content_image;

        private double _autoBlastDelay;
        private readonly double _autoBlastDelayDefault = 15;

        private readonly AudioStub _audioStub;

        private Rect _targetHitbox;

        #endregion

        #region Ctor

        public MafiaBossRocketBullsEye(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            _bomb_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.MAFIA_BOSS_ROCKET_BULLS_EYE).Select(x => x.Uri).ToArray();
            _bomb_blast_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.BLAST).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.MAFIA_BOSS_ROCKET_BULLS_EYE);

            ConstructType = ConstructType.MAFIA_BOSS_ROCKET_BULLS_EYE;

            var width = size.Width;
            var height = size.Height;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var uri = ConstructExtensions.GetRandomContentUri(_bomb_uris);

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(_content_image);

            BorderThickness = new Microsoft.UI.Xaml.Thickness(Constants.DEFAULT_BLAST_RING_BORDER_THICKNESS);
            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(Constants.DEFAULT_BLAST_RING_CORNER_RADIUS);

            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE;

            _audioStub = new AudioStub((SoundType.SEEKER_ROCKET_LAUNCH, 0.7, false), (SoundType.ROCKET_BLAST, 1, false));
        }

        #endregion

        #region Properties

        public bool IsBlasting { get; set; }

        #endregion

        #region Methods

        public void Reset()
        {
            _audioStub.Play(SoundType.SEEKER_ROCKET_LAUNCH);

            Opacity = 1;

            var uri = ConstructExtensions.GetRandomContentUri(_bomb_uris);
            _content_image.Source = new BitmapImage(uri);

            BorderBrush = new SolidColorBrush(Colors.Transparent);

            SetScaleTransform(1);
            SetRotation(0);

            IsBlasting = false;

            _targetHitbox = new Rect(0, 0, 0, 0);
            _autoBlastDelay = _autoBlastDelayDefault;
        }

        public void Reposition(PlayerBalloon player)
        {
            SetPosition(
                left: (player.GetLeft() + player.Width / 2) - Width / 2,
                top: player.GetBottom() - (40));
        }

        public void SetTarget(Rect target)
        {
            double left = GetLeft();
            double top = GetTop();

            double playerMiddleX = left + Width / 2;
            double playerMiddleY = top + Height / 2;

            var scaling = ScreenExtensions.GetScreenSpaceScaling();

            // move up
            if (target.Y < playerMiddleY)
            {
                var distance = Math.Abs(target.Y - playerMiddleY);
                _targetHitbox.Y = target.Y - distance;

                if (_targetHitbox.Y > 0)
                    _targetHitbox.Y -= distance;
            }

            // move left
            if (target.X < playerMiddleX)
            {
                var distance = Math.Abs(target.X - playerMiddleX);
                _targetHitbox.X = target.X - distance;

                if (_targetHitbox.X > 0)
                    _targetHitbox.X -= distance;
            }

            // move down
            if (target.Y > playerMiddleY)
            {
                var distance = Math.Abs(target.Y - playerMiddleY);
                _targetHitbox.Y = target.Y + distance;

                if (_targetHitbox.Y < Constants.DEFAULT_SCENE_HEIGHT * scaling)
                    _targetHitbox.Y += distance;

            }

            // move right
            if (target.X > playerMiddleX)
            {
                var distance = Math.Abs(target.X - playerMiddleX);
                _targetHitbox.X = target.X + distance;

                if (_targetHitbox.X < Constants.DEFAULT_SCENE_WIDTH * scaling)
                    _targetHitbox.X += distance;
            }
        }

        public void Move()
        {
            Seek(target: _targetHitbox); // seek at normal speed so that the player can evade
        }

        public bool AutoBlast()
        {
            if (_targetHitbox.IntersectsWith(this.GetCloseHitBox())) // if target reached
                return true;

            _autoBlastDelay -= 0.1;

            if (_autoBlastDelay <= 0)
                return true;

            return false;
        }

        public void SetBlast()
        {
            _audioStub.Play(SoundType.ROCKET_BLAST);

            SetRotation(0);
            SetScaleTransform(Constants.DEFAULT_BLAST_SHRINK_SCALE);

            BorderBrush = new SolidColorBrush(Colors.Crimson);

            var uri = ConstructExtensions.GetRandomContentUri(_bomb_blast_uris);
            _content_image.Source = new BitmapImage(uri);
            IsBlasting = true;
        }

        #endregion
    }
}
