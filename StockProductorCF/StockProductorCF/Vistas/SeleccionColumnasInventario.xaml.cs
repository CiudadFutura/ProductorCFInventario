
using System;
using Google.GData.Spreadsheets;
using Xamarin.Forms;
using StockProductorCF.Clases;
using System.Linq;
using System.Collections.Generic;

namespace StockProductorCF.Vistas
{
	public partial class SeleccionColumnasInventario
	{
		private readonly SpreadsheetsService _servicio;
		private readonly string _linkHojaConsulta;
		private int[] _listaColumnas;

		public SeleccionColumnasInventario(IReadOnlyCollection<CellEntry> columnas, string linkHojaConsulta, SpreadsheetsService servicio)
		{
			InitializeComponent();

			_servicio = servicio;
			_linkHojaConsulta = linkHojaConsulta;

			LlenarGrillaColumnasInventario(columnas);
		}

		private void LlenarGrillaColumnasInventario(IReadOnlyCollection<CellEntry> columnas)
		{
			_listaColumnas = Enumerable.Repeat(1, columnas.Count).ToArray();

			var esGris = false;
			foreach (var columna in columnas)
			{
				var etiquetaColumna = new Label
				{
					TextColor = Color.Black,
					HorizontalOptions = LayoutOptions.StartAndExpand,
					HorizontalTextAlignment = TextAlignment.Start,
					VerticalOptions = LayoutOptions.Center,
					Text = columna.Value,
					FontSize = 18
				};

				var seleccionar = new Switch
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.CenterAndExpand,
					StyleId = columna.Column.ToString()
				};
				seleccionar.Toggled += AgregarColumna;

				var itemColumna = new StackLayout
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
			var ficha = (Switch)sender;
			_listaColumnas.SetValue(Convert.ToInt32(!e.Value), Convert.ToInt32(ficha.StyleId) - 1);
		}

		[Android.Runtime.Preserve]
		private void Listo(object sender, EventArgs e)
		{
			CuentaUsuario.AlmacenarColumnasInventarioDeHoja(_linkHojaConsulta, string.Join(",", _listaColumnas));
			var paginaGrilla = new PaginaGrilla(_linkHojaConsulta, _servicio);
			Navigation.PushAsync(paginaGrilla);
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
			App.OrientacionApaisada = width > height;
			Cabecera.Source = App.ObtenerImagenEncabezadoProyectos();
		}
	}
}
