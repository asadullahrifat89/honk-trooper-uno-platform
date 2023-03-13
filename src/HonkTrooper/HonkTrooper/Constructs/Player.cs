using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class Player : Construct
    {
        #region Fields

        private Random _random;
        private Uri[] _player_uris;
        private Uri[] _player_attack_uris;
        private Uri[] _player_win_uris;
        private Uri[] _player_hit_uris;

        private double _hoverDelay;
        private readonly double _hoverDelayDefault = 15;

        private bool _isMovingUp;
        private bool _isMovingDown;

        private bool _isMovingLeft;
        private bool _isMovingRight;

        private double _movementStopDelay;
        private readonly double _movementStopDelayDefault = 6;
        private readonly double _movementStopSpeedLoss = 0.5;

        private double _lastSpeed;
        private readonly double _rotationThreadhold = 9;
        private readonly double _unrotationSpeed = 1.1;
        private readonly double _rotationSpeed = 0.5;
        private readonly double _hoverSpeed = 0.5;

        private double _attackStanceDelay;
        private readonly double _attackStanceDelayDefault = 1.5;

        private double _winStanceDelay;
        private readonly double _winStanceDelayDefault = 15;

        private double _hitStanceDelay;
        private readonly double _hitStanceDelayDefault = 1.5;

        private readonly Image _content_image;

        #endregion

        #region Ctor

        public Player(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            _hoverDelay = _hoverDelayDefault;
            _random = new Random();

            _player_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.PLAYER).Select(x => x.Uri).ToArray();
            _player_attack_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.PLAYER_ATTACK).Select(x => x.Uri).ToArray();
            _player_win_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.PLAYER_WIN).Select(x => x.Uri).ToArray();
            _player_hit_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.PLAYER_HIT).Select(x => x.Uri).ToArray();

            ConstructType = ConstructType.PLAYER;

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.PLAYER);

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var uri = _player_uris[_random.Next(0, _player_uris.Length)];

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(_content_image);

            SpeedOffset = 2;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE;
            Health = 100;
        }

        #endregion

        #region Properties

        public double Health { get; set; }

        public bool IsDead => Health <= 0;

        #endregion

        #region Methods

        public void Reset()
        {
            Health = 100;

            _isMovingUp = false;
            _isMovingDown = false;

            _isMovingLeft = false;
            _isMovingRight = false;

            var uri = _player_uris[_random.Next(0, _player_uris.Length)];
            _content_image.Source = new BitmapImage(uriSource: uri);

            _movementStopDelay = _movementStopDelayDefault;
            _lastSpeed = 0;
        }

        public void Reposition()
        {
            SetPosition(
                  left: ((Scene.Width / 4) * 2) - Width / 2,
                  top: (Scene.Height / 2 - Height / 2) - 150 * Scene.DownScaling,
                  z: 6);
        }

        public void SetAttackStance()
        {
            var uri = _player_attack_uris[_random.Next(0, _player_attack_uris.Length)];
            _content_image.Source = new BitmapImage(uriSource: uri);
            _attackStanceDelay = _attackStanceDelayDefault;
        }

        public void SetWinStance()
        {
            var uri = _player_win_uris[_random.Next(0, _player_win_uris.Length)];
            _content_image.Source = new BitmapImage(uriSource: uri);
            _winStanceDelay = _winStanceDelayDefault;
        }

        public void SetHitStance()
        {
            var uri = _player_hit_uris[_random.Next(0, _player_hit_uris.Length)];
            _content_image.Source = new BitmapImage(uriSource: uri);
            _hitStanceDelay = _hitStanceDelayDefault;
        }

        public void DepleteHitStance()
        {
            if (_hitStanceDelay > 0)
            {
                _hitStanceDelay -= 0.1;

                if (_hitStanceDelay <= 0)
                {
                    SetIdleStance();
                }
            }
        }

        public void DepleteAttackStance()
        {
            if (_attackStanceDelay > 0)
            {
                _attackStanceDelay -= 0.1;

                if (_attackStanceDelay <= 0)
                {
                    SetIdleStance();
                }
            }
        }

        public void DepleteWinStance()
        {
            if (_winStanceDelay > 0)
            {
                _winStanceDelay -= 0.1;

                if (_winStanceDelay <= 0)
                {
                    SetIdleStance();
                }
            }
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

        #region Isometric Movement

        public void MoveUp(double speed)
        {
            _isMovingUp = true;
            _isMovingDown = false;

            _isMovingLeft = false;
            _isMovingRight = false;

            SetLeft(GetLeft() - speed * 2);
            SetTop(GetTop() - speed);

            _movementStopDelay = _movementStopDelayDefault;
            _lastSpeed = speed;

            Rotate(
                rotationDirection: RotationDirection.Backward,
                threadhold: _rotationThreadhold,
                rotationSpeed: _rotationSpeed);
        }

        public void MoveDown(double speed)
        {
            _isMovingDown = true;
            _isMovingUp = false;

            _isMovingLeft = false;
            _isMovingRight = false;

            SetLeft(GetLeft() + speed * 2);
            SetTop(GetTop() + speed);

            _movementStopDelay = _movementStopDelayDefault;
            _lastSpeed = speed;

            Rotate(
                rotationDirection: RotationDirection.Forward,
                threadhold: _rotationThreadhold,
                rotationSpeed: _rotationSpeed);
        }

        public void MoveLeft(double speed)
        {
            _isMovingUp = false;
            _isMovingDown = false;

            _isMovingLeft = true;
            _isMovingRight = false;

            SetLeft(GetLeft() - speed);
            SetTop(GetTop() + speed);

            _movementStopDelay = _movementStopDelayDefault;
            _lastSpeed = speed;

            Rotate(
                rotationDirection: RotationDirection.Backward,
                threadhold: _rotationThreadhold,
                rotationSpeed: _rotationSpeed);
        }

        public void MoveRight(double speed)
        {
            _isMovingUp = false;
            _isMovingDown = false;

            _isMovingLeft = false;
            _isMovingRight = true;

            SetLeft(GetLeft() + speed);
            SetTop(GetTop() - speed);

            _movementStopDelay = _movementStopDelayDefault;
            _lastSpeed = speed;

            Rotate(
                rotationDirection: RotationDirection.Forward,
                threadhold: _rotationThreadhold,
                rotationSpeed: _rotationSpeed);
        }

        #endregion

        #region Linear Movement

        //public void MoveUp(double speed)
        //{
        //    _isMovingUp = true;
        //    _isMovingDown = false;

        //    _isMovingLeft = false;
        //    _isMovingRight = false;

        //    SetTop(GetTop() - speed);

        //    _movementStopDelay = _movementStopDelayDefault;
        //    _lastSpeed = speed;

        //    Rotate(
        //        rotationDirection: RotationDirection.Backward,
        //        threadhold: _rotationThreadhold,
        //        rotationSpeed: _rotationSpeed);
        //}

        //public void MoveDown(double speed)
        //{
        //    _isMovingDown = true;
        //    _isMovingUp = false;

        //    _isMovingLeft = false;
        //    _isMovingRight = false;

        //    SetTop(GetTop() + speed);

        //    _movementStopDelay = _movementStopDelayDefault;
        //    _lastSpeed = speed;

        //    Rotate(
        //        rotationDirection: RotationDirection.Forward,
        //        threadhold: _rotationThreadhold,
        //        rotationSpeed: _rotationSpeed);
        //}

        //public void MoveLeft(double speed)
        //{
        //    _isMovingUp = false;
        //    _isMovingDown = false;

        //    _isMovingLeft = true;
        //    _isMovingRight = false;

        //    SetLeft(GetLeft() - speed * 2);

        //    _movementStopDelay = _movementStopDelayDefault;
        //    _lastSpeed = speed;

        //    Rotate(
        //        rotationDirection: RotationDirection.Backward,
        //        threadhold: _rotationThreadhold,
        //        rotationSpeed: _rotationSpeed);
        //}

        //public void MoveRight(double speed)
        //{
        //    _isMovingUp = false;
        //    _isMovingDown = false;

        //    _isMovingLeft = false;
        //    _isMovingRight = true;

        //    SetLeft(GetLeft() + speed * 2);

        //    _movementStopDelay = _movementStopDelayDefault;
        //    _lastSpeed = speed;

        //    Rotate(
        //        rotationDirection: RotationDirection.Forward,
        //        threadhold: _rotationThreadhold,
        //        rotationSpeed: _rotationSpeed);
        //}

        #endregion

        public void StopMovement()
        {
            if (_movementStopDelay > 0)
            {
                _movementStopDelay--;

                double mvmntSpdLs = _movementStopSpeedLoss;

                if (Scene.IsSlowMotionActivated)
                    mvmntSpdLs = _movementStopSpeedLoss / Constants.DEFAULT_SLOW_MOTION_REDUCTION_FACTOR;


                if (_isMovingUp)
                {
                    if (_lastSpeed > 0)
                    {
                        MoveUp(_lastSpeed - mvmntSpdLs);
                    }
                }
                else if (_isMovingDown)
                {
                    if (_lastSpeed > 0)
                    {
                        MoveDown(_lastSpeed - mvmntSpdLs);
                    }
                }
                else if (_isMovingLeft)
                {
                    if (_lastSpeed > 0)
                    {
                        MoveLeft(_lastSpeed - mvmntSpdLs);
                    }
                }
                else if (_isMovingRight)
                {
                    if (_lastSpeed > 0)
                    {
                        MoveRight(_lastSpeed - mvmntSpdLs);
                    }
                }

                UnRotate(rotationSpeed: _unrotationSpeed);
            }
            else
            {
                _isMovingUp = false;
                _isMovingDown = false;
                _isMovingLeft = false;
                _isMovingRight = false;
            }
        }

        public void LooseHealth()
        {
            Health -= 5;
        }

        public void GainHealth()
        {
            Health += 10;
        }

        private void SetIdleStance()
        {
            var uri = _player_uris[_random.Next(0, _player_uris.Length)];
            _content_image.Source = new BitmapImage(uriSource: uri);
        }

        #endregion        
    }
}
