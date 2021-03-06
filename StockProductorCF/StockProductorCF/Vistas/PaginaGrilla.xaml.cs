﻿
using Google.GData.Spreadsheets;
using StockProductorCF.Clases;
using StockProductorCF.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;
using DataTemplate = Xamarin.Forms.DataTemplate;
using TextAlignment = Xamarin.Forms.TextAlignment;

namespace StockProductorCF.Vistas
{
	public partial class PaginaGrilla
	{
		private readonly ServiciosGoogle _servicioGoogle;
		private readonly SpreadsheetsService _servicio;
		private CellFeed _celdas;
		private string _linkHojaConsulta;
		private string[] _nombresColumnas;
		private string[] _listaColumnasParaVer;
		private string[] _listaColumnasInventario;
		private ViewCell _ultimoItemSeleccionado;
		private Color _ultimoColorSeleccionado;
		private List<string[]> _productos;
		private bool _esCargaInicial = true;
		private ActivityIndicator _indicadorActividad;
		private Image _accesoDatos;
		private Image _refrescar;
		private Image _escanearCodigo;
		private Picker _listaHojas;
		private Picker _listaRubros;
		private double _anchoActual;
		private bool _esGoogle;

		//Constructor para Hoja de cálculo de Google
		public PaginaGrilla(string linkHojaConsulta, SpreadsheetsService servicio)
		{
			InitializeComponent();

			_linkHojaConsulta = linkHojaConsulta;
			_servicioGoogle = new ServiciosGoogle();
			//El servicio viene nulo cuando se llama directamente desde el lanzador (ya tiene conexión a datos configurada)
			_servicio = servicio ?? _servicioGoogle.ObtenerServicioParaConsultaGoogleSpreadsheets(CuentaUsuario.ObtenerTokenActualDeGoogle());

			InicializarValoresGenerales();
			ConfigurarSelectorHojas();

			//La carga de los productos se realiza en el OnAppearing
		}

		//Constructor para Base de Datos
		public PaginaGrilla()
		{
			InitializeComponent();
			InicializarValoresGenerales();
			ConfigurarSelectorRubros();

			ObtenerProductosDesdeBD();
		}

		#region Métodos para Hoja de cálculo de Google

		private async void ObtenerDatosProductosDesdeHCG(bool refrescarLugaresRelaciones)
		{
			try
			{
				IsBusy = true;

				await Task.Run(async () =>
				{
					if (CuentaUsuario.ValidarTokenDeGoogle())
					{
						_celdas = _servicioGoogle.ObtenerCeldasDeUnaHoja(_linkHojaConsulta, _servicio);

						//Refresca los lugares (compra o venta) y relaciones (insumos - productos) sólo cuando presionamos el botón de refrescar y durante la carga inicial
						if (refrescarLugaresRelaciones)
							RefrescarLugCompVtas_RelacionInsProd();
					}
					else
					{
						//Si se quedó la pantalla abierta un largo tiempo y se venció el token, se cierra y refresca el token
						var paginaAuntenticacion = new PaginaAuntenticacion(true);
						Navigation.InsertPageBefore(paginaAuntenticacion, this);
						await Navigation.PopAsync();
					}
				});
			}
			finally
			{
				IsBusy = false; //Remueve el Indicador de Actividad.
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
					_nombresColumnas.SetValue(celda.Value, (int)celda.Column - 1);
			}
			
			LlenarGrillaDeProductos(productos);
		}

		private void ConfigurarSelectorHojas()
		{
			_listaHojas = new Picker
			{
				IsVisible = false,
				WidthRequest = App.AnchoRetratoDePantalla * .3,
				HorizontalOptions = LayoutOptions.EndAndExpand,
				VerticalOptions = LayoutOptions.Center
			};

			_listaHojas.IsVisible = true;
			var nombreHojaActual = CuentaUsuario.ObtenerNombreHoja(_linkHojaConsulta);
			var nombres = CuentaUsuario.ObtenerTodosLosNombresDeHojas();
			var i = 0;
			foreach (var nombre in nombres)
			{
				_listaHojas.Items.Add(nombre);
				if (nombre == nombreHojaActual)
					_listaHojas.SelectedIndex = i;
				i += 1;
			}

			_listaHojas.SelectedIndexChanged += CargarHoja;
			Cabecera.Children.Add(_listaHojas);
		}

