
using Google.GData.Client;
using Google.GData.Spreadsheets;
using Xamarin.Forms;
using StockProductorCF.Clases;
using System.Collections.Generic;
using System.Threading.Tasks;
using StockProductorCF.Servicios;

namespace StockProductorCF.Vistas
{
	public partial class ListaHojasPtosVtaGoogle
	{
		private readonly AtomEntryCollection _listaHojas;
		private readonly SpreadsheetsService _servicio;
		private CellFeed _celdas;
		private double _anchoActual;

		public ListaHojasPtosVtaGoogle(SpreadsheetsService servicio, AtomEntryCollection listaHojas)
		{
			InitializeComponent();
			Cabecera.Children.Add(App.ObtenerImagen(TipoImagen.EncabezadoProyectos));
			SombraEncabezado.Source = ImageSource.FromResource(App.RutaImagenSombraEncabezado);
			_servicio = servicio;
			_listaHojas = listaHojas;

			CargarListaHojas();
		}

		private void EnviarPaginaSeleccionColumnas(string linkHoja)
		{
			//Se almacena el link para recobrar los datos de puntos de venta de la hoja cuando ingrese nuevamente.
			ObtenerPuntosVenta(linkHoja);
			var puntosVentaTexto = "";
			foreach (CellEntry celda in _celdas.Entries)
			{
				if(celda.Row != 1)
					puntosVentaTexto += celda.Value + "|";
			}

			CuentaUsuario.AlmacenarPuntosVenta(puntosVentaTexto.TrimEnd('|'));
			CuentaUsuario.AlmacenarPuntosVentaDeHoja(CuentaUsuario.ObtenerLinkHojaConsulta(), puntosVentaTexto.TrimEnd('|'));
			CuentaUsuario.AlmacenarNombreHojaPuntosVentaDeHoja(CuentaUsuario.ObtenerLinkHojaConsulta(), linkHoja);

			ContentPage pagina = new SeleccionColumnasParaVer(CuentaUsuario.ObtenerLinkHojaConsulta(), _servicio);
			Navigation.PushAsync(pagina);
		}

		private async void ObtenerPuntosVenta(string linkHoja)
		{
			if (CuentaUsuario.ValidarTokenDeGoogle())
			{
				_celdas = new ServiciosGoogle().ObtenerCeldasDeUnaHoja(linkHoja, _servicio);
			}
			else
			{
				//Si la pantalla anterior se quedó abierta un largo tiempo y se venció el token, se cierra y refresca el token
				var paginaAuntenticacion = new PaginaAuntenticacion(true);
				Navigation.InsertPageBefore(paginaAuntenticacion, this);
				await Navigation.PopAsync();
			}
		}

		private void CargarListaHojas()
		{
			var listaHojas = new List<ClaseHoja>();
			var esTeclaPar = false;
			foreach (var datosHoja in _listaHojas)
			{
				var linkHoja = datosHoja.Links.FindService(GDataSpreadsheetsNameTable.CellRel, null).HRef.ToString();
				var linkHistoricos = datosHoja.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null).HRef.ToString();
				var estaSeleccionada = CuentaUsuario.ObtenerLinkHojaConsulta() == linkHoja; // Tiene que ser la actualmente seleccionada
				var estaUsada = CuentaUsuario.VerificarHojaUsada(linkHoja); // Tiene que haber sido seleccionada alguna vez.
				var esHistorico = CuentaUsuario.VerificarHojaHistoricosUsada(linkHistoricos);
				var esPuntosVenta = CuentaUsuario.VerificarHojaPuntosVentaUsada(linkHoja);

				if (estaSeleccionada || estaUsada || esHistorico) continue;
				var hoja = new ClaseHoja(linkHoja, datosHoja.Title.Text, false, false, false, esPuntosVenta, esTeclaPar);
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
						EnviarPaginaSeleccionColumnas(hoja.Link);
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
