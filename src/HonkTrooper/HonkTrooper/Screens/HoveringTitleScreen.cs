using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using System;

namespace HonkTrooper
{
    public partial class HoveringTitleScreen : AnimableConstruct
    {
        #region Fields

        private readonly Storyboard _storyboard;
        private readonly DoubleAnimation _doubleAnimation;

        #endregion

        #region Ctor

        public HoveringTitleScreen()
        {
            _doubleAnimation = new DoubleAnimation()
            {
                Duration = new Duration(TimeSpan.FromSeconds(2)),
                From = 0,
                To = 1,
            };

            Storyboard.SetTarget(_doubleAnimation, this);
            Storyboard.SetTargetProperty(_doubleAnimation, "Opacity");

            _storyboard = new Storyboard();
            _storyboard.Children.Add(_doubleAnimation);
        }

        #endregion

        #region Methods

        public void Reset()
        {
            _storyboard.Begin();
        }

        public void Reposition()
        {
            SetPosition(
                left: (((Scene.Width / 4) * 2) - Width / 2),
                top: ((Scene.Height / 2) - Height / 2),
                z: 10);
        }

        #endregion
    }
}
