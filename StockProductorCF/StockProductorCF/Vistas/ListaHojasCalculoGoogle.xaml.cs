
using System;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using Xamarin.Forms;
using StockProductorCF.Clases;

namespace StockProductorCF.Vistas
{
	public partial class ListaHojasCalculoGoogle : ContentPage
	{
		private AtomEntryCollection _listaHojas;
		private SpreadsheetsService _servicio;

		public ListaHojasCalculoGoogle(SpreadsheetsService servicio, AtomEntryCollection listaHojas)
		{
			InitializeComponent();

			_servicio = servicio;
			_listaHojas = listaHojas;

			CargarListaHojas();
		}

		private void CargarListaHojas()
		{
			StackLayout itemHoja;
			foreach (WorksheetEntry hoja in _listaHojas)
			{
				if (hoja.Title.Text == "Historial")
					CuentaUsuario.AlmacenarLinkHojaHistorial(hoja.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null).HRef.ToString());

				itemHoja = new StackLayout
				{
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Start
				};

				var boton = new Button
				{
					Text = hoja.Title.Text,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					VerticalOptions = LayoutOptions.Start,
					HeightRequest = 55
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

			var paginaSeleccionColumnasParaVer = new SeleccionColumnasParaVer(link.HRef.ToString(), _servicio);
			Navigation.PushAsync(paginaSeleccionColumnasParaVer);

		}
	}
}
