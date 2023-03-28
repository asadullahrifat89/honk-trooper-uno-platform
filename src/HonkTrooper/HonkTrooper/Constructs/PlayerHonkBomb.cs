using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

namespace HonkTrooper
{
    public partial class PlayerHonkBomb : MovableConstruct
    {
        #region Fields

        private Uri[] _bomb_uris;
        private readonly Uri[] _blast_uris;
        private readonly Uri[] _bang_uris;

        private readonly Image _content_image;

        private readonly AudioStub _audioStub;

        #endregion

        #region Ctor

        public PlayerHonkBomb(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            _bomb_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.PLAYER_HONK_BOMB && x.Uri.OriginalString.Contains("cracker")).Select(x => x.Uri).ToArray();
            _blast_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.BLAST).Select(x => x.Uri).ToArray();
            _bang_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.BANG).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.PLAYER_HONK_BOMB);

            ConstructType = ConstructType.PLAYER_HONK_BOMB;

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

            BorderThickness = new Microsoft.UI.Xaml.Thickness(Constants.DEFAULT_BLAST_RING_BORDER_THICKNESS);
            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(Constants.DEFAULT_BLAST_RING_CORNER_RADIUS);

            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET + 1;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE - 15;

            _audioStub = new AudioStub((SoundType.CRACKER_DROP, 0.3, false), (SoundType.CRACKER_BLAST, 1, false), (SoundType.TRASH_CAN_HIT, 1, false));
        }

        #endregion

        #region Properties

        public bool IsBlasting { get; set; }

        private PlayerHonkBombTemplate HonkBombTemplate { get; set; }

        #endregion

        #region Methods

        public void Reset()
        {
            _audioStub.Play(SoundType.CRACKER_DROP);

            Opacity = 1;

            var uri = ConstructExtensions.GetRandomContentUri(_bomb_uris);
            _content_image.Source = new BitmapImage(uri);

            BorderBrush = new SolidColorBrush(Colors.Transparent);

            SetScaleTransform(1);
            SetRotation(0);

            IsBlasting = false;
        }

        public void SetHonkBombTemplate(PlayerHonkBombTemplate honkBombTemplate)
        {
            HonkBombTemplate = honkBombTemplate;

            switch (HonkBombTemplate)
            {
                case PlayerHonkBombTemplate.Cracker:
                    {
                        _bomb_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.PLAYER_HONK_BOMB && x.Uri.OriginalString.Contains("cracker")).Select(x => x.Uri).ToArray();
                    }
                    break;
                case PlayerHonkBombTemplate.TrashCan:
                    {
                        _bomb_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.PLAYER_HONK_BOMB && x.Uri.OriginalString.Contains("trash")).Select(x => x.Uri).ToArray();
                    }
                    break;
                default:
                    break;
            }

            var uri = ConstructExtensions.GetRandomContentUri(_bomb_uris);
            _content_image.Source = new BitmapImage(uriSource: uri);
        }

        public void Reposition(PlayerBalloon player)
        {
            SetPosition(
                left: (player.GetLeft() + player.Width / 2) - Width / 2,
                top: player.GetBottom() - (35),
                z: 7);
        }       

        public void SetBlast()
        {
            Uri uri = null;

            switch (HonkBombTemplate)
            {
                case PlayerHonkBombTemplate.Cracker:
                    {
                        _audioStub.Play(SoundType.CRACKER_BLAST);
                        uri = ConstructExtensions.GetRandomContentUri(_blast_uris);

                        BorderBrush = new SolidColorBrush(Colors.Goldenrod);
                    }
                    break;
                case PlayerHonkBombTemplate.TrashCan:
                    {
                        _audioStub.Play(SoundType.TRASH_CAN_HIT);
                        uri = ConstructExtensions.GetRandomContentUri(_bang_uris);

                        BorderBrush = new SolidColorBrush(Colors.GreenYellow);
                    }
                    break;
                default:
                    break;
            }

            SetScaleTransform(Constants.DEFAULT_BLAST_SHRINK_SCALE);

            _content_image.Source = new BitmapImage(uri);
            IsBlasting = true;
        }

        #endregion
    }

    public enum PlayerHonkBombTemplate
    {
        Cracker,
        TrashCan,
    }
}
