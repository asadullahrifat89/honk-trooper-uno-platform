using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace HonkTrooper
{
    public partial class AssetsLoadingScreen : HoveringTitleScreen
    {
        #region Fields

        private readonly ProgressBar _progressBar;

        private readonly TextBlock _sub_title_text;

        #endregion

        #region Ctor

        public AssetsLoadingScreen(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction)
        {
            ConstructType = ConstructType.TITLE_SCREEN;

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.TITLE_SCREEN);

            var width = size.Width;
            var height = size.Height;

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
                Background = new SolidColorBrush(Colors.DeepSkyBlue),
                CornerRadius = new CornerRadius(15),
                Opacity = 0.6,
                //BorderBrush = new SolidColorBrush(Colors.White),
                //BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
            });

            Grid container = new() { VerticalAlignment = VerticalAlignment.Center };
            container.RowDefinitions.Add(new RowDefinition());
            container.RowDefinitions.Add(new RowDefinition());

            #endregion

            #region Content

            #region Loading Bar

            _progressBar = new()
            {
                Width = 200,
                Height = 10,
                Value = 0,
                Maximum = 0,
                Minimum = 0,
                Margin = new Thickness(5),
                Foreground = new SolidColorBrush(Colors.Crimson),
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            Grid.SetRow(_progressBar, 0);

            container.Children.Add(_progressBar);

            #endregion

            #region SubTitle

            _sub_title_text = new TextBlock()
            {
                FontSize = Constants.DEFAULT_GUI_FONT_SIZE - 7,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5),
                Foreground = new SolidColorBrush(Colors.White),
            };

            Grid.SetRow(_sub_title_text, 1);

            container.Children.Add(_sub_title_text);

            #endregion 

            #endregion

            rootGrid.Children.Add(container);
            SetChild(rootGrid);
        }

        #endregion

        #region Methods

        public void SetSubTitle(string subTitle)
        {
            _sub_title_text.Text = subTitle;
        }

        public async Task PreloadAssets(Action completed)
        {
            _progressBar.IsIndeterminate = false;
            _progressBar.ShowPaused = false;
            _progressBar.Value = 0;
            _progressBar.Minimum = 0;

            _progressBar.Maximum = Constants.CONSTRUCT_TEMPLATES.Length;

            foreach (var template in Constants.CONSTRUCT_TEMPLATES)
            {
                await GetFileAsync(template.Uri);
            }

            if (_progressBar.Value == _progressBar.Maximum)
                completed?.Invoke();
        }

        private async Task GetFileAsync(Uri uri)
        {
            await StorageFile.GetFileFromApplicationUriAsync(uri);
            _progressBar.Value++;
        }

        #endregion
    }
}