		#endregion

		#region Métodos para Base de Datos

		private async void ObtenerProductosDesdeBD()
		{
			RefrescarUIGrilla();

			const string url = "https://misionantiinflacion.com.ar/api/v1/products?token=57137b4cabbab300843e45583aae5250f7bfff3115c12d58e3ac7bd195cd2b3c";

			using (var cliente = new HttpClient())
			{
				List<string[]> productos = null;

				try
				{
					IsBusy = true;

					await Task.Run(async () =>
					{
						//Obtiene json de productos desde el webservice
						var jsonProductos = await cliente.GetStringAsync(url);
						//Parsea el json para obtener la lista de productos
						productos = ParsearJsonProductos(jsonProductos);

						_nombresColumnas = new[] { "Código", "Nombre", "Marca", "Rubro", "Stock" };
					});
				}
				finally
				{
					IsBusy = false;
				}

				if (productos != null)
				{
					LlenarGrillaDeProductos(productos);
					FiltrarProductos(null, null);
				}
			}
		}

		private List<string[]> ParsearJsonProductos(string jsonProductos)
		{
			jsonProductos = jsonProductos
				.Replace("[{", "")
				.Replace("}]", "")
				.Replace("},{\"id\"", "|\"id\"");
			var arregloProductos = jsonProductos.Split('|');
			var productos = new List<string[]>();

			foreach (var datos in arregloProductos)
			{
				var temporal = datos.Replace(",\"", "|").Split('|');

				//Si el producto no está oculto lo agregamos
				if (temporal[9].Split(':')[1].TrimStart('"').TrimEnd('"') == "true") continue;
				var producto = new string[5];
				producto[0] = temporal[0].Split(':')[1].TrimStart('"').TrimEnd('"'); // ID
				producto[1] = temporal[1].Split(':')[1].TrimStart('"').TrimEnd('"').Replace("\\\"", "\""); // Nombre
				producto[2] = temporal[3].Split(':')[1].TrimStart('"').TrimEnd('"'); // Marca
				var rubro = temporal[11].Split(':')[1].TrimStart('"').TrimEnd('"');
				switch (rubro)
				{
					case "wholesaler":
						rubro = "Almacén";
						break;
					case "fragile":
						rubro = "Frágil";
						break;
					case "freshes":
						rubro = "Frescos";
						break;
					case "cleaning":
						rubro = "Limpieza";
						break;
					case "vegetables":
						rubro = "Vegetales";
						break;
					default:
						rubro = "Almacén";
						break;
				}
				producto[3] = rubro;
				var stock = temporal[8].Split(':')[1].TrimStart('"').TrimEnd('"'); // Stock
				producto[4] = stock == "null" ? "0" : stock;

				productos.Add(producto);
			}

			return productos.OrderBy(x => x[3]).ThenBy(x => x[1]).ToList();
		}

		private void ConfigurarSelectorRubros()
		{
			_listaRubros = new Picker
			{
				WidthRequest = App.AnchoRetratoDePantalla * .3,
				HorizontalOptions = LayoutOptions.EndAndExpand,
				VerticalOptions = LayoutOptions.Center,
				IsVisible = true
			};

			string[] nombres = { "Todos", "Almacén", "Frágil", "Frescos", "Limpieza", "Vegetales" };

			foreach (var nombre in nombres)
			{
				_listaRubros.Items.Add(nombre);
			}
			_listaRubros.SelectedIndex = 0;
			_listaRubros.SelectedIndexChanged += FiltrarProductos;

			Cabecera.Children.Add(_listaRubros);
		}

		#endregion

		#region Métodos comunes

