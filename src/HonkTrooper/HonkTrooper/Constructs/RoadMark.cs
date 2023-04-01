﻿using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class RoadMark : MovableConstruct
    {
        #region Fields

        private readonly ImageEx _content_image;
        private readonly BitmapImage _bitmapImage;

        private readonly Uri[] _tree_uris;

        #endregion

        #region Ctor

        public RoadMark(
            Action<Construct> animateAction,
            Action<Construct> recycleAction)
        {
            ConstructType = ConstructType.ROAD_MARK;

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            _tree_uris = Constants.CONSTRUCT_TEMPLATES.Where(x => x.ConstructType == ConstructType.ROAD_MARK).Select(x => x.Uri).ToArray();

            SetConstructSize();

            var uri = ConstructExtensions.GetRandomContentUri(_tree_uris);
            _bitmapImage = new BitmapImage(uriSource: uri);

            _content_image = new()
            {
                Source = _bitmapImage,
                Height = this.Height,
                Width = this.Width,
                IsCacheEnabled = true,
            };

            SetChild(_content_image);

            SetSkewY(36);
            SetRotation(-63.5);

            Speed = Constants.DEFAULT_CONSTRUCT_SPEED;
            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
        }

        #endregion
    }
}
