
using System;
using Google.GData.Spreadsheets;
using Xamarin.Forms;
using StockProductorCF.Clases;
using StockProductorCF.Servicios;
using System.Net.Http;

namespace StockProductorCF.Vistas
{
	public partial class Producto
	{
		private bool[] _signoPositivo;
		private double[] _movimientos;
		private double[] _precios;
		private string[] _lugares;
		private readonly CellEntry[] _producto;
		private string[] _listaColumnasInventario;
		private string[] _listaLugares;
		private readonly string[] _productoString;
		private readonly SpreadsheetsService _servicio;
		private readonly string[] _nombresColumnas;
		private string _mensaje = "";
		private double _anchoActual;

		public Producto(CellEntry[] producto, string[] nombresColumnas, SpreadsheetsService servicio)
		{
			InitializeComponent();
			InicializarValoresGenerales();
			_producto = producto;
			_servicio = servicio;
			_nombresColumnas = nombresColumnas;

			//Almacenar el arreglo de strings para cargar el producto en pantalla
			_productoString = new string[producto.Length];
			var i = 0;
			foreach (var celda in producto)
			{
				_productoString.SetValue(celda.Value, i);
				i = i + 1;
			}

			CargarDatosProductos();
		}

		public Producto(string[] productoBD, string[] nombresColumnas)
		{
			InitializeComponent();
			InicializarValoresGenerales();
			_productoString = productoBD;
			_nombresColumnas = nombresColumnas;

			CargarDatosProductos();
		}

		private void InicializarValoresGenerales()
		{
			Cabecera.Children.Add(App.Instancia.ObtenerImagen(TipoImagen.EncabezadoProductores));
			SombraEncabezado.Source = ImageSource.FromResource(App.RutaImagenSombraEncabezado);
		}

