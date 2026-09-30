using Android.App;
using Android.Content.PM;
using Android.OS;

namespace TrainTracker.App;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        // شريط الحالة بنفس لون الهيدر
#pragma warning disable CA1422
        Window?.SetStatusBarColor(Android.Graphics.Color.ParseColor("#12263A"));
#pragma warning restore CA1422
    }
}
