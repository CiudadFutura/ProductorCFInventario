using System;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace StockProductorCF.Vistas
{
    public partial class PaginaGrilla : ContentPage
    {
        public PaginaGrilla(string listaHojas)
        {
            InitializeComponent();
            Cabecera.Source = ImageSource.FromResource("StockProductorCF.Imagenes.ciudadFutura.png");
            Grilla.Text = listaHojas;
        }

        async void AbrirPaginaEscaner(object sender, EventArgs args)
        {
            var paginaEscaner = new ZXingScannerPage();

            paginaEscaner.OnScanResult += (result) => {
                // Stop scanning
                paginaEscaner.IsScanning = false;

                // Pop the page and show the result
                Device.BeginInvokeOnMainThread(() => {
                    Navigation.PopModalAsync();
                    DisplayAlert("Scanned Barcode", result.Text, "OK");
                });
            };

            // Navigate to our scanner page
            await Navigation.PushModalAsync(paginaEscaner);
        }
    }
}
