using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;

namespace HonkTrooper
{
    public partial class Boss : Construct
    {
        #region Fields

        private Random _random;
        private Uri[] _boss_uris;

        private int _hoverDelay;
        private readonly int _hoverDelayDefault = 15;

        #endregion

        #region Ctor

        public Boss(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            _hoverDelay = _hoverDelayDefault;

            _random = new Random();

            _boss_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.BOSS).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.BOSS);

            ConstructType = ConstructType.BOSS;

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var uri = _boss_uris[_random.Next(0, _boss_uris.Length)];

            var content = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(content);

            IsometricDisplacement = 0.5;
            DropShadowDistance = 50;
            SpeedOffset = 1;
        }

        #endregion

        #region Properties

        public bool IsAttacking { get; set; }

        public bool AwaitMoveRight { get; set; }

        public bool AwaitMoveLeft { get; set; }

        public bool AwaitMoveUp { get; set; }

        public bool AwaitMoveDown { get; set; }

        public double Health { get; set; }

        #endregion

        #region Methods

        public void Reset()
        {
            Opacity = 1;
            Health = 100;
            IsAttacking = false;

            AwaitMoveLeft = false;
            AwaitMoveRight = false;

            AwaitMoveUp = false;
            AwaitMoveDown = false;

            SetScaleTransform(1);
        }

        public void Hover()
        {
            _hoverDelay--;

            if (_hoverDelay > 0)
            {
                SetTop(GetTop() + 0.4);
            }
            else
            {
                SetTop(GetTop() - 0.4);

                if (_hoverDelay <= _hoverDelayDefault * -1)
                    _hoverDelay = _hoverDelayDefault;
            }
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
