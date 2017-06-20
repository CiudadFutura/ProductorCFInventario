
using Google.GData.Client;
using Google.GData.Spreadsheets;
using Xamarin.Forms;
using StockProductorCF.Clases;
using System.Collections.Generic;

namespace StockProductorCF.Vistas
{
	public partial class ListaHojasHistoricoGoogle
	{
		private readonly AtomEntryCollection _listaHojas;
		private readonly SpreadsheetsService _servicio;
		private double _anchoActual;
		private readonly string _nombreHojaExistenciaSeleccionada;

		public ListaHojasHistoricoGoogle(SpreadsheetsService servicio, AtomEntryCollection listaHojas, string nombreHojaExistenciaSeleccionada)
		{
			InitializeComponent();
			Cabecera.Children.Add(App.ObtenerImagen(TipoImagen.EncabezadoProyectos));
			SombraEncabezado.Source = ImageSource.FromResource(App.RutaImagenSombraEncabezado);
			_servicio = servicio;
			_listaHojas = listaHojas;
			_nombreHojaExistenciaSeleccionada = nombreHojaExistenciaSeleccionada;

			CargarListaHojas();
		}

		private void EnviarPaginaColumnas(string linkHoja)
		{
			//Almacenar la hoja para el historial de movimientos
			CuentaUsuario.AlmacenarLinkHojaHistorial(linkHoja);
			//Almacena la hoja de Histórico en el diccionario para cambiarla cuando se cambie la hoja de stock
			CuentaUsuario.AlmacenarNombreDeHojaHistoricos(linkHoja, _nombreHojaExistenciaSeleccionada);

			ContentPage pagina = new SeleccionColumnasParaVer(CuentaUsuario.ObtenerLinkHojaConsulta(), _servicio);
			Navigation.PushAsync(pagina);
		}

		private void CargarListaHojas()
		{
			var listaHojas = new List<ClaseHoja>();
			var esTeclaPar = false;
			foreach (var datosHoja in _listaHojas)
			{
				var linkHoja = datosHoja.Links.FindService(GDataSpreadsheetsNameTable.CellRel, null).HRef.ToString();
				var linkHistoricos = datosHoja.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null).HRef.ToString();
				var estaUsada = CuentaUsuario.VerificarHojaUsada(linkHoja);
				var esHistorico = CuentaUsuario.VerificarHojaHistoricosUsada(linkHistoricos);

				if (estaUsada) continue; //Si la hoja está siendo usada para inventario no la exponemos para históricos.
				var hoja = new ClaseHoja(linkHistoricos, datosHoja.Title.Text, false, false, esHistorico, esTeclaPar);
				listaHojas.Add(hoja);
				esTeclaPar = !esTeclaPar;
			}

			var vista = new ListView
			{
				RowHeight = 60,
				VerticalOptions = LayoutOptions.StartAndExpand,
				HorizontalOptions = LayoutOptions.Fill,
				ItemsSource = listaHojas,
				ItemTemplate = new DataTemplate(() =>
				{
					var nombreHoja = new Label
					{
						FontSize = 18,
						TextColor = Color.FromHex("#1D1D1B"),
						VerticalOptions = LayoutOptions.CenterAndExpand,
						HorizontalOptions = LayoutOptions.CenterAndExpand
					};
					nombreHoja.SetBinding(Label.TextProperty, "Nombre");

					var icono = new Image
					{
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.End,
						HeightRequest = App.AnchoRetratoDePantalla * .09259
					};
					icono.SetBinding(Image.SourceProperty, "ArchivoIcono");
					icono.SetBinding(IsVisibleProperty, "TieneImagen");

					var tecla = new StackLayout
					{
						Padding = 7,
						Orientation = StackOrientation.Horizontal,
						Children = { nombreHoja, icono }
					};
					tecla.SetBinding(BackgroundColorProperty, "ColorFondo");

					var celda = new ViewCell { View = tecla };

					celda.Tapped += (sender, args) =>
					{
						var hoja = (ClaseHoja)((ViewCell)sender).BindingContext;
						EnviarPaginaColumnas(hoja.Link);
						celda.View.BackgroundColor = Color.Silver;
					};

					return celda;
				})
			};

			ContenedorHojas.Children.Add(vista);
		}

		protected override async void OnSizeAllocated(double ancho, double alto)
		{
			base.OnSizeAllocated(ancho, alto);
			if (_anchoActual == ancho) return;
			if (ancho > alto)
			{
				if (_anchoActual != 0)
					await GrupoEncabezado.TranslateTo(0, -100, 1000);
				GrupoEncabezado.IsVisible = false;
			}
			else
			{
				GrupoEncabezado.IsVisible = true;
				if (_anchoActual != 0)
					await GrupoEncabezado.TranslateTo(0, 0, 1000);
			}
			_anchoActual = ancho;
		}
	}

}
