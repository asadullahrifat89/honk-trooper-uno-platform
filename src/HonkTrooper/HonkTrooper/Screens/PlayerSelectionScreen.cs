using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class PlayerSelectionScreen : HoveringTitleScreen
    {
        #region Fields

        private readonly TextBlock _titleScreenText;

        private readonly AudioStub _audioStub;

        #endregion

        #region Ctor

        public PlayerSelectionScreen
           (Func<Construct, bool> animateAction,
           Func<Construct, bool> recycleAction,
           Func<int, bool> playAction,
           Func<bool> backAction)
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
                BorderBrush = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(Constants.DEFAULT_CONTROLLER_KEY_BORDER_THICKNESS),
            });

            Grid container = new() { VerticalAlignment = VerticalAlignment.Center, };
            container.RowDefinitions.Add(new RowDefinition());
            container.RowDefinitions.Add(new RowDefinition());
            container.RowDefinitions.Add(new RowDefinition());

            #endregion

            #region Content

            #region Back Button

            Button backButton = new()
            {
                Height = Constants.DEFAULT_CONTROLLER_KEY_SIZE - 10,
                Width = Constants.DEFAULT_CONTROLLER_KEY_SIZE - 10,
                CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS * 2),
                Content = new SymbolIcon()
                {
                    Symbol = Symbol.Back,
                },
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(20),
            };

            backButton.Click += (s, e) =>
            {
                _audioStub.Play(SoundType.OPTION_SELECT);
                backAction();
            };

            Grid.SetRow(backButton, 0);

            rootGrid.Children.Add(backButton);

            #endregion

            #region Title

            _titleScreenText = new TextBlock()
            {
                Text = "Select Player",
                FontSize = Constants.DEFAULT_GUI_FONT_SIZE - 5,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 10, 0, 0),
            };

            Grid.SetRow(_titleScreenText, 0);

            container.Children.Add(_titleScreenText);

            #endregion

            #region Player Select Buttons

            var playerUris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.PLAYER_BALLOON).Select(x => x.Uri).ToArray();

            StackPanel playerTemplates = new()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 5),
            };

            ToggleButton player1btn = new() { Margin = new Thickness(5), CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS - 10) };
            ToggleButton player2btn = new() { Margin = new Thickness(5), CornerRadius = new CornerRadius(Constants.DEFAULT_CONTROLLER_KEY_CORNER_RADIUS - 10) };

            player1btn.Content = new Image()
            {
                Width = 100,
                Height = 100,
                Source = new BitmapImage(playerUris[0]),
                Stretch = Stretch.Uniform
            };
            player1btn.Checked += (s, e) =>
            {
                _audioStub.Play(SoundType.OPTION_SELECT);

                if (player2btn.IsChecked == true)
                    player2btn.IsChecked = false;
            };

            playerTemplates.Children.Add(player1btn);

            player2btn.Content = new Image()
            {
                Width = 100,
                Height = 100,
                Source = new BitmapImage(playerUris[1]),
                Stretch = Stretch.Uniform
            };
            player2btn.Checked += (s, e) =>
            {
                _audioStub.Play(SoundType.OPTION_SELECT);

                if (player1btn.IsChecked == true)
                    player1btn.IsChecked = false;
            };

            playerTemplates.Children.Add(player2btn);

            Grid.SetRow(playerTemplates, 1);

            container.Children.Add(playerTemplates);

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
                playAction(player2btn.IsChecked == true ? 2 : 1);
            };

            Grid.SetRow(playButton, 2);

            container.Children.Add(playButton);

            #endregion

            #endregion

            rootGrid.Children.Add(container);
            SetChild(rootGrid);

            _audioStub = new AudioStub((SoundType.OPTION_SELECT, 1, false), (SoundType.GAME_START, 1, false));
        }

        #endregion
    }
}
