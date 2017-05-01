﻿using Android.App;
using Android.Content.PM;
using Android.OS;

namespace StockProductorCF.Droid
{
    [Activity(Label = "Inventario Productor Ciudad Futura", Icon = "@drawable/icon", Theme = "@style/MainTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);
            
            ZXing.Net.Mobile.Forms.Android.Platform.Init();

            global::Xamarin.Forms.Forms.Init(this, bundle);

            App.AlmacenarAnchoPantalla((int)(Resources.DisplayMetrics.WidthPixels / Resources.DisplayMetrics.Density), Resources.DisplayMetrics.WidthPixels);

						LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            global::ZXing.Net.Mobile.Forms.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        
    }
    
}

