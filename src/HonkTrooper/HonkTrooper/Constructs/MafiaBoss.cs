﻿using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace HonkTrooper
{
    public partial class MafiaBoss : AnimableConstruct
    {
        #region Fields

        private readonly Random _random;
        private readonly Uri[] _mafia_boss_idle_uris;
        private readonly Uri[] _mafia_boss_hit_uris;
        private readonly Uri[] _mafia_boss_win_uris;

        private readonly double _grace = 7;
        private readonly double _lag = 125;

        private double _changeMovementPatternDelay;

        private readonly Image _content_image;

        private double _hitStanceDelay;
        private readonly double _hitStanceDelayDefault = 1.5;

        private double _winStanceDelay;
        private readonly double _winStanceDelayDefault = 8;

        private readonly AudioStub _audioStub;

        private MovementDirection _movementDirection;

        #endregion

        #region Ctor

        public MafiaBoss(
           Func<Construct, bool> animateAction,
           Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.MAFIA_BOSS;

            _random = new Random();

            _mafia_boss_idle_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.MAFIA_BOSS_IDLE).Select(x => x.Uri).ToArray();
            _mafia_boss_hit_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.MAFIA_BOSS_HIT).Select(x => x.Uri).ToArray();
            _mafia_boss_win_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.MAFIA_BOSS_WIN).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.MAFIA_BOSS);

            var width = size.Width;
            var height = size.Height;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var uri = ConstructExtensions.GetRandomContentUri(_mafia_boss_idle_uris);

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(_content_image);

            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE;

            _audioStub = new AudioStub(
                (SoundType.UFO_HOVERING, 0.8, true),
                (SoundType.BOSS_ENTRY, 0.8, false),
                (SoundType.BOSS_DEAD, 1, false));
        }

        #endregion

        #region Properties

        public bool IsAttacking { get; set; }

        public double Health { get; set; }

        public bool IsDead => Health <= 0;

        public MafiaBossMovementPattern MovementPattern { get; set; }

        private BossStance MafiaBossStance { get; set; } = BossStance.Idle;

        #endregion

        #region Methods

        public void Reset()
        {
            _audioStub.Play(SoundType.BOSS_ENTRY);

            PlaySoundLoop();

            Opacity = 1;
            Health = 100;
            IsAttacking = false;
            MafiaBossStance = BossStance.Idle;

            _movementDirection = MovementDirection.None;

            var uri = ConstructExtensions.GetRandomContentUri(_mafia_boss_idle_uris);
            _content_image.Source = new BitmapImage(uriSource: uri);

            RandomizeMovementPattern();
            SetScaleTransform(1);
        }

        private void PlaySoundLoop()
        {
            _audioStub.Play(SoundType.UFO_HOVERING);
        }

        public void SetHitStance()
        {
            if (MafiaBossStance != BossStance.Win)
            {
                MafiaBossStance = BossStance.Hit;
                var uri = ConstructExtensions.GetRandomContentUri(_mafia_boss_hit_uris);
                _content_image.Source = new BitmapImage(uriSource: uri);
                _hitStanceDelay = _hitStanceDelayDefault;
            }
        }

        public void SetWinStance()
        {
            MafiaBossStance = BossStance.Win;
            var uri = ConstructExtensions.GetRandomContentUri(_mafia_boss_win_uris);
            _content_image.Source = new BitmapImage(uriSource: uri);
            _winStanceDelay = _winStanceDelayDefault;
        }

        public void SetIdleStance()
        {
            MafiaBossStance = BossStance.Idle;
            var uri = ConstructExtensions.GetRandomContentUri(_mafia_boss_idle_uris);
            _content_image.Source = new BitmapImage(uriSource: uri);
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

        public void LooseHealth()
        {
            Health -= 5;

            if (IsDead)
            {
                IsAttacking = false;
                StopSoundLoop();
                _audioStub.Play(SoundType.BOSS_DEAD);
            }
        }

        public void StopSoundLoop()
        {
            _audioStub.Stop(SoundType.UFO_HOVERING);
        }

        public void Move(
              double speed,
              double sceneWidth,
              double sceneHeight,
              Rect playerPoint)
        {
            switch (MovementPattern)
            {
                case MafiaBossMovementPattern.PLAYER_SEEKING:
                    SeekPlayer(playerPoint);
                    break;
                case MafiaBossMovementPattern.RECTANGULAR_SQUARE:
                    MoveInRectangularSquares(
                        speed: speed,
                        sceneWidth: sceneWidth,
                        sceneHeight: sceneHeight);
                    break;
                case MafiaBossMovementPattern.RIGHT_LEFT:
                    MoveRightLeft(
                        speed: speed,
                        sceneWidth: sceneWidth,
                        sceneHeight: sceneHeight);
                    break;
                case MafiaBossMovementPattern.UP_DOWN:
                    MoveUpDown(
                        speed: speed,
                        sceneWidth: sceneWidth,
                        sceneHeight: sceneHeight);
                    break;
            }
        }

        private bool MoveInRectangularSquares(double speed, double sceneWidth, double sceneHeight)
        {
            _changeMovementPatternDelay -= 0.1;

            if (_changeMovementPatternDelay < 0)
            {
                RandomizeMovementPattern();
                return true;
            }

            if (IsAttacking && _movementDirection == MovementDirection.None)
            {
                _movementDirection = MovementDirection.UpRight;
            }
            else
            {
                IsAttacking = true;
            }

            return false;
        }

        private void SeekPlayer(Rect playerPoint)
        {
            _changeMovementPatternDelay -= 0.1;

            if (_changeMovementPatternDelay < 0)
            {
                RandomizeMovementPattern();
            }

            double left = GetLeft();
            double top = GetTop();

            double playerMiddleX = left + Width / 2;
            double playerMiddleY = top + Height / 2;

            // move up
            if (playerPoint.Y < playerMiddleY - _grace)
            {
                var distance = Math.Abs(playerPoint.Y - playerMiddleY);
                double speed = GetFlightSpeed(distance);

                SetTop(top - speed);
            }

            // move left
            if (playerPoint.X < playerMiddleX - _grace)
            {
                var distance = Math.Abs(playerPoint.X - playerMiddleX);
                double speed = GetFlightSpeed(distance);

                SetLeft(left - speed);
            }

            // move down
            if (playerPoint.Y > playerMiddleY + _grace)
            {
                var distance = Math.Abs(playerPoint.Y - playerMiddleY);
                double speed = GetFlightSpeed(distance);

                SetTop(top + speed);
            }

            // move right
            if (playerPoint.X > playerMiddleX + _grace)
            {
                var distance = Math.Abs(playerPoint.X - playerMiddleX);
                double speed = GetFlightSpeed(distance);

                SetLeft(left + speed);
            }
        }

        private double GetFlightSpeed(double distance)
        {
            var flightSpeed = distance / _lag;
            return flightSpeed;
        }

        private bool MoveRightLeft(double speed, double sceneWidth, double sceneHeight)
        {
            _changeMovementPatternDelay -= 0.1;

            if (_changeMovementPatternDelay < 0)
            {
                RandomizeMovementPattern();
                return true;
            }

            if (IsAttacking && _movementDirection == MovementDirection.None)
            {
                _movementDirection = MovementDirection.Right;
            }
            else
            {
                IsAttacking = true;
            }

            if (IsAttacking)
            {
                if (_movementDirection == MovementDirection.Right)
                {
                    MoveRight(speed);

                    if (GetRight() > sceneWidth)
                    {
                        _movementDirection = MovementDirection.Left;
                    }
                }
                else
                {
                    if (_movementDirection == MovementDirection.Left)
                    {
                        MoveLeft(speed);

                        if (GetLeft() < 0)
                        {
                            _movementDirection = MovementDirection.Right;
                        }
                    }
                }
            }

            return false;
        }

        private bool MoveUpDown(double speed, double sceneWidth, double sceneHeight)
        {
            _changeMovementPatternDelay -= 0.1;

            if (_changeMovementPatternDelay < 0)
            {
                RandomizeMovementPattern();
                return true;
            }

            if (IsAttacking && _movementDirection == MovementDirection.None)
            {
                _movementDirection = MovementDirection.Up;
            }
            else
            {
                IsAttacking = true;
            }

            if (IsAttacking)
            {
                if (_movementDirection == MovementDirection.Up)
                {
                    MoveUp(speed);

                    if (GetTop() < 0)
                    {
                        _movementDirection = MovementDirection.Down;
                    }
                }
                else
                {
                    if (_movementDirection == MovementDirection.Down)
                    {
                        MoveDown(speed);

                        if (GetBottom() > sceneHeight)
                        {
                            _movementDirection = MovementDirection.Up;
                        }
                    }
                }
            }

            return false;
        }

        private void RandomizeMovementPattern()
        {
            _changeMovementPatternDelay = _random.Next(40, 60);
            _movementDirection = MovementDirection.None;
            MovementPattern = (MafiaBossMovementPattern)_random.Next(Enum.GetNames(typeof(MafiaBossMovementPattern)).Length);
        }

        #endregion
    }

    public enum MafiaBossMovementPattern
    {
        PLAYER_SEEKING,
        RECTANGULAR_SQUARE,
        RIGHT_LEFT,
        UP_DOWN,
    }
}
