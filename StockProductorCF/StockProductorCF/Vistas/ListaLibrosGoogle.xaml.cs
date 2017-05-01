
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
			Cabecera.Source = ImageSource.FromResource(string.Format("StockProductorCF.Imagenes.encabezadoProyectos{0}.png", App.SufijoImagen));

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
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Start
				};
				var boton = new Button
				{
					Text = libro.Title.Text,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					VerticalOptions = LayoutOptions.Start,
					HeightRequest = 55
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
