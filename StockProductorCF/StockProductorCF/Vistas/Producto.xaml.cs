
using System;
using Google.GData.Spreadsheets;
using Xamarin.Forms;
using StockProductorCF.Servicios;

namespace StockProductorCF.Vistas
{
    public partial class Producto : ContentPage
    {
        private bool _signoPositivo5 = true;
        private bool _signoPositivo6 = true;
        private SpreadsheetsService _servicio;
        private CellEntry[] _producto;
        private ServiciosGoogle _servicioGoogle;

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
                    WidthRequest = 180
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
                if (celda != null && (celda.Column == 5 || celda.Column == 6))
                {
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
                        FontSize = 25
                    };

                    if (celda.Column == 5)
                    {
                        botonSigno.Clicked += DefinirSigno5;
                    }
                    else
                    {
                        botonSigno.Clicked += DefinirSigno6;
                    }

                    movimiento = new Entry()
                    {
                        HorizontalOptions = LayoutOptions.StartAndExpand,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Start,
                        StyleId = "movimiento" + celda.Column,
                        WidthRequest = 100
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

        private void DefinirSigno5(object sender, EventArgs e)
        {
            ((Button)sender).Text = _signoPositivo5 ? "-" : "+";
            _signoPositivo5 = ((Button)sender).Text == "+";
        }
        private void DefinirSigno6(object sender, EventArgs e)
        {
            ((Button)sender).Text = _signoPositivo6 ? "-" : "+";
            _signoPositivo6 = ((Button)sender).Text == "+";
        }

        [Android.Runtime.Preserve]
        void GuardarCambios(object sender, EventArgs args)
        {
            var movimiento5 = 0.00;
            var movimiento6 = 0.00;
            foreach (View stackLayout in ContenedorProducto.Children)
            {
                foreach (View control in ((StackLayout)stackLayout).Children)
                {
                    if (control.StyleId == "movimiento5")
                        movimiento5 = Convert.ToDouble(((Entry)control).Text);
                    if (control.StyleId == "movimiento6")
                        movimiento6 = Convert.ToDouble(((Entry)control).Text);
                }
            }

            int multiplicador5 = _signoPositivo5 ? 1 : -1;
            int multiplicador6 = _signoPositivo6 ? 1 : -1;
            foreach (CellEntry celda in _producto)
            {
                if (celda != null && celda.Column == 5)
                {
                    celda.InputValue = (Convert.ToDouble(celda.InputValue) + multiplicador5 * movimiento5).ToString();
                    _servicioGoogle.ActualizarCelda(_servicio, celda);
                }

                if (celda != null && celda.Column == 6)
                {
                    celda.InputValue = (Convert.ToDouble(celda.InputValue) + multiplicador6 * movimiento6).ToString();
                    _servicioGoogle.ActualizarCelda(_servicio, celda);
                }
            }
            
            Navigation.PopAsync();
        }
    }
}
