
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
		private bool _esTeclaPar;

		public ListaHojasCalculoGoogle(SpreadsheetsService servicio, AtomEntryCollection listaHojas)
		{
			InitializeComponent();

			_servicio = servicio;
			_listaHojas = listaHojas;

			CargarListaHojas();
		}
		
		private void EnviarPaginaGrilla(string linkHoja, string nombreHoja, bool reiniciaHoja)
		{
			//Se almacena el link para recobrar los datos de stock de la hoja cuando ingrese nuevamente.
			CuentaUsuario.AlmacenarLinkHojaConsulta(linkHoja);
			CuentaUsuario.AlmacenarNombreDeHoja(linkHoja, nombreHoja.Replace("Reiniciar ", ""));

			ContentPage pagina;
			//Si ya se usó esta hoja alguna vez, carga las columnas ya seleccionadas y envía a Grilla.
			//Si no o si el botón presionado es el de reiniciar, envía a pantallas de selección de columnas.
			if (!reiniciaHoja && CuentaUsuario.VerificarHojaUsadaRecuperarColumnas(linkHoja))
			{
				pagina = new PaginaGrilla(linkHoja, _servicio);
			}
			else
			{
				pagina = new SeleccionColumnasParaVer(linkHoja, _servicio);
			}
			Navigation.PushAsync(pagina);
		}

		private void CargarListaHojas()
		{
			var listaHojas = new List<ClaseHoja>();
			foreach (WorksheetEntry datosHoja in _listaHojas)
			{
				//Almacenar la hoja para el historial de movimientos
				if(datosHoja.Title.Text == "Historial")
					CuentaUsuario.AlmacenarLinkHojaHistorial(datosHoja.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null).HRef.ToString());

				var hoja = new ClaseHoja(datosHoja.Links.FindService(GDataSpreadsheetsNameTable.CellRel, null).HRef.ToString(), datosHoja.Title.Text, false);
				listaHojas.Add(hoja);

				if (CuentaUsuario.VerificarHojaUsada(hoja.Link))
				{
					hoja = new ClaseHoja(datosHoja.Links.FindService(GDataSpreadsheetsNameTable.CellRel, null).HRef.ToString(), "Reiniciar " + datosHoja.Title.Text, true);
					listaHojas.Add(hoja);
				}
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

					var celda = new ViewCell
					{
						View = new StackLayout
						{
							Padding = 2,
							Orientation = StackOrientation.Horizontal,
							Children = { nombreHoja }
						}
					};
					
					celda.Tapped += (sender, args) =>
					{
						var hoja = (ClaseHoja)((ViewCell) sender).BindingContext;
						EnviarPaginaGrilla(hoja.Link, hoja.Nombre, hoja.EsDeReinicio);
						celda.View.BackgroundColor = Color.Silver;
					};

					celda.Appearing += (sender, args) =>
					{
						var viewCell = (ViewCell)sender;
						if (viewCell.View != null)
						{
							viewCell.View.BackgroundColor = _esTeclaPar ? Color.FromHex("#EDEDED") : Color.FromHex("#E2E2E1");
							if(((ClaseHoja)((ViewCell)sender).BindingContext).EsDeReinicio)
								((StackLayout)viewCell.View).Children.Add(new Image { Source = ImageSource.FromResource($"StockProductorCF.Imagenes.refrescarHoja{App.Sufijo}.png") });
						}
						_esTeclaPar = !_esTeclaPar;
					};

					return celda;
				})
			};

			ContenedorHojas.Children.Add(vista);
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
			App.OrientacionApaisada = width > height;
			Cabecera.Source = App.ObtenerImagenEncabezadoProyectos();
		}
	}

	//Clase hoja: utilizada para armar la lista scrolleable de hojas
	[Android.Runtime.Preserve]
	public class ClaseHoja
	{
		[Android.Runtime.Preserve]
		public ClaseHoja(string link, string nombre, bool esDeReinicio)
		{
			Link = link;
			Nombre = nombre;
			EsDeReinicio = esDeReinicio;
		}

		[Android.Runtime.Preserve]
		public string Link { get; }
		[Android.Runtime.Preserve]
		public string Nombre { get; }
		[Android.Runtime.Preserve]
		public bool EsDeReinicio { get; }
	}
}
