using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;

namespace HonkTrooper
{
    public partial class BossRocket : Construct
    {
        #region Fields

        private readonly Random _random;

        private readonly Uri[] _bomb_uris;
        private readonly Uri[] _bomb_blast_uris;

        private readonly Image _content_image;

        private readonly Sound[] _rocket_launch_sounds;
        private readonly Sound[] _rocket_blast_sounds;

        private double _autoBlastDelay;
        private readonly double _autoBlastDelayDefault = 9;

        #endregion

        #region Ctor

        public BossRocket(
           Func<Construct, bool> animateAction,
           Func<Construct, bool> recycleAction,
           double downScaling)
        {
            _random = new Random();

            _bomb_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.BOSS_ROCKET).Select(x => x.Uri).ToArray();
            _bomb_blast_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.BOMB_BLAST).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.BOSS_ROCKET);

            ConstructType = ConstructType.BOSS_ROCKET;

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
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET * 2.5;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE + 10;

            _rocket_launch_sounds = Constants.SOUND_TEMPLATES.Where(x => x.SoundType == SoundType.ROCKET_LAUNCH).Select(x => x.Uri).Select(uri => new Sound(uri: uri, volume: 0.1)).ToArray();
            _rocket_blast_sounds = Constants.SOUND_TEMPLATES.Where(x => x.SoundType == SoundType.ROCKET_BLAST).Select(x => x.Uri).Select(uri => new Sound(uri: uri)).ToArray();
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

        public void Reposition(Boss boss, double downScaling)
        {
            SetPosition(
                left: (boss.GetLeft() + boss.Width / 2) - Width / 2,
                top: boss.GetBottom() - (50 * downScaling),
                z: 7);
        }

        public void Reset()
        {
            var sound = _rocket_launch_sounds[_random.Next(0, _rocket_launch_sounds.Length)];
            sound.Play();

            Opacity = 1;
            SetScaleTransform(1);

            IsBlasting = false;

            var uri = _bomb_uris[_random.Next(0, _bomb_uris.Length)];
            _content_image.Source = new BitmapImage(uri);

            AwaitMoveLeft = false;
            AwaitMoveRight = false;

            AwaitMoveUp = false;
            AwaitMoveDown = false;

            _autoBlastDelay = _autoBlastDelayDefault;
        }

        public void SetBlast()
        {
            var sound = _rocket_blast_sounds[_random.Next(0, _rocket_blast_sounds.Length)];
            sound.Play();

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
