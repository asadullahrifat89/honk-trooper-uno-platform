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
        #region Fields

        private ProgressBar _bar = new()
        {
            Width = 60,
            Height = 5,
            Value = 0,
            Maximum = 0,
            Minimum = 0,
            Margin = new Thickness(0, 0, 5, 0)
        };

        private Image _icon = new()
        {
            Height = 30,
            Width = 30,
            Stretch = Stretch.Uniform,
            Margin = new Thickness(5)
        };

        private StackPanel _container;

        #endregion

        #region Ctor

        public HealthBar()
        {
            VerticalAlignment = VerticalAlignment.Center;
            HorizontalAlignment = HorizontalAlignment.Center;

            this._container = new()
            {
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal
            };

            this._container.Children.Add(_icon);
            this._container.Children.Add(_bar);

            this.Child = this._container;

            CornerRadius = new CornerRadius(10);
            BorderThickness = new Thickness(4);
            Background = new SolidColorBrush(Colors.Goldenrod);
            BorderBrush = new SolidColorBrush(Colors.White);

            Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Properties

        public bool HasHealth => this._bar.Value > 0;

        #endregion

        #region Methods

        public void SetIcon(Uri uri)
        {
            this._icon.Source = new BitmapImage(uri);
        }

        public void SetMaxiumHealth(double value)
        {
            this._bar.Maximum = value;
        }

        public void UpdateValue(double value)
        {
            this._bar.Value = value;
            Visibility = this._bar.Value > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetBarForegroundColor(Color color)
        {
            this._bar.Foreground = new SolidColorBrush(color);
        }

        public double GetValue()
        {
            return this._bar.Value;
        }

        #endregion
    }
}
