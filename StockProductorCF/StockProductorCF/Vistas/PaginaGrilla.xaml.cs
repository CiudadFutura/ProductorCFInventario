
using Google.GData.Spreadsheets;
using StockProductorCF.Clases;
using StockProductorCF.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Windows.UI.Xaml;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;
using DataTemplate = Xamarin.Forms.DataTemplate;
using TextAlignment = Xamarin.Forms.TextAlignment;

namespace StockProductorCF.Vistas
{
	public partial class PaginaGrilla
	{
		private readonly ServiciosGoogle _servicioGoogle;
		private SpreadsheetsService _servicio;
		private CellFeed _celdas;
		private string _linkHojaConsulta;
		private string[] _nombresColumnas;
		private string[] _listaColumnasParaVer;
		private string[] _listaColumnasInventario;
		private ViewCell _ultimoItemSeleccionado;
		private Color _ultimoColorSeleccionado;
		private List<string[]> _productos;
		private bool _esTeclaPar;
		private bool _esCargaInicial;
		private ActivityIndicator _indicadorActividad;

		//Constructor para Hoja de cálculo de Google
		public PaginaGrilla(string linkHojaConsulta, SpreadsheetsService servicio)
		{
			InitializeComponent();

			_linkHojaConsulta = linkHojaConsulta;
			_servicio = servicio;
			_servicioGoogle = new ServiciosGoogle();

			InicializarVariablesGlobales();
			//La carga de productos se realiza cuando se selecciona el índice del selector de hojas.
			ConfigurarSelectorHojas();
		}

		//Constructor para Base de Datos
		public PaginaGrilla()
		{
			InitializeComponent();
			InicializarVariablesGlobales();
			ObtenerProductosDesdeBD();
		}

		#region Métodos para Hoja de cálculo de Google

		private async void ObtenerDatosProductosDesdeHCG()
		{

			if (_servicio == null) //El servicio viene nulo cuando se llama directamente desde el lanzador (ya tiene conexión a datos configurada)
				_servicio = _servicioGoogle.ObtenerServicioParaConsultaGoogleSpreadsheets(CuentaUsuario.ObtenerTokenActualDeGoogle());

			if(CuentaUsuario.ValidarTokenDeGoogle())
			{
				_celdas = _servicioGoogle.ObtenerCeldasDeUnaHoja(_linkHojaConsulta, _servicio);
			}
			else
			{
				//Si se quedó la pantalla abierta un largo tiempo y se venció el token, se cierra y refresca el token
				var paginaAuntenticacion = new PaginaAuntenticacion(true);
				Navigation.InsertPageBefore(paginaAuntenticacion, this);
				await Navigation.PopAsync();
			}

			_nombresColumnas = new string[_celdas.ColCount.Count];

			var productos = new List<string[]>();
			var producto = new string[_celdas.ColCount.Count];

			foreach (CellEntry celda in _celdas.Entries)
			{
				if (celda.Row != 1)
				{
					if (celda.Column == 1)
						producto = new string[_celdas.ColCount.Count];

					producto.SetValue(celda.Value, (int)celda.Column - 1);

					if (celda.Column == _celdas.ColCount.Count)
						productos.Add(producto);
				}
				else
				{
					_nombresColumnas.SetValue(celda.Value, (int)celda.Column - 1);
				}
			}
			
			LlenarGrillaDeProductos(productos);
		}

		private void ConfigurarSelectorHojas()
		{
			ListaHojas.IsVisible = true;
			var nombreHojaActual = CuentaUsuario.ObtenerNombreHoja(_linkHojaConsulta);
			var nombres = CuentaUsuario.ObtenerTodosLosNombresDeHojas();
			var i = 0;
			foreach (var nombre in nombres)
			{
				ListaHojas.Items.Add(nombre);
				if (nombre == nombreHojaActual)
					ListaHojas.SelectedIndex = i; //Esto genera la carga inicial de productos.
				i += 1;
			}
		}

		#endregion

		#region Métodos para Base de Datos

		private async void ObtenerProductosDesdeBD()
		{
			var url = $@"http://169.254.80.80/PruebaMision/Service.asmx/RecuperarProductos?token={
					CuentaUsuario.ObtenerTokenActualDeBaseDeDatos()
				}";

