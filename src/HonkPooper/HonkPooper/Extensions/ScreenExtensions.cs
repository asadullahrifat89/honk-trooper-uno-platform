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

        #endregion
    }
}
