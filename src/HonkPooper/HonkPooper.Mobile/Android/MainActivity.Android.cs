using Android.App;
using Android.Views;

namespace HonkPooper
{
    [Activity(
            MainLauncher = true,
            ConfigurationChanges = Uno.UI.ActivityHelper.AllConfigChanges,
            WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden
        )]
    public class MainActivity : Microsoft.UI.Xaml.ApplicationActivity
    {
    }
}