			using (var cliente = new HttpClient())
			{
				//Obtiene json de productos desde el webservice
				var jsonProductos = await cliente.GetStringAsync(url);
				//Parsea el json para obtener la lista de productos
				var productos = ParsearJSONProductos(jsonProductos);

				_nombresColumnas = new[] { "Código", "Nombre", "Stock" };
				LlenarGrillaDeProductos(productos);
			}
		}

		private static List<string[]> ParsearJSONProductos(string jsonProductos)
		{
			jsonProductos = jsonProductos.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<string xmlns=\"http://tempuri.org/\">[{", "")
				.Replace("}]</string>", "")
				.Replace("},{", "|");
			var arregloProductos = jsonProductos.Split('|');
			var productos = new List<string[]>();

			foreach (var datos in arregloProductos)
			{
				var temporal = datos.Split(',');

				//Si el precio es diferente de 0.0 lo agregamos
				if (temporal[1].Split(':')[1].TrimStart('"').TrimEnd('"') != "0.0")
				{
					var producto = new string[3];
					producto[0] = temporal[0].Split(':')[1].TrimStart('"').TrimEnd('"');
					producto[1] = temporal[2].Split(':')[1].TrimStart('"').TrimEnd('"');
					var stock = temporal[18].Split(':')[1].TrimStart('"').TrimEnd('"');
					producto[2] = stock == "null" ? "0" : stock;

					productos.Add(producto);
				}
			}

