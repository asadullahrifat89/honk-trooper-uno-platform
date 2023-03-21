﻿using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System;
using System.Linq;

namespace HonkTrooper
{
    public partial class Road : Construct
    {
        #region Ctor

        public Road(
            Func<Construct, bool> animateAction,
            Func<Construct, bool> recycleAction,
            double downScaling)
        {
            ConstructType = ConstructType.ROAD;

            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.ROAD);

            var width = size.Width * downScaling;
            var height = size.Height * downScaling;

            SetSize(width: width, height: height);

            AnimateAction = animateAction;
            RecycleAction = recycleAction;

            Background = App.Current.Resources["RoadForegroundColor"] as SolidColorBrush;
            CornerRadius = new CornerRadius(5);

            SetSkewY(42);
            SetRotation(-63.5);

            IsometricDisplacement = Constants.DEFAULT_ISOMETRIC_DISPLACEMENT;
            SpeedOffset = Constants.DEFAULT_SPEED_OFFSET;
        }

        #endregion
    }
}
