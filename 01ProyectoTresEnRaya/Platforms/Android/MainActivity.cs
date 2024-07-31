using Android.App;
using Android.Content.PM;
using Android.OS;

namespace _01ProyectoTresEnRaya
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.SensorPortrait)] 
    public class MainActivity : MauiAppCompatActivity
    {
    }
}
