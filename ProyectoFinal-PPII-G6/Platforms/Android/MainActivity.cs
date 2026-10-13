using Android.App;
using Android.Content.PM;
using Android.OS;

namespace ProyectoFinal_PPII_G6
{
    /// <summary>
    /// Actividad principal de Android que aloja la aplicación MAUI.
    /// </summary>
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
    }
}