		private void CargarDatosProductos()
		{
			var columnasInventario = CuentaUsuario.ObtenerColumnasInventario();
			_listaColumnasInventario = null;
			if (!string.IsNullOrEmpty(columnasInventario))
				_listaColumnasInventario = columnasInventario.Split(',');

			//Obtener, si existen, los puntos de venta.
			var puntosVentaTexto = CuentaUsuario.ObtenerPuntosVenta();
			_listaLugares = null;
			if (!string.IsNullOrEmpty(puntosVentaTexto))
				_listaLugares = puntosVentaTexto.Split('|');

			_signoPositivo = new bool[_productoString.Length];
			_movimientos = new double[_productoString.Length];
			_precios = new double[_productoString.Length];
			_lugares = new string[_productoString.Length];
			var i = 0;

			var anchoEtiqueta = App.AnchoRetratoDePantalla / 3 - 10;
			var anchoCampo = App.AnchoRetratoDePantalla / 3 * 2 - 30;
			foreach (var celda in _productoString)
			{
				if (celda != null && _nombresColumnas[i] != "Usuario-Movimiento")
				{
					var nombreCampo = new Label
					{
						HorizontalOptions = LayoutOptions.EndAndExpand,
						VerticalOptions = LayoutOptions.Center,
						HorizontalTextAlignment = TextAlignment.End,
						FontSize = 16,
						WidthRequest = anchoEtiqueta,
						Text = _nombresColumnas[i],
						TextColor = Color.Black
					};

					var valorCampo = new Entry
					{
						HorizontalOptions = LayoutOptions.CenterAndExpand,
						VerticalOptions = LayoutOptions.Center,
						HorizontalTextAlignment = TextAlignment.Start,
						WidthRequest = anchoCampo,
						IsEnabled = false,
						Text = celda,
						TextColor = Color.Black
					};

					var campoValor = new StackLayout
					{
						HorizontalOptions = LayoutOptions.FillAndExpand,
						VerticalOptions = LayoutOptions.CenterAndExpand,
						Orientation = StackOrientation.Horizontal,
						HeightRequest = 50,
						Children = { nombreCampo, valorCampo }
					};

					ContenedorProducto.Children.Add(campoValor);

					//Si es columna de stock agrega campo movimiento y precio
					if(!string.IsNullOrEmpty(celda) && _listaColumnasInventario != null && _listaColumnasInventario[i] == "1")
					{
						#region Movimiento stock

						_signoPositivo.SetValue(true, i);

						nombreCampo = new Label
						{
							HorizontalOptions = LayoutOptions.EndAndExpand,
							VerticalOptions = LayoutOptions.Center,
							HorizontalTextAlignment = TextAlignment.End,
							Text = "Movimiento",
							FontSize = 16,
							WidthRequest = anchoEtiqueta - 13,
							TextColor = Color.Black
						};

						var botonSigno = new Button
						{
							Text = "+",
							HorizontalOptions = LayoutOptions.Center,
							VerticalOptions = LayoutOptions.Center,
							FontSize = 25,
							HeightRequest = 60,
							WidthRequest = 60,
							StyleId = i.ToString()
						};

						botonSigno.Clicked += DefinirSigno;

						valorCampo = new Entry
						{
							HorizontalOptions = LayoutOptions.StartAndExpand,
							VerticalOptions = LayoutOptions.Center,
							HorizontalTextAlignment = TextAlignment.Start,
							StyleId = "movimiento-" + i,
							WidthRequest = anchoCampo - 55,
							Keyboard = Keyboard.Numeric
						};

						campoValor = new StackLayout
						{
							BackgroundColor = Color.FromHex("#FFFFFF"),
							HorizontalOptions = LayoutOptions.FillAndExpand,
							VerticalOptions = LayoutOptions.CenterAndExpand,
							Orientation = StackOrientation.Horizontal,
							HeightRequest = 50,
							Children = { nombreCampo, botonSigno, valorCampo }
						};

						ContenedorProducto.Children.Add(campoValor);

						#endregion

						#region Precio movimiento

						nombreCampo = new Label
						{
							HorizontalOptions = LayoutOptions.EndAndExpand,
							VerticalOptions = LayoutOptions.Center,
							HorizontalTextAlignment = TextAlignment.End,
							Text = "Precio Total",
							FontSize = 16,
							WidthRequest = anchoEtiqueta,
							TextColor = Color.Black
						};

						valorCampo = new Entry
						{
							HorizontalOptions = LayoutOptions.CenterAndExpand,
							VerticalOptions = LayoutOptions.Center,
							HorizontalTextAlignment = TextAlignment.Start,
							StyleId = "precio-" + i,
							WidthRequest = anchoCampo - 55,
							Keyboard = Keyboard.Numeric
						};

						campoValor = new StackLayout
						{
							BackgroundColor = Color.FromHex("#FFFFFF"),
							HorizontalOptions = LayoutOptions.FillAndExpand,
							VerticalOptions = LayoutOptions.CenterAndExpand,
							Orientation = StackOrientation.Horizontal,
							HeightRequest = 50,
							Children = { nombreCampo, valorCampo }
						};

						ContenedorProducto.Children.Add(campoValor);

						#endregion

						#region Punto de venta
						if(_listaLugares != null) { 
							nombreCampo = new Label
							{
								HorizontalOptions = LayoutOptions.EndAndExpand,
								VerticalOptions = LayoutOptions.Center,
								HorizontalTextAlignment = TextAlignment.End,
								Text = "Punto de venta",
								FontSize = 16,
								WidthRequest = anchoEtiqueta,
								TextColor = Color.Black
							};

							var puntoVenta = new Picker
							{
								HorizontalOptions = LayoutOptions.CenterAndExpand,
								VerticalOptions = LayoutOptions.Center,
								StyleId = "punto-" + i,
								WidthRequest = anchoCampo - 55
							};
							foreach (var punto in _listaLugares)
							{
								puntoVenta.Items.Add(punto);
							}

							campoValor = new StackLayout
							{
								BackgroundColor = Color.FromHex("#FFFFFF"),
								HorizontalOptions = LayoutOptions.FillAndExpand,
								VerticalOptions = LayoutOptions.CenterAndExpand,
								Orientation = StackOrientation.Horizontal,
								HeightRequest = 50,
								Children = { nombreCampo, puntoVenta }
							};

							ContenedorProducto.Children.Add(campoValor);
						}
						#endregion
					}
				}

				i = i + 1;
			}

		}

		private void DefinirSigno(object sender, EventArgs e)
		{
			var boton = (Button)sender;
			var columna = Convert.ToInt32(boton.StyleId);
			boton.Text = _signoPositivo[columna] ? "-" : "+";
			_signoPositivo.SetValue(boton.Text == "+", columna);
		}

