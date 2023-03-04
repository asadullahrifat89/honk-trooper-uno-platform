using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Linq;
using System.Numerics;

namespace HonkPooper
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

        private double _movementStopDelay;
        private readonly double _movementStopDelayDefault = 10;

        private double _lastSpeed;
        #endregion

        public Player(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
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
            _hoverDelay = _hoverDelayDefault;
        }

        public void Reposition(Scene scene)
        {
            SetPosition(
                  left: (scene.Width / 2) + (scene.Width / 4) - Width / 2,
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

        public void MoveUp(double speed, double downScaling)
        {
            _isMovingUp = true;
            _isMovingDown = false;

            SetLeft(GetLeft() + speed);
            SetTop(GetTop() - speed);

            _movementStopDelay = _movementStopDelayDefault * downScaling;
            _lastSpeed = speed;
        }

        public void MoveDown(double speed, double downScaling)
        {
            _isMovingDown = true;
            _isMovingUp = false;

            SetLeft(GetLeft() - speed);
            SetTop(GetTop() + speed);

            _movementStopDelay = _movementStopDelayDefault * downScaling;
            _lastSpeed = speed;
        }

        public void StopMovement(double downScaling)
        {
            if (_movementStopDelay > 0)
            {
                _movementStopDelay--;

                if (_isMovingUp)
                {
                    if (_lastSpeed > 0)
                        MoveUp(_lastSpeed - 0.1, downScaling);
                }
                else if (_isMovingDown)
                {
                    if (_lastSpeed > 0)
                        MoveDown(_lastSpeed - 0.1, downScaling);
                }
            }
            else
            {
                _isMovingUp = false;
                _isMovingDown = false;
            }
        }
    }
}
