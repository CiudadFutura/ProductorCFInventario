
using System;
using Google.GData.Spreadsheets;
using Xamarin.Forms;
using StockProductorCF.Servicios;
using StockProductorCF.Clases;
using System.Linq;

namespace StockProductorCF.Vistas
{
    public partial class SeleccionColumnasParaVer : ContentPage
    {
        private SpreadsheetsService _servicio;
        private int[] _listaColumnas;
        private CellEntry[] _columnas;
        private string _linkHojaConsulta;

        public SeleccionColumnasParaVer(string linkHojaConsulta, SpreadsheetsService servicio)
        {
            InitializeComponent();

            _servicio = servicio;
            _linkHojaConsulta = linkHojaConsulta;

            ObtenerColumnas();
        }

        private void ObtenerColumnas()
        {
            try
            {
                var celdas = new ServiciosGoogle().ObtenerCeldasDeUnaHoja(_linkHojaConsulta, _servicio);

                _columnas = new CellEntry[celdas.ColCount.Count];
                _listaColumnas = Enumerable.Repeat(1, (int)celdas.ColCount.Count).ToArray();

                foreach (CellEntry celda in celdas.Entries)
                {
                    if (celda.Row == 1)
                    {
                        _columnas.SetValue(celda, (int)celda.Column - 1);
                    }
                    else
                    {
                        break;
                    }
                }

                LlenarGrillaColumnasParaVer(_columnas);
            }
            catch (Exception)
            {
                Navigation.PushAsync(new AccesoDatos());
            }
        }

        private void LlenarGrillaColumnasParaVer(CellEntry[] columnas)
        {
            StackLayout itemColumna;
            Label etiquetaColumna;
            Switch seleccionar;
            var esGris = false;
            foreach (CellEntry columna in columnas)
            {
                etiquetaColumna = new Label
                {
                    TextColor = Color.Black,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    HorizontalTextAlignment = TextAlignment.Start,
                    VerticalOptions = LayoutOptions.Center,
                    Text = columna.Value,
                    FontSize = 18
                };

                seleccionar = new Switch
                {
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center,
                    StyleId = columna.Column.ToString()
                };
                seleccionar.Toggled += AgregarColumna;

                itemColumna = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    Orientation = StackOrientation.Horizontal,
                    HeightRequest = 45,
                    WidthRequest = 300,
                    Children = { etiquetaColumna, seleccionar },
                    Spacing = 0,
                    Padding = 4,
                    BackgroundColor = esGris ? Color.Silver : Color.FromHex("#F5F5F5")
                };

                esGris = !esGris;
                ContenedorColumnas.Children.Add(itemColumna);
            }
        }

        [Android.Runtime.Preserve]
        private void AgregarColumna(object sender, ToggledEventArgs e)
        {
            Switch ficha = (Switch)sender;
            _listaColumnas.SetValue(Convert.ToInt32(!e.Value), Convert.ToInt32(ficha.StyleId) - 1);
        }

        [Android.Runtime.Preserve]
        void Listo(object sender, EventArgs e)
        {
            CuentaUsuario.AlmacenarColumnasParaVer(string.Join(",", _listaColumnas));
            var paginaSeleccionColumnasInventario = new SeleccionColumnasInventario(_columnas, _linkHojaConsulta, _servicio);
            Navigation.PushAsync(paginaSeleccionColumnasInventario);
        }
    }
}
