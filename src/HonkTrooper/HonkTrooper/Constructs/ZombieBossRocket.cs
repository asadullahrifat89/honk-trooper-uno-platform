using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

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
            _bomb_blast_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.BLAST).Select(x => x.Uri).ToArray();

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
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE + 10;

            _audioStub = new AudioStub((SoundType.ORB_LAUNCH, 0.3, false), (SoundType.ROCKET_BLAST, 1, false));
        }

        #endregion

        #region Properties

        public double Health { get; set; }

        public bool IsDestroyed => Health <= 0;

        #endregion

        #region Methods

        public void Reset()
        {
            _audioStub.Play(SoundType.ORB_LAUNCH);

            Opacity = 1;
            SetScaleTransform(1);

            BorderBrush = new SolidColorBrush(Colors.Transparent);
            BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(0);

            Health = _random.Next(1, 3) * 50;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET + 1.5;

            IsBlasting = false;

            var uri = ConstructExtensions.GetRandomContentUri(_bomb_uris);
            _content_image.Source = new BitmapImage(uri);

            _autoBlastDelay = _autoBlastDelayDefault;
        }

        public void Reposition()
        {
            var topOrLeft = _random.Next(2); // generate top and left corner lane wise vehicles
            var lane = ScreenExtensions.Height < 450 ? _random.Next(3) : _random.Next(4); // generate number of lanes based on screen height
            var randomY = _random.Next(-10, 10);

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

        public void LooseHealth()
        {
            Health -= 50;

            if (IsDestroyed)
            {
                SetBlast();
            }
        }

        public void SetBlast()
        {
            _audioStub.Play(SoundType.ROCKET_BLAST);
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET - 1;

            SetScaleTransform(Constants.DEFAULT_BLAST_SHRINK_SCALE - 0.2);

            BorderBrush = new SolidColorBrush(Colors.DarkGreen);
            BorderThickness = new Microsoft.UI.Xaml.Thickness(Constants.DEFAULT_BLAST_RING_BORDER_THICKNESS);
            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(Constants.DEFAULT_BLAST_RING_CORNER_RADIUS);

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
