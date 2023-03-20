using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class TitleScreen : HoveringTitleScreen
    {
        #region Fields       

        private readonly TextBlock _titleScreenText;
        private readonly Image _content_image;

        private readonly AudioStub _audioStub;

        #endregion

        #region Ctor

        public TitleScreen
            (Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            Func<bool> playAction,
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

            Grid container = new() { VerticalAlignment = VerticalAlignment.Center };
            container.RowDefinitions.Add(new RowDefinition());
            container.RowDefinitions.Add(new RowDefinition());
            container.RowDefinitions.Add(new RowDefinition());

            #endregion

            #region Content

            #region Image

            var playerUris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.PLAYER).Select(x => x.Uri).ToArray();

            var uri = ConstructExtensions.GetRandomContentUri(playerUris);

            _content_image = new Image()
            {
                Source = new BitmapImage(uriSource: uri),
                HorizontalAlignment = HorizontalAlignment.Center,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(0, 10, 0, 5),
                Height = 110,
                Width = 110,
            };

            Grid.SetRow(_content_image, 0);

            container.Children.Add(_content_image);

            #endregion

            #region Title

            _titleScreenText = new TextBlock()
            {
                Text = "Honk Trooper",
                FontSize = Constants.DEFAULT_GUI_FONT_SIZE,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5),
                Foreground = new SolidColorBrush(Colors.White),                
            };

            Grid.SetRow(_titleScreenText, 1);

            container.Children.Add(_titleScreenText);

            #endregion

            #region Play Button

            Button playButton = new()
            {
                Background = new SolidColorBrush(Colors.Goldenrod),
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE * 3,
                CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS),
                Content = new SymbolIcon()
                {
                    Symbol = Symbol.Play,
                },
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 0, 0, 5),
            };

            playButton.Click += (s, e) =>
            {
                _audioStub.Play(SoundType.GAME_START);
                playAction();
            };

            Grid.SetRow(playButton, 2);

            container.Children.Add(playButton);

            #endregion

            #endregion

            rootGrid.Children.Add(container);
            SetChild(rootGrid);

            _audioStub = new AudioStub((SoundType.GAME_START, 1, false));
        }

        #endregion      

        #region Methods

        public void SetContent(Uri uri)
        {
            _content_image.Source = new BitmapImage(uri);
        }

        public void SetTitle(string title)
        {
            _titleScreenText.Text = title;
        }

        #endregion
    }
}
