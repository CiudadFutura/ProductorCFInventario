
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using StockProductorCF.Droid;

[Activity(Label = "Inventario de la MAI", MainLauncher = true, NoHistory = true, Theme = "@style/Theme.Lanzamiento", 
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
public class pantallaLanzamiento : Activity
{
    protected override void OnCreate(Bundle bundle)
    {
        base.OnCreate(bundle);

        var intent = new Intent(this, typeof(MainActivity));
        StartActivity(intent);
        Finish();
    }
}