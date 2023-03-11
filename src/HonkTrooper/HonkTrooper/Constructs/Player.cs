﻿using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace HonkTrooper
{
    public partial class Player : Construct
    {
        #region Fields

        private Random _random;
        private Uri[] _player_uris;

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

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.PLAYER);

            ConstructType = ConstructType.PLAYER;

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var uri = _player_uris[_random.Next(0, _player_uris.Length)];

            var content = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(content);

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

        #endregion        
    }
}