		private void InicializarValoresGenerales()
		{
			_esGoogle = CuentaUsuario.ObtenerAccesoDatos() == "G";

			Cabecera.Children.Add(App.Instancia.ObtenerImagen(TipoImagen.EncabezadoProductores));
			SombraEncabezado.Source = ImageSource.FromResource(App.RutaImagenSombraEncabezado);

			ConfigurarBotones();

			var columnasParaVer = CuentaUsuario.ObtenerColumnasParaVer();
			if (!string.IsNullOrEmpty(columnasParaVer))
				_listaColumnasParaVer = columnasParaVer.Split(',');

			var columnasInventario = CuentaUsuario.ObtenerColumnasInventario();
			if (!string.IsNullOrEmpty(columnasInventario))
				_listaColumnasInventario = columnasInventario.Split(',');

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
			_accesoDatos = App.Instancia.ObtenerImagen(TipoImagen.BotonAccesoDatos);
			_accesoDatos.GestureRecognizers.Add(new TapGestureRecognizer(AccederDatos));
			_refrescar = App.Instancia.ObtenerImagen(TipoImagen.BotonRefrescarDatos);
			_refrescar.GestureRecognizers.Add(new TapGestureRecognizer(RefrescarDatos));
			_escanearCodigo = App.Instancia.ObtenerImagen(TipoImagen.BotonEscanearCodigo);
			_escanearCodigo.GestureRecognizers.Add(new TapGestureRecognizer(AbrirPaginaEscaner));

			ContenedorBotones.Children.Add(_accesoDatos);
			ContenedorBotones.Children.Add(_refrescar);
			ContenedorBotones.Children.Add(_escanearCodigo);
		}

		private void LlenarGrillaDeProductos(List<string[]> productos, bool esBusqueda = false)
		{
			if (!esBusqueda)
				FijarProductosYBuscador(productos);
			//Se carga la grilla de productos y se muestra en pantalla.
			ConstruirVistaDeLista(productos);
		}

		private async void IrAlProducto(string codigoProductoSeleccionado)
		{
			var fila = -1;
			if (_esGoogle)
			{
				var productoSeleccionado = new CellEntry[_celdas.ColCount.Count];

				//Obtener el arreglo del producto para enviar
				foreach (CellEntry celda in _celdas.Entries)
				{
					if (celda.Column == 1 && celda.Value == codigoProductoSeleccionado)
						fila = (int)celda.Row;
					if (celda.Row == fila)
						productoSeleccionado.SetValue(celda, (int)celda.Column - 1);

					//Si encontró producto (fila > -1) y ya pasó alpróximo producto (celda.Row > fila) o es el último producto (celda.Column == _celdas.ColCount.Count)
					if (fila > -1 && (celda.Row > fila || celda.Column == _celdas.ColCount.Count))
					{
						var titulo = _nombresColumnas != null && _nombresColumnas.Length > 1 ? _nombresColumnas[1] : "PRODUCTO";
						await Navigation.PushAsync(new Producto(productoSeleccionado, _nombresColumnas, _servicio, titulo), true);
						break;
					}
				}
			}
			else
			{
				foreach (var producto in _productos)
				{
					if (producto[0] != codigoProductoSeleccionado) continue;
					fila = 0;
					await Navigation.PushAsync(new Producto(producto, _nombresColumnas, producto[1]), true);
					break;
				}
			}
			//Si fila = -1 no se ha encuentrado el código
			if (fila == -1)
				await DisplayAlert("Código", "No se ha encontrado un producto para el código escaneado.", "Listo");

		}

		private void ConstruirVistaDeLista(IReadOnlyCollection<string[]> productos)
		{
			var esTeclaPar = false;
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

				var producto = new ClaseProducto(datosProducto[0], datosParaVer, esTeclaPar, _esGoogle);
				listaProductos.Add(producto);
				esTeclaPar = !esTeclaPar;
			}

			var anchoColumnaNombreProd = _esGoogle ? 120 : 200;
			var anchoColumnaDatosProd = App.AnchoRetratoDePantalla - (anchoColumnaNombreProd + 2);
			if (!_esGoogle)
				anchoColumnaDatosProd -= 10; //Cuadro de Rubro

