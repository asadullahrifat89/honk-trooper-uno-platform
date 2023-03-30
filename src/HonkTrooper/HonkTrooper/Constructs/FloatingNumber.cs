using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;

namespace HonkTrooper
{
    public partial class FloatingNumber : MovableConstruct
    {
        #region Fields

        private readonly Random _random;
        private readonly TextBlock _textBlock;

        private double _messageOnScreenDelay;
        private readonly double _messageOnScreenDelayDefault = 5;

        private MovementDirection _movementDirection;

        #endregion

        #region Ctor

        public FloatingNumber(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.FLOATING_NUMBER;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            _random = new Random();

            SetConstructSize();

            _textBlock = new TextBlock() { FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Colors.White), FontSize = Constants.DEFAULT_GUI_FONT_SIZE - 3 };

            SetChild(_textBlock);
            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
        }

        #endregion


        #region Properties

        public bool IsDepleted => _messageOnScreenDelay <= 0;

        #endregion

        #region Methods

        public void Reset(int number)
        {
            _textBlock.Text = $"-{number}";
            _messageOnScreenDelay = _messageOnScreenDelayDefault;
            RandomizeMovementDirection();
        }

        public void Reposition(Construct source)
        {
            SetPosition(
                left: (source.GetLeft() + source.Width / 2) - Width / 2,
                top: (source.GetTop() + source.Height / 2) - Height / 2);
        }

        public bool DepleteOnScreenDelay()
        {
            _messageOnScreenDelay -= 0.1;
            return true;
        }

        public void Move()
        {
            var speed = GetMovementSpeed();

            switch (_movementDirection)
            {
                case MovementDirection.Up:
                    MoveUp(speed / 2.5);
                    break;
                case MovementDirection.UpLeft:
                    MoveUpLeft(speed / 2.5);
                    break;
                case MovementDirection.UpRight:
                    MoveUpRight(speed / 2.5);
                    break;
                case MovementDirection.Down:
                    MoveDown(speed / 2.5);
                    break;
                case MovementDirection.DownLeft:
                    MoveDownLeft(speed / 2.5);
                    break;
                case MovementDirection.DownRight:
                    MoveDownRight(speed / 2.5);
                    break;
                case MovementDirection.Right:
                    MoveRight(speed / 2.5);
                    break;
                case MovementDirection.Left:
                    MoveLeft(speed / 2.5);
                    break;
                default:
                    break;
            }
        }

        private void RandomizeMovementDirection()
        {
            _movementDirection = (MovementDirection)_random.Next(1, Enum.GetNames(typeof(MovementDirection)).Length);
        }

        #endregion
    }
}
