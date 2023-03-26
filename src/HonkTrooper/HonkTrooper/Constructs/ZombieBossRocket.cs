﻿using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;

namespace HonkTrooper
{
    public partial class ZombieBossRocket : AnimableConstruct
    {
        #region Fields

        private readonly Random _random;

        private readonly Uri[] _bomb_uris;
        private readonly Uri[] _bomb_blast_uris;

        private readonly Image _content_image;

        private double _autoBlastDelay;
        private readonly double _autoBlastDelayDefault = 14;

        private readonly AudioStub _audioStub;

        #endregion

        #region Ctor

        public ZombieBossRocket(
          Func<Construct, bool> animateAction,
          Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.ZOMBIE_BOSS_ROCKET;

            _random = new Random();

            _bomb_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.ZOMBIE_BOSS_ROCKET).Select(x => x.Uri).ToArray();
            _bomb_blast_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.BOMB_BLAST).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.ZOMBIE_BOSS_ROCKET);

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

            _audioStub = new AudioStub((SoundType.ORB_LAUNCH, 0.3, false), (SoundType.ROCKET_BLAST, 1, false));
        }

        #endregion

        #region Methods

        public void Reposition()
        {
            var topOrLeft = _random.Next(2); // generate top and left corner lane wise vehicles            
            var randomY = _random.Next(-10, 10);
            var lane = _random.Next(4); // generate number of lanes based of screen height

            switch (topOrLeft)
            {
                case 0:
                    {
                        var xLaneWidth = Constants.DEFAULT_SCENE_WIDTH / 4;

                        switch (lane)
                        {
                            case 0:
                                {
                                    SetPosition(
                                        left: 0 - Width / 2,
                                        top: (Height * -1) + randomY);
                                }
                                break;
                            case 1:
                                {
                                    SetPosition(
                                        left: (xLaneWidth - Width / 1.5),
                                        top: (Height * -1) + randomY);
                                }
                                break;
                            case 2:
                                {
                                    SetPosition(
                                       left: (xLaneWidth * 2 - Width / 1.5),
                                       top: (Height * -1) + randomY);
                                }
                                break;
                            case 3:
                                {
                                    SetPosition(
                                       left: (xLaneWidth * 3 - Width / 1.5),
                                       top: (Height * -1) + randomY);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case 1:
                    {
                        var yLaneHeight = Constants.DEFAULT_SCENE_HEIGHT / 6;

                        switch (lane)
                        {
                            case 0:
                                {
                                    SetPosition(
                                        left: Width * -1,
                                        top: (0 - Height / 2) + randomY);
                                }
                                break;
                            case 1:
                                {
                                    SetPosition(
                                        left: Width * -1,
                                        top: (yLaneHeight - Height / 3) + randomY);
                                }
                                break;
                            case 2:
                                {
                                    SetPosition(
                                       left: Width * -1,
                                       top: (yLaneHeight * 2 - Height / 3) + randomY);
                                }
                                break;
                            case 3:
                                {
                                    SetPosition(
                                       left: Width * -1,
                                       top: (yLaneHeight * 3 - Height / 3) + randomY);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
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