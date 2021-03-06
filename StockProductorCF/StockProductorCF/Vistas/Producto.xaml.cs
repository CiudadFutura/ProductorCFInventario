﻿
using System;
using System.IO;
using System.Net;
using Google.GData.Spreadsheets;
using Xamarin.Forms;
using StockProductorCF.Clases;
using StockProductorCF.Servicios;
using System.Text;
using System.Threading.Tasks;

namespace StockProductorCF.Vistas
{
	public partial class Producto
	{
		private bool[] _signoPositivo;
		private double[] _cantidades;
		private double[] _precios;
		private string[] _lugares;
		private string _comentario;
		private readonly CellEntry[] _producto;
		private string[] _listaColumnasInventario;
		private string[] _listaLugares;
		private readonly string[] _productoString;
		private readonly SpreadsheetsService _servicio;
		private readonly string[] _nombresColumnas;
		private string _mensaje = "";
		private double _anchoActual;
		private ActivityIndicator _indicadorActividad;
		private Image _volver;
		private Image _movimientos;
		private Image _guardarCambios;
		private bool _esGoogle;
		private double _stockBd;
		private static bool _terminoGuardadoBd;

		public Producto(CellEntry[] producto, string[] nombresColumnas, SpreadsheetsService servicio, string titulo)
		{
			InitializeComponent();
			InicializarValoresGenerales(titulo);
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

			ConstruirVistaDeProducto();
		}

		public Producto(string[] productoBD, string[] nombresColumnas, string titulo)
		{
			InitializeComponent();
			InicializarValoresGenerales(titulo);
			_productoString = productoBD;
			_nombresColumnas = nombresColumnas;
			_terminoGuardadoBd = false;

			ConstruirVistaDeProducto();
		}

		private void InicializarValoresGenerales(string titulo)
		{
			_esGoogle = CuentaUsuario.ObtenerAccesoDatos() == "G";
			Titulo.Text += " " + titulo.Replace("App", "").Replace("es ", " ").Replace("s ", " ");
			Cabecera.Children.Add(App.Instancia.ObtenerImagen(TipoImagen.EncabezadoProductores));
			SombraEncabezado.Source = ImageSource.FromResource(App.RutaImagenSombraEncabezado);

			ConfigurarBotones();

			_indicadorActividad = new ActivityIndicator
			{
				VerticalOptions = LayoutOptions.CenterAndExpand,
				IsEnabled = true,
				BindingContext = this
			};
			_indicadorActividad.SetBinding(IsVisibleProperty, "IsBusy");
			_indicadorActividad.SetBinding(ActivityIndicator.IsRunningProperty, "IsBusy");
		}

		private void ConfigurarBotones()
		{
			_volver = App.Instancia.ObtenerImagen(TipoImagen.BotonVolver);
			_volver.GestureRecognizers.Add(new TapGestureRecognizer(Volver));
			_movimientos = App.Instancia.ObtenerImagen(TipoImagen.BotonMovimientos);
			_movimientos.GestureRecognizers.Add(new TapGestureRecognizer(AccederMovimientos));
			_guardarCambios = App.Instancia.ObtenerImagen(TipoImagen.BotonGuardarCambios);
			_guardarCambios.GestureRecognizers.Add(new TapGestureRecognizer(EventoGuardarCambios));

			ContenedorBotones.Children.Add(_volver);
			ContenedorBotones.Children.Add(_movimientos);
			ContenedorBotones.Children.Add(_guardarCambios);
		}

		private void ConstruirVistaDeProducto()
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
			_cantidades = new double[_productoString.Length];
			_precios = new double[_productoString.Length];
			_lugares = new string[_productoString.Length];
			var i = 0;

			var anchoEtiqueta = App.AnchoRetratoDePantalla / 3 - 10;
			var anchoCampo = App.AnchoRetratoDePantalla / 3 * 2 - 30;

			Label nombreCampo;
			StackLayout campoValor;

