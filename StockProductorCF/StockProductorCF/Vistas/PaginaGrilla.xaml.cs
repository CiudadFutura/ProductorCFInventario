
using Google.GData.Spreadsheets;
using StockProductorCF.Clases;
using StockProductorCF.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace StockProductorCF.Vistas
{
	public partial class PaginaGrilla : ContentPage
	{
		private ServiciosGoogle _servicioGoogle;
		private SpreadsheetsService _servicio;
		private CellFeed _celdas;
		private string _linkHojaConsulta;
		private string[] _nombresColumnas;
		private string[] _listaColumnasParaVer;
		private string[] _listacolumnasInventario;
		private bool _seleccionoItem;
		private ViewCell _ultimoItemSeleccionado;
		private List<string[]> _productos;

		//Constructor para Hoja de cálculo de Google
		public PaginaGrilla(string linkHojaConsulta, SpreadsheetsService servicio)
		{
			InitializeComponent();
			FijarAnchoDeBotones();

			_linkHojaConsulta = linkHojaConsulta;
			_servicio = servicio;
			_servicioGoogle = new ServiciosGoogle();

			InicializarVariablesGlobales();
			ObtenerDatosProductosDesdeHCG();
			ConfigurarSelectorHojas();
		}

		//Constructor para Base de Datos
		public PaginaGrilla()
		{
			InitializeComponent();
			FijarAnchoDeBotones();
			InicializarVariablesGlobales();
			ObtenerProductosDesdeBD();
		}

		#region Métodos para Hoja de cálculo de Google

		private void InicializarVariablesGlobales()
		{
			string rutaCabecera = string.Format("StockProductorCF.Imagenes.ciudadFutura{0}.png", App.SufijoImagen);
			Cabecera.Source = ImageSource.FromResource(rutaCabecera);
			//Cabecera.WidthRequest = App.AnchoDePantalla;

			var columnasParaVer = CuentaUsuario.ObtenerColumnasParaVer();
			if (!string.IsNullOrEmpty(columnasParaVer))
				_listaColumnasParaVer = columnasParaVer.Split(',');

			var columnasInventario = CuentaUsuario.ObtenerColumnasInventario();
			if (!string.IsNullOrEmpty(columnasInventario))
				_listacolumnasInventario = columnasInventario.Split(',');
		}

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

			FijarProductosYBuscador(productos);
			LlenarGrillaDeProductos(productos);
		}

		private void ConfigurarSelectorHojas()
		{
			ListaHojas.IsVisible = true;
			var nombreHojaActual = CuentaUsuario.ObtenerNombreHoja(_linkHojaConsulta);
			var nombres = CuentaUsuario.ObtenerTodosLosNombresDeHojas();
			int i = 0;
			foreach (string nombre in nombres)
			{
				ListaHojas.Items.Add(nombre);
				if (nombre == nombreHojaActual)
					ListaHojas.SelectedIndex = i;

				i += 1;
			}
		}

		#endregion

		#region Métodos para Base de Datos

		private async void ObtenerProductosDesdeBD()
		{
			var url = string.Format(@"http://169.254.80.80/PruebaMision/Service.asmx/RecuperarProductos?token={0}", CuentaUsuario.ObtenerTokenActualDeBaseDeDatos());

			using (var cliente = new HttpClient())
			{
				//Obtiene json de productos desde el webservice
				var jsonProductos = await cliente.GetStringAsync(url);
				//Parsea el json para obtener la lista de productos
				var productos = ParsearJSONProductos(jsonProductos);

				_nombresColumnas = new[] { "Código", "Nombre", "Stock" };
				FijarProductosYBuscador(productos);
				LlenarGrillaDeProductos(productos);
			}
		}

		private List<string[]> ParsearJSONProductos(string jsonProductos)
		{
			jsonProductos = jsonProductos.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<string xmlns=\"http://tempuri.org/\">[{", "")
				.Replace("}]</string>", "")
				.Replace("},{", "|");
			var arregloProductos = jsonProductos.Split('|');
			var productos = new List<string[]>();
			string[] producto;
			string[] temporal;
			string stock;

			foreach (string datos in arregloProductos)
			{
				temporal = datos.Split(',');

				//Si el precio es diferente de 0.0 lo agregamos
				if (temporal[1].Split(':')[1].TrimStart('"').TrimEnd('"') != "0.0")
				{
					producto = new string[3];
					producto[0] = temporal[0].Split(':')[1].TrimStart('"').TrimEnd('"');
					producto[1] = temporal[2].Split(':')[1].TrimStart('"').TrimEnd('"');
					stock = temporal[18].Split(':')[1].TrimStart('"').TrimEnd('"');
					producto[2] = stock == "null" ? "0" : stock;

					productos.Add(producto);
				}
			}

			return productos;
		}

		#endregion

		#region Métodos comunes

		private void FijarAnchoDeBotones()
		{
			//Ancho de botones
			Datos.WidthRequest = App.AnchoDePantalla / 3;
			Refrescar.WidthRequest = App.AnchoDePantalla / 3;
			Escanear.WidthRequest = App.AnchoDePantalla / 3;
		}

		private void LlenarGrillaDeProductos(List<string[]> productos)
		{
			//Se quita lay grilla para recargarla.
			ContenedorTabla.Children.Clear();
			//Se carga la grilla de productos y se muestra en pantalla.
			var lista = ConstruirVistaDeLista(productos);
			ContenedorTabla.Children.Add(lista);
		}

		private void IrAlProducto(string codigoProductoSeleccionado)
		{
			if (CuentaUsuario.ObtenerAccesoDatos() == "G")
			{
				int fila = -1;
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
				foreach (string[] producto in _productos)
				{
					if (producto[0] == codigoProductoSeleccionado)
					{
						Navigation.PushAsync(new Producto(producto, _nombresColumnas));
						break;
					}
				}

			}
		}

		private StackLayout ConstruirVistaDeLista(List<string[]> productos)
		{
			List<ClaseProducto> listaProductos = new List<ClaseProducto>();
			ClaseProducto producto;
			List<string> datosParaVer;
			foreach (string[] datosProducto in productos)
			{
				datosParaVer = new List<string>();
				var i = 0;
				string textoDato;
				foreach (string dato in datosProducto)
				{
					textoDato = "";

					if (_listaColumnasParaVer != null && _listaColumnasParaVer[i] == "1")
					{
						if (_listacolumnasInventario[i] == "1")
							textoDato += _nombresColumnas[i] + " : ";

						textoDato += dato;
						datosParaVer.Add(textoDato);
					}
					i = i + 1;
				}

				producto = new ClaseProducto(datosProducto[0], datosParaVer);
				listaProductos.Add(producto);
			}

			StackLayout Encabezado = new StackLayout
			{
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Start,
				BackgroundColor = Color.FromHex("#C0C0C0"),
				Children =
								{
									new Label
									{
										Text = "Productos",
										FontSize = 18,
										HorizontalOptions = LayoutOptions.Center,
										FontAttributes = FontAttributes.Bold,
										TextColor = Color.Black,
										FontFamily = Device.OnPlatform(null,
																									 "fonts/WINGDING.TTF#Wingdings", // Android
																									 null
																									)
									}
								}
			};

			ListView vista = new ListView
			{
				RowHeight = 60,
				SeparatorColor = Color.Black,
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.Fill,
				ItemsSource = listaProductos,
				ItemTemplate = new DataTemplate(() =>
				{
					Label nombreProducto = new Label
					{
						FontSize = 16,
						FontAttributes = FontAttributes.Bold,
						VerticalOptions = LayoutOptions.CenterAndExpand,
						WidthRequest = 110
					};
					nombreProducto.SetBinding(Label.TextProperty, "Nombre");

					Label datos = new Label()
					{
						FontSize = 15,
						TextColor = Color.Black,
						VerticalOptions = LayoutOptions.CenterAndExpand
					};
					datos.SetBinding(Label.TextProperty, "Datos");

					BoxView cuadradito = new BoxView()
					{
						WidthRequest = 5,
						BackgroundColor = Color.Red
					};

					BoxView separador = new BoxView()
					{
						WidthRequest = 1,
						BackgroundColor = Color.FromHex("#E0E0E0"),
						HeightRequest = 55
					};

					// Return an assembled ViewCell.
					var celda = new ViewCell
					{
						View = new StackLayout
						{
							Padding = 2,
							Orientation = StackOrientation.Horizontal,
							BackgroundColor = Color.White,
							Children =
										{
											cuadradito,
											nombreProducto,
											separador,
											new StackLayout
											{
												Orientation = StackOrientation.Vertical,
												Spacing = 0,
												Children =
												{
													datos
												}
											}
										}
						}
					};

					celda.SetBinding(ViewCell.ClassIdProperty, "ID");
					celda.Tapped += (sender, args) =>
					{
						if (_ultimoItemSeleccionado != null)
							_ultimoItemSeleccionado.View.BackgroundColor = Color.White;
						IrAlProducto(((ViewCell)sender).ClassId);
						celda.View.BackgroundColor = Color.Silver;
						_ultimoItemSeleccionado = ((ViewCell)sender);
					};

					return celda;
				})
			};

			// Build the page.
			return new StackLayout
			{
				Spacing = 0,
				Padding = 0,
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.Fill,
				Children =
								{
									Encabezado,
									vista
								}
			};
		}

		private void FijarProductosYBuscador(List<string[]> productos)
		{
			//Si hay más de 50 productos se muestra el buscador
			if (productos.Count > 50)
			{
				//Almacena la lista de productos en la variable global que usará el buscador
				_productos = productos;

				Buscador.IsVisible = true;
			}
		}

		#endregion

		#region Eventos

		[Android.Runtime.Preserve]
		void AccederDatos(object sender, EventArgs args)
		{
			var paginaAccesoDatos = new AccesoDatos();

			Navigation.PushAsync(paginaAccesoDatos);
		}

		[Android.Runtime.Preserve]
		void RefrescarDatos(object sender, EventArgs args)
		{
			//Recarga la grilla.
			if (!string.IsNullOrEmpty(_linkHojaConsulta))
				ObtenerDatosProductosDesdeHCG(); //Hoja de cálculo de Google
			else
				ObtenerProductosDesdeBD(); //Base de Datos
		}

		[Android.Runtime.Preserve]
		async void AbrirPaginaEscaner(object sender, EventArgs args)
		{
			var paginaEscaner = new ZXingScannerPage();

			paginaEscaner.OnScanResult += (result) =>
			{
				// Detiene el escaner
				paginaEscaner.IsScanning = false;

				//Hace autofoco, particularmente para los códigos de barra
				TimeSpan ts = new TimeSpan(0, 0, 0, 3, 0);
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
			await Navigation.PushModalAsync(paginaEscaner);
		}

		//Al seleccionar un producto (dando click con el dedo en el tile)
		[Android.Runtime.Preserve]
		void CargarProducto(object sender, EventArgs args)
		{
			if (!_seleccionoItem)
			{
				var Id = ((ClaseProducto)((ListView)sender).SelectedItem).ID;
				IrAlProducto(Id);
				((ListView)sender).SelectedItem = null;
				_seleccionoItem = true;
			}
			else
				_seleccionoItem = false;
		}

		[Android.Runtime.Preserve]
		void FiltrarProductos(object sender, EventArgs args)
		{
			if (Buscador.Text.Length > 2 || Buscador.Text.Length == 0)
			{
				var productos = new List<string[]>();
				foreach (string[] producto in _productos)
				{
					if (producto[1].ToLower().Contains(Buscador.Text.ToLower()))
						productos.Add(producto);
				}

				LlenarGrillaDeProductos(productos);
			}
		}

		[Android.Runtime.Preserve]
		void CargarHoja(object sender, EventArgs args)
		{
			_linkHojaConsulta = CuentaUsuario.ObtenerLinkHojaSeleccionada(ListaHojas.Items[ListaHojas.SelectedIndex]);
			_listaColumnasParaVer = CuentaUsuario.ObtenerColumnasParaVer().Split(',');
			_listacolumnasInventario = CuentaUsuario.ObtenerColumnasInventario().Split(',');
			ObtenerDatosProductosDesdeHCG();
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
			ID = id;
			Nombre = datos[0];
			Datos = string.Join(" - ", datos.Skip(1).Take(datos.Count));
		}

		[Android.Runtime.Preserve]
		public string ID { private set; get; }
		[Android.Runtime.Preserve]
		public string Nombre { private set; get; }
		[Android.Runtime.Preserve]
		public string Datos { private set; get; }
	};

}
