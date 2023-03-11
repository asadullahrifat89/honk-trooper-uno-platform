﻿using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;
using Windows.Foundation;

namespace HonkTrooper
{
    public partial class BossBombSeeking : BombSeeking
    {
        #region Fields

        private Random _random;
        private Uri[] _bomb_uris;
        private Uri[] _bomb_blast_uris;

        #endregion

        #region Ctor

        public BossBombSeeking(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            _random = new Random();

            _bomb_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.BOSS_BOMB_SEEKING).Select(x => x.Uri).ToArray();
            _bomb_blast_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.BOMB_BLAST).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.BOSS_BOMB_SEEKING);

            ConstructType = ConstructType.BOSS_BOMB_SEEKING;

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var uri = _bomb_uris[_random.Next(0, _bomb_uris.Length)];

            var content = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(content);
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET - 4;
            IsometricDisplacement = 0.5;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE;
        }

        #endregion

        #region Properties

        public bool IsBlasting { get; set; }

        public double TimeLeftUntilBlast { get; set; }

        #endregion

        #region Methods

        public void Reposition(Boss boss, double downScaling)
        {
            SetPosition(
                left: (boss.GetLeft() + boss.Width / 2) - Width / 2,
                top: boss.GetBottom() - (40 * downScaling),
                z: 7);
        }

        public void Reset()
        {
            Opacity = 1;
            SetScaleTransform(1);

            var uri = _bomb_uris[_random.Next(0, _bomb_uris.Length)];

            var content = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(content);

            IsBlasting = false;
            TimeLeftUntilBlast = 25;
        }

        public bool RunOutOfTimeToBlast()
        {
            TimeLeftUntilBlast -= 0.1;

            if (TimeLeftUntilBlast <= 0)
                return true;

            return false;
        }

        public void SetBlast()
        {
            var uri = _bomb_blast_uris[_random.Next(0, _bomb_blast_uris.Length)];

            var content = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(content);

            IsBlasting = true;
        }

        #endregion
    }
}