			var titulo = _nombresColumnas != null && _nombresColumnas.Length > 1 ? _nombresColumnas[1].ToUpper() : "PRODUCTO";
			var encabezado = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				VerticalOptions = LayoutOptions.Start,
				BackgroundColor = Color.FromHex("#C0C0C0"),
				HeightRequest = productos.Count <= 25 ? 35 : 50,
				Children =
								{
									new Label
									{
										Text = "  " + titulo,
										FontSize = 13,
										HorizontalOptions = LayoutOptions.Center,
										FontAttributes = FontAttributes.Bold,
										TextColor = Color.Black,
										VerticalTextAlignment = TextAlignment.Center,
										VerticalOptions = LayoutOptions.Center,
										WidthRequest = anchoColumnaNombreProd
									},
									new Label
									{
										Text = "       INFO",
										FontSize = 13,
										HorizontalOptions = LayoutOptions.End,
										FontAttributes = FontAttributes.Bold,
										TextColor = Color.Black,
										VerticalTextAlignment = TextAlignment.Center,
										VerticalOptions = LayoutOptions.Center,
										WidthRequest = anchoColumnaDatosProd
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
						WidthRequest = anchoColumnaNombreProd
					};
					nombreProducto.SetBinding(Label.TextProperty, "Nombre");

					var separador = new BoxView
					{
						WidthRequest = 2,
						BackgroundColor = Color.FromHex("#FFFFFF"),
						HeightRequest = 55
					};

					var datos = new Label
					{
						FontSize = 15,
						TextColor = Color.FromHex("#1D1D1B"),
						VerticalOptions = LayoutOptions.CenterAndExpand,
						WidthRequest = anchoColumnaDatosProd
					};
					datos.SetBinding(Label.TextProperty, "Datos");

					var tecla = new StackLayout
					{
						Padding = 2,
						Orientation = StackOrientation.Horizontal,
						Children = { nombreProducto, separador, datos }
					};
					tecla.SetBinding(BackgroundColorProperty, "ColorFondo");

					if (!_esGoogle)
					{
						var rubro = new BoxView
						{
							WidthRequest = 10,
							HeightRequest = 55
						};
						rubro.SetBinding(BackgroundColorProperty, "ColorRubro");

						tecla.Children.Add(rubro);
					}

					var celda = new ViewCell { View = tecla };

					celda.Tapped += (sender, args) =>
					{
						if (_ultimoItemSeleccionado != null)
							_ultimoItemSeleccionado.View.BackgroundColor = _ultimoColorSeleccionado;
						IrAlProducto(((ClaseProducto)((ViewCell)sender).BindingContext).Id);
						_ultimoColorSeleccionado = celda.View.BackgroundColor;
						celda.View.BackgroundColor = Color.Silver;
						_ultimoItemSeleccionado = (ViewCell)sender;
					};

					return celda;
				})
			};

			ContenedorTabla.Children.Clear();
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
			Buscador.Text = "";
		}

		private void RefrescarUIGrilla()
		{
			//Se quita la grilla para recargarla.
			ContenedorTabla.Children.Clear();
			Buscador.IsVisible = false;
			ContenedorTabla.Children.Add(_indicadorActividad);
		}

		private void RefrescarDatos(bool refrescarLugaresRelaciones = false)
		{
			RefrescarUIGrilla();

			if (CuentaUsuario.ObtenerAccesoDatos() == "G")
				ObtenerDatosProductosDesdeHCG(refrescarLugaresRelaciones); //Hoja de cálculo de Google
			else
				ObtenerProductosDesdeBD(); //Base de Datos
		}

		#endregion

		#region Eventos

		[Android.Runtime.Preserve]
		private void AccederDatos(View arg1, object arg2)
		{
			_accesoDatos.Opacity = 0.5f;
			Device.StartTimer(TimeSpan.FromMilliseconds(300), () =>
			{
				var paginaAccesoDatos = new AccesoDatos();
				Navigation.PushAsync(paginaAccesoDatos, true);
				_accesoDatos.Opacity = 1f;
				return false;
			});
		}

		[Android.Runtime.Preserve]
		private void RefrescarDatos(View arg1, object arg2)
		{
			_refrescar.Opacity = 0.5f;
			Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
			{
				RefrescarDatos(true);
				_refrescar.Opacity = 1f;
				return false;
			});
		}

		private async void RefrescarLugCompVtas_RelacionInsProd()
		{
			if (CuentaUsuario.ValidarTokenDeGoogle())
			{
				new LugaresCompraVenta().ObtenerActualizarLugares(_linkHojaConsulta, _servicio);
				new RelacionesInsumoProducto().ObtenerActualizarRelaciones(_linkHojaConsulta, _servicio);
			}
			else
			{
				//Si se quedó la pantalla abierta un largo tiempo y se venció el token, se cierra y refresca el token
				var paginaAuntenticacion = new PaginaAuntenticacion(true);
				Navigation.InsertPageBefore(paginaAuntenticacion, this);
				await Navigation.PopAsync();
			}
		}

		[Android.Runtime.Preserve]
		private void AbrirPaginaEscaner(View arg1, object arg2)
		{
			_escanearCodigo.Opacity = 0.5f;
			Device.StartTimer(TimeSpan.FromMilliseconds(300), () =>
			{
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

				_escanearCodigo.Opacity = 1f;
				return false;
			});

		}

		[Android.Runtime.Preserve]
		private void FiltrarProductos(object sender, EventArgs args)
		{
			var productos = new List<string[]>();

			if (_esGoogle)
			{
				if (Buscador.Text.Length > 0 && Buscador.Text.Length <= 2) return;

				foreach (var producto in _productos)
				{
					if (producto[1].ToLower().Contains(Buscador.Text.ToLower()))
						productos.Add(producto);
				}
			}
			else
			{
				foreach (var producto in _productos)
				{
					if (("Todos|" + producto[3]).Contains(_listaRubros.Items[_listaRubros.SelectedIndex]) 
						&& (!Buscador.IsVisible || producto[1].ToLower().Contains(Buscador.Text.ToLower())))
						productos.Add(producto);
				}
			}

			//Se quita la grilla para recargarla.
			LlenarGrillaDeProductos(productos, true);
		}

		[Android.Runtime.Preserve]
		private void CargarHoja(object sender, EventArgs args)
		{
			Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
			{
				_linkHojaConsulta = CuentaUsuario.CambiarHojaSeleccionada(_listaHojas.Items[_listaHojas.SelectedIndex]);
				_listaColumnasParaVer = CuentaUsuario.ObtenerColumnasParaVer().Split(',');
				_listaColumnasInventario = CuentaUsuario.ObtenerColumnasInventario().Split(',');

				RefrescarDatos(true);
				//	ObtenerDatosProductosDesdeHCG();
				return false;
			});
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

		//Cuando carga la página y cuando vuelve de registrar un movimiento.
		protected override void OnAppearing()
		{
			RefrescarDatos(_esCargaInicial);
			_esCargaInicial = false; //Si era carga inicial venía en true, si no ya estaba en false
		}

		#endregion

	}

	//Clase Producto: utilizada para armar la lista scrolleable de productos
	[Android.Runtime.Preserve]
	public class ClaseProducto
	{
		[Android.Runtime.Preserve]
		public ClaseProducto(string id, IList<string> datos, bool esTeclaPar, bool esGoogle)
		{
			Id = id;
			Nombre = datos[0];
			Datos = string.Join(" - ", datos.Skip(1).Take(datos.Count));
			ColorFondo = esTeclaPar ? Color.FromHex("#EDEDED") : Color.FromHex("#E2E2E1");
			ColorRubro = esGoogle ? Color.Red : ColorearRubro(datos[2]);
		}

		private Color ColorearRubro(string rubro)
		{
			// Si es Misión usa color por rubro
			switch (rubro)
			{
				case "Almacén":
					return Color.FromHex("#FF9933");
				case "Frágil":
					return Color.FromHex("#D21721");
				case "Frescos":
					return Color.FromHex("#7A0099");
				case "Limpieza":
					return Color.FromHex("#0033CC");
				case "Vegetales":
					return Color.FromHex("#00802B"); ;
			}

			return Color.FromHex("#E2E2E1");
		}

		[Android.Runtime.Preserve]
		public string Id { get; }
		[Android.Runtime.Preserve]
		public string Nombre { get; }
		[Android.Runtime.Preserve]
		public string Datos { get; }
		[Android.Runtime.Preserve]
		public Color ColorFondo { get; }
		[Android.Runtime.Preserve]
		public Color ColorRubro { get; }
	}

}
