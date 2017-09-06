
using System;
using StockProductorCF.Clases;
using Xamarin.Forms;
using StockProductorCF.Vistas;

namespace StockProductorCF
{
	public partial class App
	{
		public static double AnchoRetratoDePantalla;
		public static double AnchoApaisadoDePantalla;
		public static bool EstaApaisado;
		public const string RutaImagenSombraEncabezado = "StockProductorCF.Imagenes.sombraEncabezado.png";
		public static App Instancia;

		public App()
		{
			InitializeComponent();
			Instancia = this;

			var columnasParaVer = CuentaUsuario.ObtenerColumnasParaVer();
			var columnasInventario = CuentaUsuario.ObtenerColumnasInventario();

			switch (CuentaUsuario.ObtenerAccesoDatos())
			{
				case "G":
					AccesoHojaDeCalculoGoogle(columnasParaVer, columnasInventario);
					break;
				case "B":
					AccesoBaseDeDatos(columnasParaVer, columnasInventario);
					break;
				default:
					MainPage = new NavigationPage(new AccesoDatos());
					break;
			}
		}

		private void AccesoHojaDeCalculoGoogle(string columnasParaVer, string columnasInventario)
		{
			var linkHojaConsulta = CuentaUsuario.ObtenerLinkHojaConsulta();

			if (string.IsNullOrEmpty(linkHojaConsulta) || string.IsNullOrEmpty(columnasParaVer) ||
					string.IsNullOrEmpty(columnasInventario) ||
					!CuentaUsuario.ValidarTokenDeGoogle())
				MainPage = new NavigationPage(new PaginaAuntenticacion(!string.IsNullOrEmpty(linkHojaConsulta) &&
																															 !string.IsNullOrEmpty(columnasParaVer) &&
																															 !string.IsNullOrEmpty(columnasInventario)));
			else
				MainPage = new NavigationPage(new PaginaGrilla(linkHojaConsulta, null));
		}

		private void AccesoBaseDeDatos(string columnasParaVer, string columnasInventario)
		{
			if (string.IsNullOrEmpty(columnasParaVer) || string.IsNullOrEmpty(columnasInventario))
				MainPage = new NavigationPage(new PaginaConexionBaseDeDatos());
			else
				MainPage = new NavigationPage(new PaginaGrilla());
		}

		[Android.Runtime.Preserve]
		public static void AlmacenarAnchoPantalla(double densidad, int anchoEnPixel, int altoEnPixel)
		{
			EstaApaisado = anchoEnPixel > altoEnPixel;
			AnchoRetratoDePantalla = EstaApaisado ? altoEnPixel / densidad : anchoEnPixel / densidad;
			AnchoApaisadoDePantalla = EstaApaisado ? anchoEnPixel / densidad : altoEnPixel / densidad;
		}

		public Image ObtenerImagen(TipoImagen tipoImagen, bool apaisada = false)
		{
			LayoutOptions alineacionHorizontal;
			ImageSource fuenteArchivo;
			var altoImagen = AnchoRetratoDePantalla * .20555; //Por defecto, el valor de los botones

			switch (tipoImagen)
			{
				case TipoImagen.EncabezadoProductores:
					alineacionHorizontal = LayoutOptions.Start;
					fuenteArchivo = ImageSource.FromResource("StockProductorCF.Imagenes.encabezadoProductores.png");
					altoImagen = AnchoRetratoDePantalla * .156; //ratio 169 / 1083 - ancho pantalla * 169 / 1083
					break;
				case TipoImagen.EncabezadoProyectos:
					alineacionHorizontal = LayoutOptions.Center;
					fuenteArchivo = ImageSource.FromResource("StockProductorCF.Imagenes.encabezadoProyectos.png");
					altoImagen = AnchoRetratoDePantalla * .251; //ratio 271 / 1080 - ancho pantalla * 271 / 1080
					break;
				case TipoImagen.BotonAccesoDatos:
					alineacionHorizontal = LayoutOptions.EndAndExpand;
					fuenteArchivo = ImageSource.FromResource("StockProductorCF.Imagenes.accesoDatos.png");
					break;
				case TipoImagen.BotonRefrescarDatos:
					alineacionHorizontal = LayoutOptions.Center;
					fuenteArchivo = ImageSource.FromResource("StockProductorCF.Imagenes.refrescarDatos.png");
					break;
				case TipoImagen.BotonEscanearCodigo:
					alineacionHorizontal = LayoutOptions.StartAndExpand;
					fuenteArchivo = ImageSource.FromResource("StockProductorCF.Imagenes.escanearCodigo.png");
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(tipoImagen), tipoImagen, null);
			}

			return new Image
			{
				HorizontalOptions = alineacionHorizontal,
				Source = fuenteArchivo,
				HeightRequest = altoImagen
			};
		}

		public void LimpiarNavegadorLuegoIrPagina(ContentPage pagina)
		{
			MainPage = new NavigationPage(pagina);
		}

	}

	public enum TipoImagen
	{
		EncabezadoProductores,
		EncabezadoProyectos,
		BotonAccesoDatos,
		BotonRefrescarDatos,
		BotonEscanearCodigo,
		SombraEncabezado
	}
}
