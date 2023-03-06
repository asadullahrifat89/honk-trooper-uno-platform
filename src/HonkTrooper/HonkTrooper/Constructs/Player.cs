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

        private int _hoverDelay;
        private readonly int _hoverDelayDefault = 15;

        private bool _isMovingUp;
        private bool _isMovingDown;

        private bool _isMovingLeft;
        private bool _isMovingRight;

        private double _movementStopDelay;
        private readonly double _movementStopDelayDefault = 5;
        private readonly double _movementStopSpeedLoss = 0.5;

        private double _lastSpeed;

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
            DropShadowDistance = 50;
            Health = 100;
        }

        #endregion

        #region Properties

        public double Health { get; set; }

        #endregion

        #region Methods

        public void Reposition(Scene scene)
        {
            SetPosition(
                  left: ((scene.Width / 4) * 2) - Width / 2,
                  top: scene.Height / 2 - Height / 2,
                  z: 6);
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
        }

        public void StopMovement()
        {
            if (_movementStopDelay > 0)
            {
                _movementStopDelay--;

                if (_isMovingUp)
                {
                    if (_lastSpeed > 0)
                        MoveUp(_lastSpeed - _movementStopSpeedLoss);
                }
                else if (_isMovingDown)
                {
                    if (_lastSpeed > 0)
                        MoveDown(_lastSpeed - _movementStopSpeedLoss);
                }
                else if (_isMovingLeft)
                {
                    if (_lastSpeed > 0)
                        MoveLeft(_lastSpeed - _movementStopSpeedLoss);
                }
                else if (_isMovingRight)
                {
                    if (_lastSpeed > 0)
                        MoveRight(_lastSpeed - _movementStopSpeedLoss);
                }
            }
            else
            {
                _isMovingUp = false;
                _isMovingDown = false;
                _isMovingLeft = false;
                _isMovingRight = false;
            }
        }

        #endregion        
    }
}
