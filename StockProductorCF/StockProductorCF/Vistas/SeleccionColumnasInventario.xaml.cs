﻿
using System;
using Google.GData.Spreadsheets;
using Xamarin.Forms;
using StockProductorCF.Servicios;
using StockProductorCF.Clases;

namespace StockProductorCF.Vistas
{
    public partial class SeleccionColumnasInventario : ContentPage
    {
        private SpreadsheetsService _servicio;
        private string _linkHojaConsulta;
        private int[] _listaColumnas;

        public SeleccionColumnasInventario(string linkHojaConsulta, SpreadsheetsService servicio)
        {
            InitializeComponent();
            //LogoEmprendimiento.Source = ImageSource.FromResource("StockProductorCF.Imagenes.logoEmprendimiento.png");
            Cabecera.Source = ImageSource.FromResource("StockProductorCF.Imagenes.ciudadFutura.png");

            _servicio = servicio;
            _linkHojaConsulta = linkHojaConsulta;

            ObtenerColumnas();
        }

        private void ObtenerColumnas()
        {
            try
            {
                var celdas = new ServiciosGoogle().ObtenerCeldasDeUnaHoja(_linkHojaConsulta, _servicio);

                CellEntry[] columnas = new CellEntry[celdas.ColCount.Count];
                _listaColumnas = new int[celdas.ColCount.Count];

                foreach (CellEntry celda in celdas.Entries)
                {
                    if (celda.Row == 1)
                    {
                        columnas.SetValue(celda, (int)celda.Column - 1);
                        _listaColumnas.SetValue(1, (int)celda.Column - 1);
                    }
                    else
                    {
                        break;
                    }
                }

                LlenarGrillaColumnasInventario(columnas);
            }
            catch (Exception)
            {
                Navigation.PushAsync(new AccesoDatos());
            }
        }

        private void LlenarGrillaColumnasInventario(CellEntry[] columnas)
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
                    FontSize = 20
                };

                seleccionar = new Switch
                {
                    HeightRequest = 50,
                    WidthRequest = 50,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    StyleId = columna.Column.ToString()
                };
                seleccionar.Toggled += AgregarColumna;

                itemColumna = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    Orientation = StackOrientation.Horizontal,
                    HeightRequest = 50,
                    WidthRequest = 250,
                    Children = { etiquetaColumna, seleccionar },
                    Spacing = 0,
                    Padding = 2,
                    Margin = 1,
                    BackgroundColor = esGris ? Color.Silver : Color.White
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
        private void Listo(object sender, EventArgs e)
        {
            CuentaUsuario.AlmacenarColumnasInventario(string.Join(",", _listaColumnas));
            var paginaGrilla = new PaginaGrilla(_linkHojaConsulta, _servicio);
            Navigation.PushAsync(paginaGrilla);
        }
    }
}
