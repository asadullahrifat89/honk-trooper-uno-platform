﻿using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;

namespace HonkTrooper
{
    public partial class UfoEnemyRocket : Rocket
    {
        #region Fields

        private readonly Random _random;

        private readonly Uri[] _bomb_uris;
        private readonly Uri[] _bomb_blast_uris;

        private readonly Image _content_image;


        private double _autoBlastDelay;
        private readonly double _autoBlastDelayDefault = 12;

        private readonly AudioStub _audioStub;

        #endregion

        #region Ctor

        public UfoEnemyRocket(
           Func<Construct, bool> animateAction,
           Func<Construct, bool> recycleAction)
        {
            _random = new Random();

            _bomb_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.UFO_ENEMY_ROCKET).Select(x => x.Uri).ToArray();
            _bomb_blast_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.BOMB_BLAST).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.UFO_ENEMY_ROCKET);

            ConstructType = ConstructType.UFO_ENEMY_ROCKET;

            var width = size.Width;
            var height = size.Height;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var uri = ConstructExtensions.GetRandomContentUri(_bomb_uris);

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(_content_image);

            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET + 2;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE + 10;

            _audioStub = new AudioStub((SoundType.ORB_LAUNCH, 0.5, false), (SoundType.ROCKET_BLAST, 1, false));

        }

        #endregion

        #region Methods

        public void Reposition(UfoEnemy ufoEnemy)
        {
            SetPosition(
                left: (ufoEnemy.GetLeft() + ufoEnemy.Width / 2) - Width / 2,
                top: ufoEnemy.GetBottom() - (50),
                z: 7);
        }

        public void Reset()
        {
            _audioStub.Play(SoundType.ORB_LAUNCH);

            Opacity = 1;
            SetScaleTransform(1);

            IsBlasting = false;

            var uri = ConstructExtensions.GetRandomContentUri(_bomb_uris);
            _content_image.Source = new BitmapImage(uri);

            _autoBlastDelay = _autoBlastDelayDefault;
        }

        public void SetBlast()
        {
            _audioStub.Play(SoundType.ROCKET_BLAST);

            var uri = ConstructExtensions.GetRandomContentUri(_bomb_blast_uris);
            _content_image.Source = new BitmapImage(uri);

            IsBlasting = true;
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
