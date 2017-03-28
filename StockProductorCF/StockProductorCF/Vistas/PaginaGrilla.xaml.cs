using Google.GData.Spreadsheets;
using StockProductorCF.Clases;
using StockProductorCF.Servicios;
using System;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace StockProductorCF.Vistas
{
    public partial class PaginaGrilla : ContentPage
    {
        private ServiciosGoogle _servicioGoogle;
        private SpreadsheetsService _servicio;
        private CellFeed _celdas;
        private string _linkHojaConsulta;

        public PaginaGrilla(string linkHojaConsulta, SpreadsheetsService servicio)
        {
            CargaInicial();

            _servicioGoogle = new ServiciosGoogle();
            _servicio = servicio;
            _linkHojaConsulta = linkHojaConsulta;

            ObtenerDatosProductos();
        }

        private void CargaInicial()
        {
            InitializeComponent();
        }
        
        private void ObtenerDatosProductos()
        {
            if (_servicio == null)
                _servicio = _servicioGoogle.ObtenerServicioParaConsultaGoogleSpreadsheets(CuentaUsuario.ObtenerTokenActual());

            _celdas = _servicioGoogle.ObtenerCeldasDeUnaHoja(_linkHojaConsulta, _servicio);

            try
            {
                string[][] productos = new string[_celdas.RowCount.Count - 1][];

                foreach (CellEntry celda in _celdas.Entries)
                {
                    if (celda.Row != 1)
                    {
                        if (celda.Column == 1)
                            productos[celda.Row - 2] = new string[_celdas.ColCount.Count];

                        productos[celda.Row - 2].SetValue(celda.Value, (int)celda.Column - 1);
                    }
                    else
                    {

                    }
                }

                LlenarGrillaProductos(productos);
            }
            catch (Exception)
            {
                Navigation.PushAsync(new AccesoDatos());
            }
        }

        private void LlenarGrillaProductos(string[][] productos)
        {
            ContenedorTabla.Children.Clear();

            StackLayout itemProducto;
            Label etiquetaProducto;
            var esGris = false;
            foreach (string[] producto in productos)
            {
                etiquetaProducto = new Label
                {
                    TextColor = Color.Black,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    HorizontalTextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Center
                };
                foreach (string dato in producto)
                {
                    etiquetaProducto.Text += " " + dato + " -";
                }
                etiquetaProducto.Text.TrimEnd('-');

                itemProducto = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    Orientation = StackOrientation.Horizontal,
                    HeightRequest = 50,
                    Children = { etiquetaProducto },
                    Spacing = 0,
                    Padding = 0,
                    Margin = 2,
                    BackgroundColor = esGris ? Color.Silver : Color.White,
                    GestureRecognizers = { new TapGestureRecognizer { Command = new Command(CargarProducto), CommandParameter = producto[0] } }
                };

                esGris = !esGris;
                ContenedorTabla.Children.Add(itemProducto);
            }
        }

        private void CargarProducto(object codigoProducto)
        {
            int fila = -1;
            CellEntry[] producto = new CellEntry[_celdas.ColCount.Count];
            CellEntry[] nombresColumnas = new CellEntry[_celdas.ColCount.Count];

            foreach (CellEntry celda in _celdas.Entries)
            {
                if (celda.Row == 1)
                {
                    nombresColumnas.SetValue(celda, (int)celda.Column - 1);
                }
                if (celda.Value == codigoProducto.ToString())
                    fila = (int)celda.Row;
                if (celda.Row == fila)
                    producto.SetValue(celda, (int)celda.Column - 1);
                if (fila > -1 && celda.Row > fila)
                    break;
            }
            
            Navigation.PushAsync(new Producto(producto, nombresColumnas, _servicio, _servicioGoogle));
        }

        [Android.Runtime.Preserve]
        async void AbrirPaginaEscaner(object sender, EventArgs args)
        {
            var paginaEscaner = new ZXingScannerPage();

            paginaEscaner.OnScanResult += (result) => {
                // Detiene el escaner
                paginaEscaner.IsScanning = false;

                // Cierra la página del escaner y llama a la página del producto
                Device.BeginInvokeOnMainThread(() => {
                    Navigation.PopModalAsync();
                    CargarProducto(result.Text);
                });
            };

            // Abre la página del escaner
            await Navigation.PushModalAsync(paginaEscaner);
        }

        [Android.Runtime.Preserve]
        void AccederDatos(object sender, EventArgs args)
        {
            var paginaAccesoDatos = new AccesoDatos();

            Navigation.PushAsync(paginaAccesoDatos);
        }

        [Android.Runtime.Preserve]
        void RefrescarDatos(object sender, EventArgs args)
        {
            ObtenerDatosProductos();
        }
    }
}
