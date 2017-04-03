
using Google.GData.Spreadsheets;
using StockProductorCF.Clases;
using StockProductorCF.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private string[] _listaColumnasParaVer;
        private string[] _listacolumnasInventario;
        private bool _seleccionoItem;
        private ViewCell _ultimoItemSeleccionado;

        public PaginaGrilla(string linkHojaConsulta, SpreadsheetsService servicio)
        {
            InitializeComponent();

            _servicioGoogle = new ServiciosGoogle();
            _servicio = servicio;
            _linkHojaConsulta = linkHojaConsulta;

            var columnasParaVer = CuentaUsuario.ObtenerColumnasParaVer();
            if (!string.IsNullOrEmpty(columnasParaVer))
                _listaColumnasParaVer = columnasParaVer.Split(',');

            var columnasInventario = CuentaUsuario.ObtenerColumnasInventario();
            _listacolumnasInventario = null;
            if (!string.IsNullOrEmpty(columnasInventario))
                _listacolumnasInventario = columnasInventario.Split(',');

            Datos.WidthRequest = App.AnchoDePantalla / 3;
            Refrescar.WidthRequest = App.AnchoDePantalla / 3;
            Escanear.WidthRequest = App.AnchoDePantalla / 3;

            ObtenerDatosProductos();
        }

        private async void ObtenerDatosProductos()
        {
            if (_servicio == null) //El servicio viene nulo cuando se llama directamente desde el lanzador (ya tiene conexión a datos configurada)
                _servicio = _servicioGoogle.ObtenerServicioParaConsultaGoogleSpreadsheets(CuentaUsuario.ObtenerTokenActual());
            
            try
            {
                _celdas = _servicioGoogle.ObtenerCeldasDeUnaHoja(_linkHojaConsulta, _servicio);
            }
            catch (Exception)
            {
                //Si se quedó la pantalla abierta un largo tiempo y se venció el token, se cierra y refresca el token
                var paginaAuntenticacion = new PaginaAuntenticacion(true);
                Navigation.InsertPageBefore(paginaAuntenticacion, this);
                await Navigation.PopAsync();
            }

            ContenedorTabla.Children.Clear();

            _nombresColumnas = new CellEntry[_celdas.ColCount.Count];

            IList<string[]> productos = new string[_celdas.RowCount.Count - 1][];

            foreach (CellEntry celda in _celdas.Entries)
            {
                if (celda.Row != 1)
                {
                    if (celda.Column == 1)
                        productos[(int)celda.Row - 2] = new string[_celdas.ColCount.Count];

                    productos[(int)celda.Row - 2].SetValue(celda.Value, (int)celda.Column - 1);
                }
                else
                {
                    _nombresColumnas.SetValue(celda, (int)celda.Column - 1);
                }
            }

            LlenarGrillaProductos(productos);
        }

        private void LlenarGrillaProductos(IList<string[]> productos)
        {
            var lista = ConstruirVistaDeLista(productos.ToList());
            ContenedorTabla.Children.Add(lista);
        }

        [Android.Runtime.Preserve]
        void CargarProducto(object sender, EventArgs args)
        {
            if (!_seleccionoItem)
            {
                var Id = ((ClaseProducto)((ListView)sender).SelectedItem).ID;
                IrAlProducto(Id);
                ((ListView)sender).SelectedItem = null;
                _seleccionoItem = true;
            }
            else
                _seleccionoItem = false;
        }

        private void IrAlProducto(string codigoProducto)
        {
            int fila = -1;
            CellEntry[] producto = new CellEntry[_celdas.ColCount.Count];

            //Obtener el arreglo del producto para enviar
            foreach (CellEntry celda in _celdas.Entries)
            {
                if (celda.Column == 1 && celda.Value == codigoProducto.ToString())
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

            paginaEscaner.OnScanResult += (result) =>
            {
                // Detiene el escaner
                paginaEscaner.IsScanning = false;

                // Cierra la página del escaner y llama a la página del producto
                Device.BeginInvokeOnMainThread(() =>
                {
                    Navigation.PopModalAsync();
                    IrAlProducto(result.Text);
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
            ContenedorTabla.Children.Add(new ActivityIndicator
            {
                Color = Color.Red,
                IsRunning = true,
                HeightRequest = 60,
                WidthRequest = 60
            });
            ObtenerDatosProductos();
        }

        private StackLayout ConstruirVistaDeLista(List<string[]> productos)
        {
            List<ClaseProducto> listaProductos = new List<ClaseProducto>();
            ClaseProducto producto;
            List<string> datosParaVer;
            foreach (string[] datosProducto in productos)
            {
                datosParaVer = new List<string>();
                var i = 0;
                string textoDato;
                foreach (string dato in datosProducto)
                {
                    textoDato = "";

                    if (_listaColumnasParaVer != null && _listaColumnasParaVer[i] == "1")
                    {
                        if (_listacolumnasInventario[i] == "1")
                            textoDato += _nombresColumnas[i].Value + " : ";

                        textoDato += dato;
                        datosParaVer.Add(textoDato);
                    }
                    i += 1;
                }

                producto = new ClaseProducto(datosProducto[0], datosParaVer);
                listaProductos.Add(producto);
            }
            
            StackLayout Encabezado = new StackLayout
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Start,
                BackgroundColor = Color.Gray,
                Children =
                {
                    new Label
                    {
                        Text = "Productos",
                        FontSize = 18,
                        HorizontalOptions = LayoutOptions.Center,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.White
                    }
                }
            };

            ListView vista = new ListView
            {
                RowHeight = 60,
                SeparatorColor = Color.Black,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.Fill,
                ItemsSource = listaProductos,
                ItemTemplate = new DataTemplate(() =>
                {
                    Label nombreProducto = new Label
                    {
                        FontSize = 16,
                        FontAttributes = FontAttributes.Bold,
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        WidthRequest = 110
                    };
                    nombreProducto.SetBinding(Label.TextProperty, "Nombre");

                    Label datos = new Label()
                    {
                        FontSize = 15,
                        TextColor = Color.Black,
                        VerticalOptions = LayoutOptions.CenterAndExpand
                    };
                    datos.SetBinding(Label.TextProperty, "Datos");

                    BoxView cuadradito = new BoxView()
                    {
                        WidthRequest = 5,
                        BackgroundColor = Color.Red
                    };

                    BoxView separador = new BoxView()
                    {
                        WidthRequest = 1,
                        BackgroundColor = Color.FromHex("#E0E0E0"),
                        HeightRequest = 55
                    };

                    // Return an assembled ViewCell.
                    var celda = new ViewCell
                    {
                        View = new StackLayout
                        {
                            Padding = 2,
                            Orientation = StackOrientation.Horizontal,
                            BackgroundColor = Color.White,
                            Children =
                            {
                                cuadradito,
                                nombreProducto,
                                separador,
                                new StackLayout
                                {
                                    Orientation = StackOrientation.Vertical,
                                    Spacing = 0,
                                    Children =
                                    {
                                        datos
                                    }
                                }
                            }
                        }
                    };

                    celda.SetBinding(ViewCell.ClassIdProperty, "ID");
                    celda.Tapped += (sender, args) => {
                        if(_ultimoItemSeleccionado != null)
                            _ultimoItemSeleccionado.View.BackgroundColor = Color.White;
                        IrAlProducto(((ViewCell)sender).ClassId);
                        celda.View.BackgroundColor = Color.Silver;
                        _ultimoItemSeleccionado = ((ViewCell)sender);
                    };

                    return celda;
                })
            };
            
            // Build the page.
            return new StackLayout
            {
                Spacing = 0,
                Padding = 0,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.Fill,
                Children =
                {
                    Encabezado,
                    vista
                }
            };
        }

    }

    [Android.Runtime.Preserve]
    public class ClaseProducto
    {
        [Android.Runtime.Preserve]
        public ClaseProducto(string id, IList<string> datos)
        {
            ID = id;
            Nombre = datos[0];
            Datos = string.Join(" - ", datos.Skip(1).Take(datos.Count));
        }

        [Android.Runtime.Preserve]
        public string ID { private set; get; }
        [Android.Runtime.Preserve]
        public string Nombre { private set; get; }
        [Android.Runtime.Preserve]
        public string Datos { private set; get; }
    };

    
}
