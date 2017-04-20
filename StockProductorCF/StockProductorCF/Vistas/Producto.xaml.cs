
using System;
using Google.GData.Spreadsheets;
using Xamarin.Forms;
using StockProductorCF.Clases;
using StockProductorCF.Servicios;
using System.Net.Http;

namespace StockProductorCF.Vistas
{
	public partial class Producto : ContentPage
	{
		private bool[] _signoPositivo;
		private double[] _movimientos;
		private CellEntry[] _producto;
		private string[] _listaColumnasInventario;
		private string[] _productoString;
		private SpreadsheetsService _servicio;
		private string[] _nombresColumnas;
		private string _mensaje = "";

		public Producto(CellEntry[] producto, string[] nombresColumnas, SpreadsheetsService servicio)
		{
			InitializeComponent();

			_producto = producto;
			_servicio = servicio;
			_nombresColumnas = nombresColumnas;

			//Almacenar el arreglo de strings para cargar el producto en pantalla
			_productoString = new string[producto.Length];
			var i = 0;
			foreach (CellEntry celda in producto)
			{
				_productoString.SetValue(celda.InputValue, i);
				i = i + 1;
			}

			CargarDatosProductos();
		}

		public Producto(string[] productoBD, string[] nombresColumnas)
		{
			InitializeComponent();

			_productoString = productoBD;
			_nombresColumnas = nombresColumnas;

			CargarDatosProductos();
		}

		private void CargarDatosProductos()
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

			_signoPositivo = new bool[_productoString.Length];
			_movimientos = new double[_productoString.Length];
			var i = 0;

			foreach (string celda in _productoString)
			{
				if (celda != null && _nombresColumnas[i] != "Usuario-Movimiento")
				{
					nombreCampo = new Label()
					{
						HorizontalOptions = LayoutOptions.EndAndExpand,
						VerticalOptions = LayoutOptions.Center,
						HorizontalTextAlignment = TextAlignment.End,
						FontSize = 16,
						WidthRequest = 100
					};

					nombreCampo.Text = _nombresColumnas[i];

					valorCampo = new Entry()
					{
						HorizontalOptions = LayoutOptions.CenterAndExpand,
						VerticalOptions = LayoutOptions.Center,
						HorizontalTextAlignment = TextAlignment.Start,
						WidthRequest = 210,
						IsEnabled = false
					};
					valorCampo.Text = celda;

					campoValor = new StackLayout
					{
						HorizontalOptions = LayoutOptions.FillAndExpand,
						VerticalOptions = LayoutOptions.CenterAndExpand,
						Orientation = StackOrientation.Horizontal,
						HeightRequest = 50,
						Children = { nombreCampo, valorCampo }
					};

					ContenedorProducto.Children.Add(campoValor);

					if (celda != null && _listaColumnasInventario != null && _listaColumnasInventario[i] == "1")
					{
						_signoPositivo.SetValue(true, i);

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
							StyleId = i.ToString()
						};

						botonSigno.Clicked += DefinirSigno;

						movimiento = new Entry()
						{
							HorizontalOptions = LayoutOptions.StartAndExpand,
							VerticalOptions = LayoutOptions.Center,
							HorizontalTextAlignment = TextAlignment.Start,
							StyleId = "movimiento-" + i.ToString(),
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

				i = i + 1;
			}

		}

		private void DefinirSigno(object sender, EventArgs e)
		{
			Button boton = ((Button)sender);
			int columna = Convert.ToInt32(boton.StyleId);
			boton.Text = _signoPositivo[columna] ? "-" : "+";
			_signoPositivo.SetValue(boton.Text == "+", columna);
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
						_movimientos.SetValue(valor, columna);
					}
				}
			}

			if (CuentaUsuario.ObtenerAccesoDatos() == "G")
				GuardarProductoHojaDeCalculoGoogle();
			else
				GuardarProductoBaseDeDatos();

			await Navigation.PopAsync();
			await DisplayAlert("Producto", _mensaje, "Listo");
		}

		private async void GuardarProductoHojaDeCalculoGoogle()
		{
			_mensaje = "Ha ocurrido un error mientras se guardaba el mocimiento.";
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

							_mensaje = "Se guardó el movimiento con éxito.";
						}
						catch (Exception)
						{
							// Si se quedó la pantalla abierta un largo tiempo y se venció el token, se cierra y refresca el token
							var paginaAuntenticacion = new PaginaAuntenticacion(true);
							Navigation.InsertPageBefore(paginaAuntenticacion, this);
							await Navigation.PopAsync();
						}
					}
				}
			}
		}

		private async void GuardarProductoBaseDeDatos()
		{
			var url = @"http://169.254.80.80/PruebaMision/Service.asmx/ActualizarProducto?codigo={0}&movimiento={1}";

			var multiplicador = 1;
			var movimiento = 0.00;
			var i = 0;
			var resultado = "";
			foreach (string celda in _productoString)
			{
				if (_listaColumnasInventario[i] == "1")
				{
					multiplicador = _signoPositivo[i] ? 1 : -1;
					movimiento = _movimientos[i];

					if (movimiento != 0)
					{
						using (var cliente = new HttpClient())
						{
							resultado = await cliente.GetStringAsync(string.Format(url, _productoString[0], (Convert.ToDouble(celda) + multiplicador * movimiento).ToString()));
						}
					}
				}
				i = i + 1;
			}

			_mensaje = "Se guardó el movimiento con éxito.";
			if(!resultado.Contains("anduvo"))
				_mensaje = "Ha ocurrido un error mientras se guardaba el mocimiento.";
		}

	}
}
