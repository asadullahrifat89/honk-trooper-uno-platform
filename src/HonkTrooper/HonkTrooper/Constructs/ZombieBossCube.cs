using System;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;

namespace HonkTrooper
{
    public partial class ZombieBossCube : VehicleBase
    {
        #region Fields

        private readonly Random _random;

        private readonly Uri[] _bomb_uris;
        private readonly Uri[] _bomb_blast_uris;

        private readonly Image _content_image;

        private readonly AudioStub _audioStub;

        #endregion

        #region Ctor

        public ZombieBossCube(
          Func<Construct, bool> animateAction,
          Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.ZOMBIE_BOSS_CUBE;

            _random = new Random();

            _bomb_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.ZOMBIE_BOSS_CUBE).Select(x => x.Uri).ToArray();
            _bomb_blast_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.BOMB_BLAST).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.ZOMBIE_BOSS_CUBE);

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
    }
}
