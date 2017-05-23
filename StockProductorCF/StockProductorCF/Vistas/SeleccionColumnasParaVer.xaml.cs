
using System;
using Google.GData.Spreadsheets;
using Xamarin.Forms;
using StockProductorCF.Servicios;
using StockProductorCF.Clases;
using System.Linq;
using System.Collections.Generic;

namespace StockProductorCF.Vistas
{
	public partial class SeleccionColumnasParaVer
	{
		private readonly SpreadsheetsService _servicio;
		private int[] _listaColumnas;
		private List<CellEntry> _columnas;
		private readonly string _linkHojaConsulta;
		private double _anchoActual;

		public SeleccionColumnasParaVer(string linkHojaConsulta, SpreadsheetsService servicio)
		{
			InitializeComponent();
			Cabecera.Children.Add(App.ObtenerImagen(TipoImagen.EncabezadoProyectos));
			SombraEncabezado.Source = ImageSource.FromResource(App.RutaImagenSombraEncabezado);
			_servicio = servicio;
			_linkHojaConsulta = linkHojaConsulta;

			ObtenerColumnas();
		}

		private void ObtenerColumnas()
		{
			try
			{
				var celdas = new ServiciosGoogle().ObtenerCeldasDeUnaHoja(_linkHojaConsulta, _servicio);

				_columnas = new List<CellEntry>(); //Columnas para listar en pantalla, si hay una de nombre Usuario - Movimiento se quita.

				foreach (CellEntry celda in celdas.Entries)
				{
					if (celda.Row == 1)
					{
						_columnas.Add(celda);
					}
					else
					{
						break;
					}
				}

				_listaColumnas = Enumerable.Repeat(1, _columnas.Count).ToArray(); //El arreglo de columnas para ver, todas con valor inicial en 1
				LlenarGrillaColumnasParaVer(_columnas);
			}
			catch (Exception)
			{
				Navigation.PushAsync(new AccesoDatos());
			}
		}

		private void LlenarGrillaColumnasParaVer(List<CellEntry> columnas)
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
					FontSize = 18
				};

				seleccionar = new Switch
				{
					HorizontalOptions = LayoutOptions.End,
					VerticalOptions = LayoutOptions.Center,
					StyleId = columna.Column.ToString()
				};
				seleccionar.Toggled += AgregarColumna;

				itemColumna = new StackLayout
				{
					HorizontalOptions = LayoutOptions.FillAndExpand,
					VerticalOptions = LayoutOptions.CenterAndExpand,
					Orientation = StackOrientation.Horizontal,
					HeightRequest = 45,
					WidthRequest = 300,
					Children = { etiquetaColumna, seleccionar },
					Spacing = 0,
					Padding = 4,
					BackgroundColor = esGris ? Color.Silver : Color.FromHex("#F5F5F5")
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
			CuentaUsuario.AlmacenarColumnasParaVerDeHoja(_linkHojaConsulta, string.Join(",", _listaColumnas));
			var paginaSeleccionColumnasInventario = new SeleccionColumnasInventario(_columnas, _linkHojaConsulta, _servicio);
			Navigation.PushAsync(paginaSeleccionColumnasInventario);
		}

		protected override void OnSizeAllocated(double ancho, double alto)
		{
			base.OnSizeAllocated(ancho, alto);
			if (_anchoActual == ancho) return;
			SombraEncabezado.WidthRequest = ancho > alto ? App.AnchoApaisadoDePantalla : App.AnchoRetratoDePantalla;
			_anchoActual = ancho;
		}

	}
}
