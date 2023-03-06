using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.UI;

namespace HonkTrooper
{
    public partial class HealthBar : Border
    {
        private ProgressBar Bar { get; set; } = new()
        {
            Width = 50,
            Height = 5,
            Value = 0,
            Maximum = 0,
            Minimum = 0,
            Margin = new Thickness(0, 0, 5, 0)
        };

        private Image Icon { get; set; } = new()
        {
            Height = 40,
            Width = 40,
            Stretch = Stretch.Uniform,
            Margin = new Thickness(5)
        };

        private StackPanel Container { get; set; }

        public HealthBar()
        {
            this.Container = new()
            {
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal
            };

            this.Container.Children.Add(Icon);
            this.Container.Children.Add(Bar);

            this.Child = this.Container;

            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(10);
            BorderThickness = new Microsoft.UI.Xaml.Thickness(4);
            Background = new SolidColorBrush(Colors.Goldenrod);
            BorderBrush = new SolidColorBrush(Colors.White);

            Visibility = Visibility.Collapsed;
        }

        public void SetIcon(Uri uri)
        {
            this.Icon.Source = new BitmapImage(uri);
        }

        public void SetMaxiumHealth(double value)
        {
            this.Bar.Maximum = value;
        }

        public void SetHealth(double value)
        {
            this.Bar.Value = value;
            Visibility = this.Bar.Value > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetBarForegroundColor(Color color)
        {
            this.Bar.Foreground = new SolidColorBrush(color);
        }      
    }
}
