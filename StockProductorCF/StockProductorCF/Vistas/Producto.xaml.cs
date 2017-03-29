
using System;
using Google.GData.Spreadsheets;
using Xamarin.Forms;
using StockProductorCF.Servicios;
using StockProductorCF.Clases;

namespace StockProductorCF.Vistas
{
    public partial class Producto : ContentPage
    {
        private bool[] _signoPositivo;
        private double[] _movimientos;
        private SpreadsheetsService _servicio;
        private CellEntry[] _producto;
        private ServiciosGoogle _servicioGoogle;
        private string[] _listaColumnasInventario;

        public Producto(CellEntry[] producto, CellEntry[] nombresColumnas, SpreadsheetsService servicio, ServiciosGoogle servicioGoogle)
        {
            InitializeComponent();

            _producto = producto;
            _servicio = servicio;
            _servicioGoogle = servicioGoogle;

            CargarDatosProductos(nombresColumnas);
        }

        private void CargarDatosProductos(CellEntry[] nombresColumnas)
        {
            Label nombreCampo;
            Entry valorCampo;
            StackLayout campoValor;

            Button botonSigno;
            Entry movimiento;

            var columnasInventario = CuentaUsuario.ObtenerColumnasInventario();
            _listaColumnasInventario = null;
            if (!string.IsNullOrEmpty(columnasInventario))
                _listaColumnasInventario = columnasInventario.Split(',');

            _signoPositivo = new bool[_producto.Length];
            _movimientos = new double[_producto.Length];

            foreach (CellEntry celda in _producto)
            {
                nombreCampo = new Label()
                {
                    HorizontalOptions = LayoutOptions.EndAndExpand,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.End,
                    FontSize = 16,
                    WidthRequest = 130
                };

                foreach (CellEntry nombreColumna in nombresColumnas)
                {
                    if(celda != null && nombreColumna.Column == celda.Column)
                        nombreCampo.Text = nombreColumna.Value;
                }
                
                valorCampo = new Entry()
                {
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Start,
                    WidthRequest = 180,
                    IsEnabled = false
                };
                valorCampo.Text = celda != null ? celda.Value : "";

                campoValor = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    Orientation = StackOrientation.Horizontal,
                    HeightRequest = 50,
                    Children = { nombreCampo, valorCampo }
                };

                ContenedorProducto.Children.Add(campoValor);

                //REEMPLAZAR
                if (celda != null && _listaColumnasInventario != null && _listaColumnasInventario[(int)celda.Column - 1] == "1")
                {
                    _signoPositivo.SetValue(true, (int)celda.Column - 1);

                    nombreCampo = new Label()
                    {
                        HorizontalOptions = LayoutOptions.EndAndExpand,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.End,
                        Text = "Movimiento",
                        FontSize = 16,
                        WidthRequest = 130
                    };

                    botonSigno = new Button()
                    {
                        Text = "+",
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Center,
                        FontSize = 25,
                        StyleId = celda.Column.ToString()
                    };

                    botonSigno.Clicked += DefinirSigno;

                    movimiento = new Entry()
                    {
                        HorizontalOptions = LayoutOptions.StartAndExpand,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Start,
                        StyleId = "movimiento-" + celda.Column.ToString(),
                        WidthRequest = 100,
                        Keyboard = Keyboard.Numeric
                    };

                    campoValor = new StackLayout
                    {
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        Orientation = StackOrientation.Horizontal,
                        HeightRequest = 50,
                        Children = { nombreCampo, botonSigno, movimiento }
                    };

                    ContenedorProducto.Children.Add(campoValor);
                }
                
                
            }
            
        }

        private void DefinirSigno(object sender, EventArgs e)
        {
            Button boton = ((Button)sender);
            boton.Text = _signoPositivo[Convert.ToInt32(boton.StyleId) - 1] ? "-" : "+";
            _signoPositivo.SetValue(boton.Text == "+", Convert.ToInt32(boton.StyleId) - 1);
        }

        [Android.Runtime.Preserve]
        void GuardarCambios(object sender, EventArgs args)
        {
            foreach (View stackLayout in ContenedorProducto.Children)
            {
                foreach (View control in ((StackLayout)stackLayout).Children)
                {
                    if (control.StyleId != null && control.StyleId.Contains("movimiento-"))
                    {
                        var columna = Convert.ToInt32(control.StyleId.Split('-')[1]);
                        var valor = Convert.ToDouble(((Entry)control).Text);
                        _movimientos.SetValue(valor, columna - 1);
                    }
                }
            }

            var multiplicador = 1;
            var movimiento = 0.00;
            foreach (CellEntry celda in _producto)
            {
                if (_listaColumnasInventario[(int)celda.Column - 1] == "1")
                {
                    multiplicador = _signoPositivo[(int)celda.Column - 1] ? 1 : -1;
                    movimiento = _movimientos[(int)celda.Column - 1];

                    celda.InputValue = (Convert.ToDouble(celda.InputValue) + multiplicador * movimiento).ToString();
                    _servicioGoogle.ActualizarCelda(_servicio, celda);
                }
            }
            
            Navigation.PopAsync();
        }
    }
}
