﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace HonkTrooper
{
    public partial class ImgageElement : Image
    {
        private readonly BitmapImage _bitmapImage;

        public ImgageElement(Uri uri, double width, double height)
        {
            _bitmapImage = new BitmapImage(uriSource: uri);
            Source = _bitmapImage;
            Width = width;
            Height = height;
            CanDrag = false;
        }

        public void SetSource(Uri uri)
        {
            _bitmapImage.UriSource = uri;
        }
    }
}
