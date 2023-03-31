using Windows.Graphics.Display;
using Windows.UI.ViewManagement;

namespace HonkTrooper
{
    public static class ScreenExtensions
    {
        #region Properties

        public static double Width { get; set; }

        public static double Height { get; set; }

        public static DisplayInformation DisplayInformation => DisplayInformation.GetForCurrentView();

        public static ApplicationView ApplicationView => ApplicationView.GetForCurrentView();

        public static DisplayOrientations RequiredScreenOrientation { get; set; }

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

        public static void SetScreenOrientation(DisplayOrientations displayOrientation)
        {
            var currentOrientation = DisplayInformation?.CurrentOrientation;

            LoggingExtensions.Log($"{currentOrientation}");

            if (currentOrientation is not null && currentOrientation != displayOrientation)
                DisplayInformation.AutoRotationPreferences = displayOrientation;
        }

        public static DisplayOrientations? GetScreenOrienation()
        {
            return DisplayInformation?.CurrentOrientation;
        }

        public static double GetScreenSpaceScaling()
        {
            return Width switch
            {
                <= 300 => 0.40,
                <= 400 => 0.45,
                <= 500 => 0.50,

                <= 700 => 0.55,
                <= 900 => 0.60,
                <= 950 => 0.65,

                <= 1000 => 0.85,
                <= 1400 => 0.90,
                <= 1900 => 0.95,
                _ => 1,
            };
        }

#endregion
    }
}
