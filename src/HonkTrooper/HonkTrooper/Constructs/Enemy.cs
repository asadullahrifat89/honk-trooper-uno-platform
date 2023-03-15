using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class Enemy : Construct
    {
        #region Fields

        private readonly Random _random;
        private readonly Uri[] _enemy_uris;

        private double _hoverDelay;
        private readonly double _hoverDelayDefault = 15;
        private readonly double _hoverSpeed = 0.5;

        private double _attackDelay;
        private double _honkDelay;

        private readonly Image _content_image;

        #endregion

        #region Ctor

        public Enemy(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            ConstructType = ConstructType.ENEMY;

            _random = new Random();

            _enemy_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.ENEMY).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.ENEMY);

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var uri = _enemy_uris[_random.Next( _enemy_uris.Length)];

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(_content_image);

            IsometricDisplacement = 0.5;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE;
        }

        #endregion

        #region Properties

        public bool WillHonk { get; set; }

        public bool WillAttack { get; set; }

        public double Health { get; set; }

        public bool IsDead => Health <= 0;

        #endregion

        #region Methods

        public void Reset()
        {
            Opacity = 1;
            SetScaleTransform(1);

            Health = 5 * _random.Next(4);

            WillAttack = Convert.ToBoolean(_random.Next( 2));
            WillHonk = Convert.ToBoolean(_random.Next( 2));

            // role dice again
            if (!WillHonk && !WillAttack)
            {
                WillAttack = Convert.ToBoolean(_random.Next( 2));
                WillHonk = Convert.ToBoolean(_random.Next( 2));
            }

            if (WillHonk)
                SetHonkDelay();

            var uri = _enemy_uris[_random.Next( _enemy_uris.Length)];
            _content_image.Source = new BitmapImage(uri);

            SpeedOffset = _random.Next(-3, 2);
        }

        public void Hover()
        {
            if (Scene.IsSlowMotionActivated)
            {
                _hoverDelay -= 0.5;

                if (_hoverDelay > 0)
                {
                    SetTop(GetTop() + _hoverSpeed / Constants.DEFAULT_SLOW_MOTION_REDUCTION_FACTOR);
                }
                else
                {
                    SetTop(GetTop() - _hoverSpeed / Constants.DEFAULT_SLOW_MOTION_REDUCTION_FACTOR);

                    if (_hoverDelay <= _hoverDelayDefault * -1)
                        _hoverDelay = _hoverDelayDefault;
                }
            }

            else
            {
                _hoverDelay--;

                if (_hoverDelay > 0)
                {
                    SetTop(GetTop() + _hoverSpeed);
                }
                else
                {
                    SetTop(GetTop() - _hoverSpeed);

                    if (_hoverDelay <= _hoverDelayDefault * -1)
                        _hoverDelay = _hoverDelayDefault;
                }
            }
        }

        public bool Attack()
        {
            if (!IsDead && WillAttack)
            {
                if (Scene.IsSlowMotionActivated)
                    _attackDelay -= 0.5;
                else
                    _attackDelay--;

                if (_attackDelay < 0)
                {
                    SetAttackDelay();
                    return true;
                }
            }

            return false;
        }

        public void SetAttackDelay()
        {
            _attackDelay = _random.Next(50, 80);
        }

        public bool Honk()
        {
            if (!IsDead && WillHonk)
            {
                if (Scene.IsSlowMotionActivated)
                    _honkDelay -= 0.5;
                else
                    _honkDelay--;

                if (_honkDelay < 0)
                {
                    SetHonkDelay();
                    return true;
                }
            }

            return false;
        }

        public void SetHonkDelay()
        {
            _honkDelay = _random.Next(40, 80);
        }

        public void LooseHealth()
        {
            Health -= 5;
        }

        #endregion
    }
}