			#region Campos de planilla
			foreach (var celda in _productoString)
			{
				if (celda != null)
				{
					#region Datos en planilla o BD

					nombreCampo = new Label
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

					campoValor = new StackLayout
					{
						VerticalOptions = LayoutOptions.Start,
						HorizontalOptions = LayoutOptions.Fill,
						Orientation = StackOrientation.Horizontal,
						HeightRequest = 50,
						Children = { nombreCampo, valorCampo }
					};

					ContenedorProducto.Children.Add(campoValor);

					#endregion

					//Si es columna de stock agrega el campo Cantidad. Si tiene lugar de compra/venta agrega los campos Precio total y Lugar.
					if (!string.IsNullOrEmpty(celda) && _listaColumnasInventario != null && _listaColumnasInventario[i] == "1")
					{
						#region Movimiento stock

						_signoPositivo.SetValue(true, i);

						nombreCampo = new Label
						{
							HorizontalOptions = LayoutOptions.EndAndExpand,
							VerticalOptions = LayoutOptions.Center,
							HorizontalTextAlignment = TextAlignment.End,
							Text = _listaLugares != null || !_esGoogle ? "Cantidad" : "Precio Total", //Si no hay lugares no hay campo PrecioTotal, el campo Cantidad toma esa etiqueta.
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
							VerticalOptions = LayoutOptions.Start,
							HorizontalOptions = LayoutOptions.Fill,
							Orientation = StackOrientation.Horizontal,
							HeightRequest = 60,
							Children = { nombreCampo, botonSigno, valorCampo }
						};

						ContenedorProducto.Children.Add(campoValor);

						#endregion

						if (_listaLugares != null)
						{

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
								VerticalOptions = LayoutOptions.Start,
								HorizontalOptions = LayoutOptions.Fill,
								Orientation = StackOrientation.Horizontal,
								HeightRequest = 60,
								Children = { nombreCampo, valorCampo }
							};

							ContenedorProducto.Children.Add(campoValor);

							#endregion

							#region Lugar de compra/venta

							nombreCampo = new Label
							{
								HorizontalOptions = LayoutOptions.EndAndExpand,
								VerticalOptions = LayoutOptions.Center,
								HorizontalTextAlignment = TextAlignment.End,
								Text = "Lugar",
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
								VerticalOptions = LayoutOptions.Start,
								HorizontalOptions = LayoutOptions.Fill,
								Orientation = StackOrientation.Horizontal,
								HeightRequest = 60,
								Children = { nombreCampo, puntoVenta }
							};

							ContenedorProducto.Children.Add(campoValor);

							#endregion

						}
					}
				}

				i = i + 1;
			}

			#endregion

			#region Comentario

			if (_esGoogle)
			{
				nombreCampo = new Label
				{
					HorizontalOptions = LayoutOptions.EndAndExpand,
					VerticalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.End,
					Text = "Comentario",
					FontSize = 16,
					WidthRequest = anchoEtiqueta,
					TextColor = Color.Black
				};

				var valorCampoArea = new Editor
				{
					HorizontalOptions = LayoutOptions.CenterAndExpand,
					VerticalOptions = LayoutOptions.Center,
					StyleId = "comentario",
					WidthRequest = anchoCampo,
					HeightRequest = 90,
					Keyboard = Keyboard.Text
				};

				campoValor = new StackLayout
				{
					BackgroundColor = Color.FromHex("#FFFFFF"),
					VerticalOptions = LayoutOptions.Start,
					HorizontalOptions = LayoutOptions.Fill,
					Orientation = StackOrientation.Horizontal,
					HeightRequest = 90,
					Children = { nombreCampo, valorCampoArea }
				};

				ContenedorProducto.Children.Add(campoValor);
			}

