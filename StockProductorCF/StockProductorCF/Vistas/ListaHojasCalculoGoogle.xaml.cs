
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
			Cabecera.Source = ImageSource.FromResource(string.Format("StockProductorCF.Imagenes.encabezadoProyectos{0}.png", App.SufijoImagen));

			_servicio = servicio;
			_listaHojas = listaHojas;

			CargarListaHojas();
		}

		private void CargarListaHojas()
		{
			StackLayout itemHoja;
			Button botonHoja;
			Button botonReiniciarHoja;
			foreach (WorksheetEntry hoja in _listaHojas)
			{
				if (hoja.Title.Text == "Historial")
					CuentaUsuario.AlmacenarLinkHojaHistorial(hoja.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null).HRef.ToString());

				itemHoja = new StackLayout
				{
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Start
				};

				botonHoja = new Button
				{
					Text = hoja.Title.Text,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					VerticalOptions = LayoutOptions.Start,
					HeightRequest = 55
				};
				botonHoja.Resources = new ResourceDictionary();
				botonHoja.Resources.Add("Id", hoja.Id);
				botonHoja.Clicked += EnviarPaginaGrilla;

				itemHoja.Children.Add(botonHoja);

				if(CuentaUsuario.VerificarHojaUsada(hoja.Links.FindService(GDataSpreadsheetsNameTable.CellRel, null).HRef.ToString()))
				{
					botonHoja.HorizontalOptions = LayoutOptions.StartAndExpand;

					botonReiniciarHoja = new Button
					{
						Text = "Reiniciar hoja",
						HorizontalOptions = LayoutOptions.EndAndExpand,
						VerticalOptions = LayoutOptions.Start,
						HeightRequest = 55
					};
					botonReiniciarHoja.Resources = new ResourceDictionary();
					botonReiniciarHoja.Resources.Add("Id", hoja.Id);
					botonReiniciarHoja.StyleId = "R";
					botonReiniciarHoja.Clicked += EnviarPaginaGrilla;

					itemHoja.Children.Add(botonReiniciarHoja);
				}

				ContenedorHojas.Children.Add(itemHoja);
			}
		}

		void EnviarPaginaGrilla(object control, EventArgs args)
		{
			var boton = (Button)control;
			var hoja = _listaHojas.FindById((AtomId)boton.Resources["Id"]);

			string link = hoja.Links.FindService(GDataSpreadsheetsNameTable.CellRel, null).HRef.ToString();

			//Se almacena el link para recobrar los datos de stock de la hoja cuando ingrese nuevamente.
			CuentaUsuario.AlmacenarLinkHojaConsulta(link);
			CuentaUsuario.AlmacenarNombreDeHoja(link, hoja.Title.Text);

			ContentPage pagina;
			//Si ya se usó esta hoja alguna vez, carga las columnas ya seleccionadas y envía a Grilla.
			//Si no o si el botón presionado es el de reiniciar, envía a pantallas de selección de columnas.
			if (boton.StyleId != "R" && CuentaUsuario.VerificarHojaUsadaRecuperarColumnas(link))
			{
				pagina = new PaginaGrilla(link, _servicio);
			}
			else
			{
				pagina = new SeleccionColumnasParaVer(link, _servicio);
			}
			Navigation.PushAsync(pagina);

		}
	}
}
