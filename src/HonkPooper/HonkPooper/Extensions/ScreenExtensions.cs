using System;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;

namespace HonkPooper
{
    public static class ScreenExtensions
    {
        #region Properties

        public static DisplayInformation DisplayInformation => DisplayInformation.GetForCurrentView();

        public static ApplicationView ApplicationView => ApplicationView.GetForCurrentView();

        public static DisplayOrientations RequiredDisplayOrientation { get; set; } 

        #endregion

        #region Methods

        public static void EnterFullScreen(bool toggleFullScreen)
        {
            //#if !DEBUG
            if (ApplicationView is not null)
            {
                if (toggleFullScreen)
                {
                    ApplicationView.TryEnterFullScreenMode();
                }
                else
                {
                    ApplicationView.ExitFullScreenMode();
                }
            }
            //#endif
        }

        public static void SetDisplayOrientation(DisplayOrientations displayOrientation)
        {
            var currentOrientation = DisplayInformation?.CurrentOrientation;

            Console.WriteLine($"{currentOrientation}");

            if (currentOrientation is not null && currentOrientation != displayOrientation)
                DisplayInformation.AutoRotationPreferences = displayOrientation;
        }

        public static DisplayOrientations? GetDisplayOrienation()
        {
            return DisplayInformation?.CurrentOrientation;
        }

        /// <summary>
        /// Gets the down scaling factor according to window size.
        /// </summary>
        /// <param name="windowWidth"></param>
        /// <returns></returns>
        public static double GetDownScaling(double windowWidth)
        {
            return windowWidth switch
            {
                <= 300 => 0.50,
                <= 400 => 0.55,
                <= 500 => 0.60,
                <= 700 => 0.65,
                <= 900 => 0.70,
                <= 950 => 0.75,
                <= 1000 => 0.80,
                <= 1400 => 0.85,
                <= 2000 => 0.90,
                _ => 1,
            };
        }

        #endregion
    }
}