		[Android.Runtime.Preserve]
		private async void GuardarCambios(object sender, EventArgs args)
		{
			foreach (var stackLayout in ContenedorProducto.Children)
			{
				foreach (var control in ((StackLayout)stackLayout).Children)
				{
					int columna;
					string valor;
					if (control.StyleId != null && control.StyleId.Contains("movimiento-"))
					{
						columna = Convert.ToInt32(control.StyleId.Split('-')[1]);
						valor = ((Entry)control).Text;
						valor = !string.IsNullOrEmpty(valor) ? valor.Replace('.',',') : "0"; //Todos los decimales con coma, evita problema de cultura.
						_movimientos.SetValue(Convert.ToDouble(valor), columna);
					}

					if (control.StyleId != null && control.StyleId.Contains("precio-"))
					{
						columna = Convert.ToInt32(control.StyleId.Split('-')[1]);
						valor = ((Entry)control).Text;
						valor = !string.IsNullOrEmpty(valor) ? valor.Replace('.', ',') : "0"; //Todos los decimales con coma, evita problema de cultura.
						_precios.SetValue(Convert.ToDouble(valor), columna);
					}

					if (_listaLugares != null && control.StyleId != null && control.StyleId.Contains("punto-"))
					{
						columna = Convert.ToInt32(control.StyleId.Split('-')[1]);
						var combo = (Picker)control;
						valor = combo.SelectedIndex != -1 ? combo.Items[combo.SelectedIndex] : "-";
						_lugares.SetValue(valor, columna);
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

		[Android.Runtime.Preserve]
		private async void AccederMovimientos(object sender, EventArgs args)
		{
			await Navigation.PushAsync(new ProductoMovimientos(_producto, _servicio), true);
		}

		[Android.Runtime.Preserve]
		private async void Volver(object sender, EventArgs args)
		{
			await Navigation.PopAsync();
		}

		private async void GuardarProductoHojaDeCalculoGoogle()
		{
			_mensaje = "Ha ocurrido un error mientras se guardaba el movimiento.";
			var servicioGoogle = new ServiciosGoogle();
			var grabo = false;
			foreach (var celda in _producto)
			{
				if (_listaColumnasInventario[(int)celda.Column - 1] == "1")
				{
					var multiplicador = _signoPositivo[(int)celda.Column - 1] ? 1 : -1;
					var movimiento = _movimientos[(int)celda.Column - 1];
					var precio = _precios[(int)celda.Column - 1];
					var lugar = _listaLugares != null ? _lugares[(int)celda.Column - 1] : "No tiene configurado.";

					if (movimiento != 0)
					{
						//celda.InputValue = (Convert.ToDouble(celda.InputValue) + multiplicador * movimiento).ToString();

						try
						{
							// Actualiza la celda en Google
							//celda.Update();
							// Inserta histórico en Google
							servicioGoogle.InsertarHistoricos(_servicio, celda, multiplicador * movimiento, precio, lugar, _producto, _nombresColumnas, _listaColumnasInventario);
							
							grabo = true;
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
			_mensaje = grabo ? "El movimiento ha sido guardado correctamente." : "No se han registrado movimientos.";
		}

		private void GuardarProductoBaseDeDatos()
		{
			_mensaje = "Ha ocurrido un error mientras se guardaba el movimiento.";
			//const string url = @"http://169.254.80.80/PruebaMision/Service.asmx/ActualizarProducto?codigo={0}&movimiento={1}";
			var i = 0;
			var grabo = false;

			foreach (var celda in _productoString)
			{
				if (_listaColumnasInventario[i] == "1")
				{
					//var multiplicador = _signoPositivo[i] ? 1 : -1;
					var movimiento = _movimientos[i];

					if (movimiento != 0)
					{
						using (var cliente = new HttpClient())
						{
							grabo = true; //await cliente.GetStringAsync(string.Format(url, _productoString[0], (Convert.ToDouble(celda) + multiplicador * movimiento)));
						}
					}
				}

				i = i + 1;
			}
			_mensaje = grabo ? "El movimiento ha sido guardado correctamente." : "No se han registrado movimientos.";
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
