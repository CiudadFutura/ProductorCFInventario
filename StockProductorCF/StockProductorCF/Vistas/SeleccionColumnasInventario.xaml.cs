
using System;
using Google.GData.Spreadsheets;
using Xamarin.Forms;
using StockProductorCF.Clases;
using System.Linq;
using System.Collections.Generic;

namespace StockProductorCF.Vistas
{
	public partial class SeleccionColumnasInventario : ContentPage
	{
		private SpreadsheetsService _servicio;
		private string _linkHojaConsulta;
		private int[] _listaColumnas;

		public SeleccionColumnasInventario(List<CellEntry> columnas, string linkHojaConsulta, SpreadsheetsService servicio)
		{
			InitializeComponent();
			Cabecera.Source = ImageSource.FromResource(string.Format("StockProductorCF.Imagenes.encabezadoProyectos{0}.png", App.SufijoImagen));

			_servicio = servicio;
			_linkHojaConsulta = linkHojaConsulta;

			LlenarGrillaColumnasInventario(columnas);
		}

		private void LlenarGrillaColumnasInventario(List<CellEntry> columnas)
		{
			_listaColumnas = Enumerable.Repeat(1, columnas.Count).ToArray();

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
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.CenterAndExpand,
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
			CuentaUsuario.AlmacenarColumnasInventarioDeHoja(_linkHojaConsulta, string.Join(",", _listaColumnas));
			var paginaGrilla = new PaginaGrilla(_linkHojaConsulta, _servicio);
			Navigation.PushAsync(paginaGrilla);
		}
	}
}
