
using System;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using Xamarin.Forms;
using StockProductorCF.Clases;

namespace StockProductorCF.Vistas
{
    public partial class ListaHojasCalculoGoogle : ContentPage
    {
        AtomEntryCollection _listaHojas;
        SpreadsheetsService _servicio;
        public ListaHojasCalculoGoogle(SpreadsheetsService servicio, AtomEntryCollection listaHojas)
        {
            InitializeComponent();
            LogoEmprendimiento.Source = ImageSource.FromResource("StockProductorCF.Imagenes.logoEmprendimiento.png");
            Cabecera.Source = ImageSource.FromResource("StockProductorCF.Imagenes.ciudadFutura.png");

            _servicio = servicio;
            _listaHojas = listaHojas;

            CargarListaHojas();
        }

        private void CargarListaHojas()
        {
            StackLayout itemHoja;
            foreach (WorksheetEntry hoja in _listaHojas)
            {
                itemHoja = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                };
                var boton = new Button
                {
                    Text = hoja.Title.Text,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    WidthRequest = 250,
                    HeightRequest = 60
                };

                boton.Resources = new ResourceDictionary();
                boton.Resources.Add("Id", hoja.Id);
                boton.Clicked += EnviarPaginaGrilla;

                itemHoja.Children.Add(boton);
                ContenedorHojas.Children.Add(itemHoja);
            }
        }

        void EnviarPaginaGrilla(object boton, EventArgs args)
        {
            var hoja = _listaHojas.FindById(((AtomId)((Button)boton).Resources["Id"]));

            AtomLink link = hoja.Links.FindService(GDataSpreadsheetsNameTable.CellRel, null);
            
            //Se almacena el link para recobrar los datos de stock de la hoja cuando ingrese nuevamente
            CuentaUsuario.AlmacenarLinkHojaConsulta(link.HRef.ToString());

            var paginaGrilla = new PaginaGrilla(link.HRef.ToString(), _servicio);
            Navigation.PushAsync(paginaGrilla);
            
        }
    }
}
