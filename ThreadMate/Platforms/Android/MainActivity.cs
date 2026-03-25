using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace ThreadMate
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Window is null)
            {
                return;
            }

            var yellow = Android.Graphics.Color.Rgb(255, 212, 0);
            Window.SetStatusBarColor(yellow);
            Window.SetNavigationBarColor(yellow);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                var uiFlags = (StatusBarVisibility)Window.DecorView.SystemUiVisibility;
                uiFlags |= (StatusBarVisibility)SystemUiFlags.LightStatusBar;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    uiFlags |= (StatusBarVisibility)SystemUiFlags.LightNavigationBar;
                }

                Window.DecorView.SystemUiVisibility = uiFlags;
            }
        }
    }
}
