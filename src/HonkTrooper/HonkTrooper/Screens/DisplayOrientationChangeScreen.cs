using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class DisplayOrientationChangeScreen : HoveringTitleScreen
    {
        #region Fields       

        private readonly TextBlock _titleScreenText;

        #endregion

        #region Ctor

        public DisplayOrientationChangeScreen(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            ConstructType = ConstructType.TITLE_SCREEN;

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.TITLE_SCREEN);

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            SetSize(width: width, height: height);

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            CornerRadius = new CornerRadius(5);

            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
            DropShadowDistance = Constants.DEFAULT_DROP_SHADOW_DISTANCE;

            #region Base Container

            Grid rootGrid = new();

            rootGrid.Children.Add(new Border()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                CornerRadius = new CornerRadius(15),
                Opacity = 0.6,
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
            });

            StackPanel container = new()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            #endregion

            #region Content

            // title screen text

            _titleScreenText = new TextBlock()
            {
                Text =
                "Pls" + Environment.NewLine +
                "Rotate" + Environment.NewLine +
                "Your Screen",
                FontSize = Constants.DEFAULT_GUI_FONT_SIZE - 5,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5),
                Foreground = new SolidColorBrush(Colors.White),
                TextWrapping = TextWrapping.Wrap,
            };

            container.Children.Add(_titleScreenText);

            #endregion

            rootGrid.Children.Add(container);
            SetChild(rootGrid);
        }

        #endregion
    }
}