			return productos;
		}

		#endregion

		#region Métodos comunes

		private void InicializarVariablesGlobales()
		{
			ConfigurarBotones();
			Cabecera.Source = App.ImagenCabeceraCiudadFutura;

			var columnasParaVer = CuentaUsuario.ObtenerColumnasParaVer();
			if (!string.IsNullOrEmpty(columnasParaVer))
				_listaColumnasParaVer = columnasParaVer.Split(',');

			var columnasInventario = CuentaUsuario.ObtenerColumnasInventario();
			if (!string.IsNullOrEmpty(columnasInventario))
				_listaColumnasInventario = columnasInventario.Split(',');

			_indicadorActividad = new ActivityIndicator { IsRunning = true, VerticalOptions = LayoutOptions.CenterAndExpand };
		}

		private void ConfigurarBotones()
		{
			Datos.Source = ImageSource.FromResource($"StockProductorCF.Imagenes.accesoDatos{App.Sufijo}.png");
			Datos.GestureRecognizers.Add(new TapGestureRecognizer(AccederDatos));
			Refrescar.Source = ImageSource.FromResource($"StockProductorCF.Imagenes.refrescarDatos{App.Sufijo}.png");
			Refrescar.GestureRecognizers.Add(new TapGestureRecognizer(RefrescarDatos));
			Escanear.Source = ImageSource.FromResource($"StockProductorCF.Imagenes.escanearCodigo{App.Sufijo}.png");
			Escanear.GestureRecognizers.Add(new TapGestureRecognizer(AbrirPaginaEscaner));
		}

		private void LlenarGrillaDeProductos(List<string[]> productos, bool esBusqueda = false)
		{
			//Se carga la grilla de productos y se muestra en pantalla.
			ConstruirVistaDeLista(productos);
			if (!esBusqueda)
				FijarProductosYBuscador(productos);
		}

		private void IrAlProducto(string codigoProductoSeleccionado)
		{
			if (CuentaUsuario.ObtenerAccesoDatos() == "G")
			{
				var fila = -1;
				var productoSeleccionado = new CellEntry[_celdas.ColCount.Count];

				//Obtener el arreglo del producto para enviar
				foreach (CellEntry celda in _celdas.Entries)
				{
					if (celda.Column == 1 && celda.Value == codigoProductoSeleccionado)
						fila = (int)celda.Row;
					if (celda.Row == fila)
						productoSeleccionado.SetValue(celda, (int)celda.Column - 1);
					if (fila > -1 && celda.Row > fila)
						break;
				}

				Navigation.PushAsync(new Producto(productoSeleccionado, _nombresColumnas, _servicio));
			}
			else
			{
				foreach (var producto in _productos)
				{
					if (producto[0] != codigoProductoSeleccionado) continue;
					Navigation.PushAsync(new Producto(producto, _nombresColumnas));
					break;
				}

			}
		}

		private void ConstruirVistaDeLista(List<string[]> productos)
		{
			var listaProductos = new List<ClaseProducto>();
			foreach (var datosProducto in productos)
			{
				var datosParaVer = new List<string>();
				var i = 0;
				foreach (var dato in datosProducto)
				{
					var textoDato = "";

					if (_listaColumnasParaVer != null && _listaColumnasParaVer[i] == "1")
					{
						if (_listaColumnasInventario[i] == "1")
							textoDato += _nombresColumnas[i] + " : ";

						textoDato += dato;
						datosParaVer.Add(textoDato);
					}
					i = i + 1;
				}

				var producto = new ClaseProducto(datosProducto[0], datosParaVer);
				listaProductos.Add(producto);
			}

			var encabezado = new StackLayout
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.Start,
				BackgroundColor = Color.FromHex("#C0C0C0"),
				HeightRequest = productos.Count <= 25 ? 30 : 50,
				Children =
								{
									new Label
									{
										Text = "    PRODUCTOS                      INFO + STOCK",
										FontSize = 13,
										HorizontalOptions = LayoutOptions.Start,
										FontAttributes = FontAttributes.Bold,
										TextColor = Color.Black,
										VerticalTextAlignment = TextAlignment.Center,
										VerticalOptions = LayoutOptions.Center
									}
								}
			};

			var vista = new ListView
			{
				RowHeight = 60,
				VerticalOptions = LayoutOptions.StartAndExpand,
				HorizontalOptions = LayoutOptions.Fill,
				ItemsSource = listaProductos,
				ItemTemplate = new DataTemplate(() =>
				{
					var nombreProducto = new Label
					{
						FontSize = 16,
						TextColor = Color.FromHex("#1D1D1B"),
						FontAttributes = FontAttributes.Bold,
						VerticalOptions = LayoutOptions.CenterAndExpand,
						WidthRequest = 130
					};
					nombreProducto.SetBinding(Label.TextProperty, "Nombre");

					var datos = new Label
					{
						FontSize = 15,
						TextColor = Color.FromHex("#1D1D1B"),
						VerticalOptions = LayoutOptions.CenterAndExpand,
						WidthRequest = App.AnchoDePantalla - 132
					};
					datos.SetBinding(Label.TextProperty, "Datos");

					var separador = new BoxView
					{
						WidthRequest = 2,
						BackgroundColor = Color.FromHex("#FFFFFF"),
						HeightRequest = 55
					};
					
					var celda = new ViewCell
					{
						View = new StackLayout
						{
							Padding = 2,
							Orientation = StackOrientation.Horizontal,
							Children =
										{
											nombreProducto,
											separador,
											new StackLayout
											{
												Orientation = StackOrientation.Vertical,
												Spacing = 0,
												Children = { datos }
											}
										}
						}
					};
					
					celda.Tapped += (sender, args) =>
					{
						if (_ultimoItemSeleccionado != null)
							_ultimoItemSeleccionado.View.BackgroundColor = _ultimoColorSeleccionado;
						IrAlProducto(((ClaseProducto)((ViewCell)sender).BindingContext).Id);
						_ultimoColorSeleccionado = celda.View.BackgroundColor;
						celda.View.BackgroundColor = Color.Silver;
						_ultimoItemSeleccionado = (ViewCell)sender;
					};

					celda.Appearing += (sender, args) =>
					{
						var viewCell = (ViewCell)sender;
						if (viewCell.View != null)
						{
							viewCell.View.BackgroundColor = _esTeclaPar ? Color.FromHex("#EDEDED") : Color.FromHex("#E2E2E1");
						}

						_esTeclaPar = !_esTeclaPar;
					};

					return celda;
				})
			};

			ContenedorTabla.Children.Clear(); //Remueve el Indicador de Actividad.
			ContenedorTabla.Children.Add(encabezado);
			ContenedorTabla.Children.Add(vista);
		}

		private void FijarProductosYBuscador(List<string[]> productos)
		{
			//Si hay más de 25 productos se muestra el buscador
			if (productos.Count <= 25) return;
			//Almacena la lista de productos en la variable global que usará el buscador
			_productos = productos;
			Buscador.IsVisible = true;
			_esCargaInicial = true;
			Buscador.Text = "";
		}

		private void RefrescarUIGrilla()
		{
			//Se quita la grilla para recargarla.
			ContenedorTabla.Children.Clear();
			Buscador.IsVisible = false;
			ContenedorTabla.Children.Add(_indicadorActividad);
		}

		#endregion

		#region Eventos

		[Android.Runtime.Preserve]
		private void AccederDatos(View arg1, object arg2)
		{
			Datos.Opacity = 0.5f;
			Device.StartTimer(TimeSpan.FromMilliseconds(300), () => {
				var paginaAccesoDatos = new AccesoDatos();
				Navigation.PushAsync(paginaAccesoDatos);
				Datos.Opacity = 1f;
				return false;
			});
		}

		[Android.Runtime.Preserve]
		private void RefrescarDatos(View arg1, object arg2)
		{
			RefrescarUIGrilla();
			Refrescar.Opacity = 0.5f;

			Device.StartTimer(TimeSpan.FromMilliseconds(100), () => {
				if (!string.IsNullOrEmpty(_linkHojaConsulta))
					ObtenerDatosProductosDesdeHCG(); //Hoja de cálculo de Google
				else
					ObtenerProductosDesdeBD(); //Base de Datos
				Refrescar.Opacity = 1f;
				return false;
			});
		}
		
		[Android.Runtime.Preserve]
		private void AbrirPaginaEscaner(View arg1, object arg2)
		{

			Escanear.Opacity = 0.5f;
			Device.StartTimer(TimeSpan.FromMilliseconds(300), () => {
				var paginaEscaner = new ZXingScannerPage();

				paginaEscaner.OnScanResult += (result) =>
				{
					// Detiene el escaner
					paginaEscaner.IsScanning = false;

					//Hace autofoco, particularmente para los códigos de barra
					var ts = new TimeSpan(0, 0, 0, 3, 0);
					Device.StartTimer(ts, () =>
					{
						if (paginaEscaner.IsScanning)
							paginaEscaner.AutoFocus();
						return true;
					});

					// Cierra la página del escaner y llama a la página del producto
					Device.BeginInvokeOnMainThread(() =>
					{
						Navigation.PopModalAsync();
						IrAlProducto(result.Text);
					});
				};

				// Abre la página del escaner
				Navigation.PushModalAsync(paginaEscaner);

				Escanear.Opacity = 1f;
				return false;
			});
			
		}

		[Android.Runtime.Preserve]
		private void FiltrarProductos(object sender, EventArgs args)
		{
			if (Buscador.Text.Length > 2 || Buscador.Text.Length == 0 && !_esCargaInicial)
			{
				//Se quita la grilla para recargarla.
				ContenedorTabla.Children.Clear();
				var productos = new List<string[]>();
				foreach (var producto in _productos)
				{
					if (producto[1].ToLower().Contains(Buscador.Text.ToLower()))
						productos.Add(producto);
				}

				LlenarGrillaDeProductos(productos, true);
				_esCargaInicial = false;
			}
		}

		[Android.Runtime.Preserve]
		private void CargarHoja(object sender, EventArgs args)
		{
			RefrescarUIGrilla();

			Device.StartTimer(TimeSpan.FromMilliseconds(100), () => {
				_linkHojaConsulta = CuentaUsuario.ObtenerLinkHojaSeleccionada(ListaHojas.Items[ListaHojas.SelectedIndex]);
				_listaColumnasParaVer = CuentaUsuario.ObtenerColumnasParaVer().Split(',');
				_listaColumnasInventario = CuentaUsuario.ObtenerColumnasInventario().Split(',');
				ObtenerDatosProductosDesdeHCG();
				return false;
			});
		}

		#endregion

	}

	//Clase Producto: utilizada para armar la lista scrolleable de productos
	[Android.Runtime.Preserve]
	public class ClaseProducto
	{
		[Android.Runtime.Preserve]
		public ClaseProducto(string id, IList<string> datos)
		{
			Id = id;
			Nombre = datos[0];
			Datos = string.Join(" - ", datos.Skip(1).Take(datos.Count));
		}

		[Android.Runtime.Preserve]
		public string Id { get; }
		[Android.Runtime.Preserve]
		public string Nombre { get; }
		[Android.Runtime.Preserve]
		public string Datos { get; }
	}

}
