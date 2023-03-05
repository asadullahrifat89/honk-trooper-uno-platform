using Windows.UI.ViewManagement;

namespace HonkPooper
{
    public static class ScreenExtensions 
    {
        #region Methods

        public static void EnterFullScreen(bool toggleFullScreen)
        {
            //#if !DEBUG
            var view = ApplicationView.GetForCurrentView();

            if (view is not null)
            {
                if (toggleFullScreen)
                {
                    view.TryEnterFullScreenMode();
                }
                else
                {
                    view.ExitFullScreenMode();
                }
            }
            //#endif
        }

        #endregion
    }
}
