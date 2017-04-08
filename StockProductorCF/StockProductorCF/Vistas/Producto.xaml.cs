
using System;
using Google.GData.Spreadsheets;
using Xamarin.Forms;
using StockProductorCF.Clases;
using StockProductorCF.Servicios;

namespace StockProductorCF.Vistas
{
	public partial class Producto : ContentPage
	{
		private bool[] _signoPositivo;
		private double[] _movimientos;
		private CellEntry[] _producto;
		private string[] _listaColumnasInventario;
		private SpreadsheetsService _servicio;
		private CellEntry[] _nombresColumnas;

		public Producto(CellEntry[] producto, CellEntry[] nombresColumnas, SpreadsheetsService servicio)
		{
			InitializeComponent();

			_producto = producto;
			_servicio = servicio;
			_nombresColumnas = nombresColumnas;

			CargarDatosProductos(nombresColumnas);
		}

		private void CargarDatosProductos(CellEntry[] nombresColumnas)
		{
			Label nombreCampo;
			Entry valorCampo;
			StackLayout campoValor;

			Button botonSigno;
			Entry movimiento;

			var columnasInventario = CuentaUsuario.ObtenerColumnasInventario();
			_listaColumnasInventario = null;
			if (!string.IsNullOrEmpty(columnasInventario))
				_listaColumnasInventario = columnasInventario.Split(',');

			_signoPositivo = new bool[_producto.Length];
			_movimientos = new double[_producto.Length];

			foreach (CellEntry celda in _producto)
			{
				if (celda != null && nombresColumnas[celda.Column - 1].Value != "Usuario-Movimiento")
				{
					nombreCampo = new Label()
					{
						HorizontalOptions = LayoutOptions.EndAndExpand,
						VerticalOptions = LayoutOptions.Center,
						HorizontalTextAlignment = TextAlignment.End,
						FontSize = 16,
						WidthRequest = 100
					};

					nombreCampo.Text = nombresColumnas[celda.Column - 1].Value;

					valorCampo = new Entry()
					{
						HorizontalOptions = LayoutOptions.CenterAndExpand,
						VerticalOptions = LayoutOptions.Center,
						HorizontalTextAlignment = TextAlignment.Start,
						WidthRequest = 210,
						IsEnabled = false
					};
					valorCampo.Text = celda != null ? celda.Value : "";

					campoValor = new StackLayout
					{
						HorizontalOptions = LayoutOptions.FillAndExpand,
						VerticalOptions = LayoutOptions.CenterAndExpand,
						Orientation = StackOrientation.Horizontal,
						HeightRequest = 50,
						Children = { nombreCampo, valorCampo }
					};

					ContenedorProducto.Children.Add(campoValor);

					if (celda != null && _listaColumnasInventario != null && _listaColumnasInventario[(int)celda.Column - 1] == "1")
					{
						_signoPositivo.SetValue(true, (int)celda.Column - 1);

						nombreCampo = new Label()
						{
							HorizontalOptions = LayoutOptions.EndAndExpand,
							VerticalOptions = LayoutOptions.Center,
							HorizontalTextAlignment = TextAlignment.End,
							Text = "Movimiento",
							FontSize = 16,
							WidthRequest = 100
						};

						botonSigno = new Button()
						{
							Text = "+",
							HorizontalOptions = LayoutOptions.Start,
							VerticalOptions = LayoutOptions.Center,
							FontSize = 25,
							StyleId = celda.Column.ToString()
						};

						botonSigno.Clicked += DefinirSigno;

						movimiento = new Entry()
						{
							HorizontalOptions = LayoutOptions.StartAndExpand,
							VerticalOptions = LayoutOptions.Center,
							HorizontalTextAlignment = TextAlignment.Start,
							StyleId = "movimiento-" + celda.Column.ToString(),
							WidthRequest = 100,
							Keyboard = Keyboard.Numeric
						};

						campoValor = new StackLayout
						{
							HorizontalOptions = LayoutOptions.FillAndExpand,
							VerticalOptions = LayoutOptions.CenterAndExpand,
							Orientation = StackOrientation.Horizontal,
							HeightRequest = 50,
							Children = { nombreCampo, botonSigno, movimiento }
						};

						ContenedorProducto.Children.Add(campoValor);
					}
				}
			}
		}

		private void DefinirSigno(object sender, EventArgs e)
		{
			Button boton = ((Button)sender);
			boton.Text = _signoPositivo[Convert.ToInt32(boton.StyleId) - 1] ? "-" : "+";
			_signoPositivo.SetValue(boton.Text == "+", Convert.ToInt32(boton.StyleId) - 1);
		}

		[Android.Runtime.Preserve]
		async void GuardarCambios(object sender, EventArgs args)
		{
			foreach (View stackLayout in ContenedorProducto.Children)
			{
				foreach (View control in ((StackLayout)stackLayout).Children)
				{
					if (control.StyleId != null && control.StyleId.Contains("movimiento-"))
					{
						var columna = Convert.ToInt32(control.StyleId.Split('-')[1]);
						var valor = Convert.ToDouble(((Entry)control).Text ?? "0");
						_movimientos.SetValue(valor, columna - 1);
					}
				}
			}

			var servicioGoogle = new ServiciosGoogle();
			var multiplicador = 1;
			var movimiento = 0.00;
			foreach (CellEntry celda in _producto)
			{
				if (_listaColumnasInventario[(int)celda.Column - 1] == "1")
				{
					multiplicador = _signoPositivo[(int)celda.Column - 1] ? 1 : -1;
					movimiento = _movimientos[(int)celda.Column - 1];

					if (movimiento != 0)
					{
						celda.InputValue = (Convert.ToDouble(celda.InputValue) + multiplicador * movimiento).ToString();

						try
						{
							// Actualiza la celda en Google
							celda.Update();
							// Inserta histórico en Google
							servicioGoogle.InsertarHistoricos(_servicio, celda, multiplicador * movimiento, _producto, _nombresColumnas, _listaColumnasInventario);
						}
						catch (Exception ex)
						{
							// Si se quedó la pantalla abierta un largo tiempo y se venció el token, se cierra y refresca el token
							var paginaAuntenticacion = new PaginaAuntenticacion(true);
							Navigation.InsertPageBefore(paginaAuntenticacion, this);
							await Navigation.PopAsync();
						}
					}
				}
			}

			await Navigation.PopAsync();
		}

	}
}
