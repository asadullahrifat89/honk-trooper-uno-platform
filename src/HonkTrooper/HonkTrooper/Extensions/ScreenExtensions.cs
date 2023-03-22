using Windows.Graphics.Display;
using Windows.UI.ViewManagement;

namespace HonkTrooper
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

            LoggerExtensions.Log($"{currentOrientation}");

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
        public static double GetScreenSpaceScaling(double windowWidth)
        {
            return windowWidth switch
            {
                <= 300 => 0.40,
                <= 400 => 0.45,
                <= 500 => 0.50,
                <= 700 => 0.55,
                <= 900 => 0.60,
                <= 950 => 0.65,
                <= 1000 => 0.80,
                <= 1400 => 0.85,
                <= 1900 => 0.90,
                _ => 1,
            };
        }

        #endregion
    }
}