			#endregion

		}

		private void DefinirSigno(object sender, EventArgs e)
		{
			var boton = (Button)sender;
			var columna = Convert.ToInt32(boton.StyleId);
			boton.Text = _signoPositivo[columna] ? "-" : "+";
			_signoPositivo.SetValue(boton.Text == "+", columna);
		}

		[Android.Runtime.Preserve]
		private void EventoGuardarCambios(View arg1, object arg2)
		{
			_guardarCambios.Opacity = 0.5f;
			Device.StartTimer(TimeSpan.FromMilliseconds(300), () =>
			{
				GuardarCambios();
				_guardarCambios.Opacity = 1f;
				return false;
			});
		}

		private async void GuardarCambios()
		{
			await TareaGuardarCambios();

			await Navigation.PopAsync();
			await DisplayAlert("Producto", _mensaje, "Listo");
		}

		private async Task TareaGuardarCambios()
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
						valor = !string.IsNullOrEmpty(valor)
							? valor.Replace('.', ',')
							: "0"; //Todos los decimales con coma, evita problema de cultura.
						_cantidades.SetValue(Convert.ToDouble(valor), columna);
					}

					if (control.StyleId != null && control.StyleId.Contains("precio-"))
					{
						columna = Convert.ToInt32(control.StyleId.Split('-')[1]);
						valor = ((Entry)control).Text;
						valor = !string.IsNullOrEmpty(valor)
							? valor.Replace('.', ',')
							: "0"; //Todos los decimales con coma, evita problema de cultura.
						_precios.SetValue(Convert.ToDouble(valor), columna);
					}

					if (_listaLugares != null && control.StyleId != null && control.StyleId.Contains("punto-"))
					{
						columna = Convert.ToInt32(control.StyleId.Split('-')[1]);
						var combo = (Picker)control;
						valor = combo.SelectedIndex != -1 ? combo.Items[combo.SelectedIndex] : "-";
						_lugares.SetValue(valor, columna);
					}

					if (control.StyleId != null && control.StyleId.Contains("comentario"))
					{
						valor = ((Editor)control).Text;
						_comentario = valor;
					}

				}
			}

			ContenedorProducto.Children.Clear();
			ContenedorProducto.Children.Add(_indicadorActividad);

			try
			{
				IsBusy = true;

				await Task.Run(async () =>
				{
					if (_esGoogle)
					{
						if (CuentaUsuario.ValidarTokenDeGoogle())
						{
							await GuardarProductoHojaDeCalculoGoogle();
						}
						else
						{
							//Si se quedó la pantalla abierta un largo tiempo y se venció el token, se cierra y refresca el token
							var paginaAuntenticacion = new PaginaAuntenticacion(true);
							Navigation.InsertPageBefore(paginaAuntenticacion, this);
							await Navigation.PopAsync();
						}
					}
					else
						GuardarProductoBaseDeDatos();
					
				});
			}
			finally
			{
				IsBusy = false; //Remueve el Indicador de Actividad.
			}

		}

		[Android.Runtime.Preserve]
		private void AccederMovimientos(View arg1, object arg2)
		{
			_movimientos.Opacity = 0.5f;
			Device.StartTimer(TimeSpan.FromMilliseconds(300), () =>
			{
				if (_esGoogle)
					Navigation.PushAsync(new ProductoMovimientos(_producto, _servicio), true);
				//else
					
				_movimientos.Opacity = 1f;
				return false;
			});
		}

		[Android.Runtime.Preserve]
		private void Volver(View arg1, object arg2)
		{
			_volver.Opacity = 0.5f;
			Device.StartTimer(TimeSpan.FromMilliseconds(300), () =>
			{
				Navigation.PopAsync();
				_volver.Opacity = 1f;
				return false;
			});
		}

		private async Task GuardarProductoHojaDeCalculoGoogle()
		{
			_mensaje = "Ha ocurrido un error mientras se guardaba el movimiento.";
			var servicioGoogle = new ServiciosGoogle();
			var grabo = false;
			foreach (var celda in _producto)
			{
				if (_listaColumnasInventario[(int)celda.Column - 1] == "1")
				{
					var multiplicador = _signoPositivo[(int)celda.Column - 1] ? 1 : -1;
					var cantidad = _cantidades[(int)celda.Column - 1];
					var precio = _precios[(int)celda.Column - 1];
					var lugar = _listaLugares != null ? _lugares[(int)celda.Column - 1] : "No tiene configurado.";

					if (cantidad != 0)
					{
						try
						{
							// Si no hay lugares no hay campo de PrecioTotal, entonces el precio lo toma de la cantidad
							if (_listaLugares == null)
								precio = multiplicador * cantidad;

							//Ingresa el movimiento de existencia (entrada - salida) en la tabla principal
							servicioGoogle.EnviarMovimiento(_servicio, celda, multiplicador * cantidad, precio, lugar, _comentario, _producto, _nombresColumnas,
								_listaColumnasInventario, CuentaUsuario.ObtenerLinkHojaHistoricos());
							//Si es página principal y tiene las relaciones insumos - productos, ingresa los movimientos de insumos
							if (multiplicador == 1) //Si es ingreso positivo
								servicioGoogle.InsertarMovimientosRelaciones(_servicio, cantidad, _producto);

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
			var grabo = false;

			for (var i = 0; i < _productoString.Length; i++)
			{
				if (_listaColumnasInventario[i] == "1")
				{
					var movimiento = (_signoPositivo[i] ? 1 : -1) * _cantidades[i];

					if (movimiento != 0)
					{

						_stockBd = movimiento + (string.IsNullOrEmpty(_productoString[4]) ? 0 : Convert.ToDouble(_productoString[4]));
						var request = HttpWebRequest.Create(
							$"https://misionantiinflacion.com.ar/api/v1/products/{_productoString[0]}?token=09a68ef6ec3e6438bb2a6d809c3bfba3f70c054f6eb62470467b197fff2c150e") as HttpWebRequest;
						request.Method = "PUT";
						request.ContentType = "application/json";
						request.BeginGetRequestStream(GetRequestStreamCallback, request);

						while (!_terminoGuardadoBd)
						{
						}

						grabo = true;
					}
				}

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

		private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
		{
			var request = (HttpWebRequest)asynchronousResult.AsyncState;
			// End the stream request operation

			var postStream = request.EndGetRequestStream(asynchronousResult);

			// Create the post data
			var postData = "{\"stock\":" + _stockBd + "}";

			var byteArray = Encoding.UTF8.GetBytes(postData);


			postStream.Write(byteArray, 0, byteArray.Length);
			postStream.Dispose();

			//Start the web request
			request.BeginGetResponse(GetResponceStreamCallback, request);
		}

		private static void GetResponceStreamCallback(IAsyncResult callbackResult)
		{
			var request = (HttpWebRequest)callbackResult.AsyncState;
			request.EndGetResponse(callbackResult);
			_terminoGuardadoBd = true;
		}
	}
}
