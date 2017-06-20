
using Google.GData.Client;
using Google.GData.Spreadsheets;
using Xamarin.Forms;
using StockProductorCF.Clases;
using System.Collections.Generic;

namespace StockProductorCF.Vistas
{
	public partial class ListaHojasCalculoGoogle
	{
		private readonly AtomEntryCollection _listaHojas;
		private readonly SpreadsheetsService _servicio;
		private double _anchoActual;

		public ListaHojasCalculoGoogle(SpreadsheetsService servicio, AtomEntryCollection listaHojas)
		{
			InitializeComponent();
			Cabecera.Children.Add(App.ObtenerImagen(TipoImagen.EncabezadoProyectos));
			SombraEncabezado.Source = ImageSource.FromResource(App.RutaImagenSombraEncabezado);
			_servicio = servicio;
			_listaHojas = listaHojas;

			CargarListaHojas();
		}

		private void EnviarPagina(string linkHoja, string nombreHoja, bool reiniciaHoja)
		{
			//Se almacena el link para recobrar los datos de stock de la hoja cuando ingrese nuevamente.
			CuentaUsuario.AlmacenarLinkHojaConsulta(linkHoja);
			CuentaUsuario.AlmacenarNombreDeHoja(linkHoja, nombreHoja.Replace("Reiniciar ", ""));

			ContentPage pagina;
			//Si ya se usó esta hoja alguna vez y si el botón presionado NO es el de reiniciar, carga las columnas ya seleccionadas y envía a Grilla.
			//Si no (alguna de las dos condiciones) envía a pantallas de selección de histórico y columnas.
			if (!reiniciaHoja && CuentaUsuario.VerificarHojaUsadaRecuperarColumnas(linkHoja))
				pagina = new PaginaGrilla(linkHoja, _servicio);
			else
				pagina = new ListaHojasHistoricoGoogle(_servicio, _listaHojas, nombreHoja.Replace("Reiniciar ", ""));

			Navigation.PushAsync(pagina);
		}

		private void CargarListaHojas()
		{
			var listaHojas = new List<ClaseHoja>();
			var esTeclaPar = false;
			foreach (WorksheetEntry datosHoja in _listaHojas)
			{
				var linkHoja = datosHoja.Links.FindService(GDataSpreadsheetsNameTable.CellRel, null).HRef.ToString();
				var linkHistoricos = datosHoja.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null).HRef.ToString();
				var estaUsada = CuentaUsuario.VerificarHojaUsada(linkHoja);
				var esHistorico = CuentaUsuario.VerificarHojaHistoricosUsada(linkHistoricos);
				//Agrega hoja (tecla) para potencial hoja de stock.
				//Si ya está siendo usada le agrega ícono.
				//Si en cambio es hoja de Históricos le pone el ícono correspondiente.
				//Nunca es hoja en uso de stock y de históricos; es una, otra o ninguna.
				var hoja = new ClaseHoja(linkHoja, datosHoja.Title.Text, false, estaUsada, esHistorico, esTeclaPar);
				listaHojas.Add(hoja);
				esTeclaPar = !esTeclaPar;

				if (!estaUsada) continue;
				//Si la hoja está siendo usada, agrega hoja (tecla) para reiniciar (con ícono).
				hoja = new ClaseHoja(datosHoja.Links.FindService(GDataSpreadsheetsNameTable.CellRel, null).HRef.ToString(), "Reiniciar " + datosHoja.Title.Text, true, false, false, esTeclaPar);
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
						Children = {nombreHoja, icono}
					};
					tecla.SetBinding(BackgroundColorProperty, "ColorFondo");

					var celda = new ViewCell { View = tecla };
					
					celda.Tapped += (sender, args) =>
					{
						var hoja = (ClaseHoja)((ViewCell)sender).BindingContext;
						EnviarPagina(hoja.Link, hoja.Nombre, hoja.EsDeReinicio);
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

	//Clase hoja: utilizada para armar la lista scrolleable de hojas
	[Android.Runtime.Preserve]
	public class ClaseHoja
	{
		[Android.Runtime.Preserve]
		public ClaseHoja(string link, string nombre, bool esDeReinicio, bool esInventario, bool esHistorico, bool esTeclaPar)
		{
			Link = link;
			Nombre = nombre;
			EsDeReinicio = esDeReinicio;
			TieneImagen = esDeReinicio || esInventario || esHistorico;
			var nombreArchivoIcono = esDeReinicio ? "refrescarHoja" : esInventario ? "hojaInventario" : "hojaHistoricos";
			ArchivoIcono = ImageSource.FromResource($"StockProductorCF.Imagenes.{nombreArchivoIcono}.png");
			ColorFondo = esTeclaPar ? Color.FromHex("#EDEDED") : Color.FromHex("#E2E2E1");
		}

		[Android.Runtime.Preserve]
		public string Link { get; }
		[Android.Runtime.Preserve]
		public string Nombre { get; }
		[Android.Runtime.Preserve]
		public bool EsDeReinicio { get; }
		[Android.Runtime.Preserve]
		public bool TieneImagen { get; }
		[Android.Runtime.Preserve]
		public ImageSource ArchivoIcono { get; }
		[Android.Runtime.Preserve]
		public Color ColorFondo { get; }
	}

}
