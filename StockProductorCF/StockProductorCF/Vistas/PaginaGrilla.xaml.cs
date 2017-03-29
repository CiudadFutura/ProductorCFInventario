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
        private CellEntry[] _nombresColumnas;

        public PaginaGrilla(string linkHojaConsulta, SpreadsheetsService servicio)
        {
            InitializeComponent();

            _servicioGoogle = new ServiciosGoogle();
            _servicio = servicio;
            _linkHojaConsulta = linkHojaConsulta;

            ObtenerDatosProductos();
        }
                
        private void ObtenerDatosProductos()
        {
            if (_servicio == null)
                _servicio = _servicioGoogle.ObtenerServicioParaConsultaGoogleSpreadsheets(CuentaUsuario.ObtenerTokenActual());

            _celdas = _servicioGoogle.ObtenerCeldasDeUnaHoja(_linkHojaConsulta, _servicio);

            _nombresColumnas = new CellEntry[_celdas.ColCount.Count];

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
                        _nombresColumnas.SetValue(celda, (int)celda.Column - 1);
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

            var columnasParaVer = CuentaUsuario.ObtenerColumnasParaVer();
            string[] listaColumnasParaVer = null;
            if (!string.IsNullOrEmpty(columnasParaVer))
                listaColumnasParaVer = columnasParaVer.Split(',');

            var columnasInventario = CuentaUsuario.ObtenerColumnasInventario();
            string[] listacolumnasInventario = null;
            if (!string.IsNullOrEmpty(columnasInventario))
                listacolumnasInventario = columnasInventario.Split(',');

            foreach (string[] producto in productos)
            {
                etiquetaProducto = new Label
                {
                    TextColor = Color.Black,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    HorizontalTextAlignment = TextAlignment.Start,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 16
                };

                var i = 0;
                
                foreach (string dato in producto)
                {
                    if (listacolumnasInventario[i] == "1")
                        etiquetaProducto.Text += " " + _nombresColumnas[i].Value + " :";

                    if (listaColumnasParaVer != null && listaColumnasParaVer[i] == "1")
                        etiquetaProducto.Text += " " + dato + " -";
                    i += 1;
                }
                etiquetaProducto.Text = etiquetaProducto.Text.TrimEnd('-');

                itemProducto = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    Orientation = StackOrientation.Horizontal,
                    HeightRequest = 55,
                    Children = { etiquetaProducto },
                    Spacing = 0,
                    Padding = 2,
                    Margin = 1,
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

            foreach (CellEntry celda in _celdas.Entries)
            {
                if (celda.Value == codigoProducto.ToString())
                    fila = (int)celda.Row;
                if (celda.Row == fila)
                    producto.SetValue(celda, (int)celda.Column - 1);
                if (fila > -1 && celda.Row > fila)
                    break;
            }
            
            Navigation.PushAsync(new Producto(producto, _nombresColumnas, _servicio, _servicioGoogle));
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
            ContenedorTabla.Children.Clear();
            ContenedorTabla.Children.Add(new ActivityIndicator {
                Color = Color.Red,
                IsRunning = true,
                HeightRequest = 60,
                WidthRequest = 60
            });
            ObtenerDatosProductos();
        }
    }
}
