
using System;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using Xamarin.Forms;
using StockProductorCF.Servicios;

namespace StockProductorCF.Vistas
{
    public partial class ListaLibrosGoogle : ContentPage
    {
        AtomEntryCollection _listaLibros;
        SpreadsheetsService _servicio;
        public ListaLibrosGoogle(SpreadsheetsService servicio, AtomEntryCollection listaLibros)
        {
            InitializeComponent();
            //LogoEmprendimiento.Source = ImageSource.FromResource("StockProductorCF.Imagenes.logoEmprendimiento.png");
            Cabecera.Source = ImageSource.FromResource("StockProductorCF.Imagenes.ciudadFutura.png");

            _servicio = servicio;
            _listaLibros = listaLibros;

            CargarListaHojas();
        }

        private void CargarListaHojas()
        {
            StackLayout itemLibro;
            foreach (SpreadsheetEntry libro in _listaLibros)
            {
                itemLibro = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                };
                var boton = new Button
                {
                    Text = libro.Title.Text,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    WidthRequest = 250,
                    HeightRequest = 60
                };

                boton.Resources = new ResourceDictionary();
                boton.Resources.Add("Id", libro.Id);
                boton.Clicked += EnviarListaHojas;

                itemLibro.Children.Add(boton);
                ContenedorLibros.Children.Add(itemLibro);
            }
        }

        void EnviarListaHojas(object boton, EventArgs args)
        {
            var libro = _listaLibros.FindById(((AtomId)((Button)boton).Resources["Id"]));

            WorksheetFeed hojas = new ServiciosGoogle().ObtenerListaHojas(libro, _servicio);

            var paginaListaLibros = new ListaHojasCalculoGoogle(_servicio, hojas.Entries);
            Navigation.PushAsync(paginaListaLibros);
        }
    }
}
