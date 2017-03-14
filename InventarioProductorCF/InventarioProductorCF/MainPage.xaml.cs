using System;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace InventarioProductorCF
{
    public partial class MainPage : ContentPage
    {
        ZXingScannerPage scanPage;
        public MainPage()
        {
            InitializeComponent();
        }

        async void AbrirPaginaEscaner(object sender, EventArgs args)
        {
            var scanPage = new ZXingScannerPage();

            scanPage.OnScanResult += (result) => {
                // Stop scanning
                scanPage.IsScanning = false;

                // Pop the page and show the result
                Device.BeginInvokeOnMainThread(() => {
                    Navigation.PopModalAsync();
                    DisplayAlert("Scanned Barcode", result.Text, "OK");
                });
            };

            // Navigate to our scanner page
            await Navigation.PushModalAsync(scanPage);
        }

       
    }
}
