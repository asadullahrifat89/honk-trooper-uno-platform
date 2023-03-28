using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class UfoEnemy : VehicleBase
    {
        #region Fields

        private readonly Random _random;
        private readonly Uri[] _enemy_uris;
        private readonly Image _content_image;

        private double _attackDelay;

        #endregion

        #region Ctor

        public UfoEnemy(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.UFO_ENEMY;

            _random = new Random();

            _enemy_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.UFO_ENEMY).Select(x => x.Uri).ToArray();

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.UFO_ENEMY);

            var width = size.Width;
            var height = size.Height;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            SetSize(width: width, height: height);

            var uri = ConstructExtensions.GetRandomContentUri(_enemy_uris);

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: uri)
            };

            SetChild(_content_image);

            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE;
        }

        #endregion

        #region Properties

        public double Health { get; set; }

        public bool IsDead => Health <= 0;

        #endregion

        #region Methods

        public void Reset()
        {
            Opacity = 1;
            SetScaleTransform(1);

            Health = 5 * _random.Next(4);

            WillHonk = Convert.ToBoolean(_random.Next(2));

            if (WillHonk)
                SetHonkDelay();

            var uri = ConstructExtensions.GetRandomContentUri(_enemy_uris);
            _content_image.Source = new BitmapImage(uri);

            SpeedOffset = _random.Next(-2, 2);
        }

        public bool Attack()
        {
            if (!IsDead)
            {
                _attackDelay--;

                if (_attackDelay < 0)
                {
                    SetAttackDelay();
                    return true;
                }
            }

            return false;
        }

        public void SetAttackDelay()
        {
            _attackDelay = _random.Next(50, 80);
        }

        public void LooseHealth()
        {
            Health -= 5;
        }

        #endregion
    }
}
