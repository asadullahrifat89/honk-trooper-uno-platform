using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.UI;

namespace HonkPooper
{
    public partial class HudPanel : Border
    {
        private ProgressBar ProgressBar { get; set; } = new()
        {
            Width = 50,
            Height = 5,
            Value = 0,
            Maximum = 0,
            Minimum = 0,
            Margin = new Thickness(0, 0, 5, 0)
        };

        private Image Image { get; set; } = new()
        {
            Height = 40,
            Width = 40,
            Stretch = Stretch.Uniform,
        };

        private StackPanel StackPanel { get; set; }

        public HudPanel()
        {
            this.StackPanel = new()
            {
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
                Orientation = Orientation.Horizontal
            };

            this.StackPanel.Children.Add(Image);
            this.StackPanel.Children.Add(ProgressBar);

            this.Child = this.StackPanel;

            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(10);
            BorderThickness = new Microsoft.UI.Xaml.Thickness(4);
            Background = new SolidColorBrush(Colors.Goldenrod);
            BorderBrush = new SolidColorBrush(Colors.White);

            Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }

        public void SetIcon(Uri uri)
        {
            this.Image.Source = new BitmapImage(uri);
        }

        public void SetMaxiumValue(double value)
        {
            this.ProgressBar.Maximum = value;
        }

        public void SetValue(double value)
        {
            this.ProgressBar.Value = value;

            Visibility = this.ProgressBar.Value > 0 ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        }

        public void SetProgressForegroundColor(Color color)
        {
            this.ProgressBar.Foreground = new SolidColorBrush(color);
        }
    }
}
