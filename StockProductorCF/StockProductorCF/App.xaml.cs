
using StockProductorCF.Clases;
using Xamarin.Forms;
using StockProductorCF.Vistas;

namespace StockProductorCF
{
	public partial class App
	{
		public static int AnchoDePantalla;
		public static int Ancho;
		public static int Alto;
		public static string Sufijo;
		public static ImageSource ImagenCabeceraCiudadFutura;
		public static ImageSource ImagenCabeceraCiudadFuturaApaisada;
		public static ImageSource ImagenCabeceraProyectos;
		public static ImageSource ImagenCabeceraProyectosApaisada;
		public static bool OrientacionApaisada;

		public App()
		{
			InitializeComponent();

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
		public static void AlmacenarAnchoPantalla(int anchoSobreDensidad, int anchoCrudo, int altoCrudo)
		{
			AnchoDePantalla = anchoSobreDensidad;
			Ancho = anchoCrudo;
			Alto = altoCrudo;
			OrientacionApaisada = Ancho > Alto;

			Sufijo = "240";
			if (!OrientacionApaisada && anchoCrudo > 240 || OrientacionApaisada && anchoCrudo > 240)
				Sufijo = "380";
			if (!OrientacionApaisada && anchoCrudo > 1000 || OrientacionApaisada && anchoCrudo > 1780)
				Sufijo = "1080";
			if (!OrientacionApaisada && anchoCrudo > 1180 || OrientacionApaisada && anchoCrudo > 1810)
				Sufijo = "1200";

			ImagenCabeceraCiudadFutura = ImageSource.FromResource($"StockProductorCF.Imagenes.ciudadFutura{Sufijo}.png");
			ImagenCabeceraCiudadFuturaApaisada = ImageSource.FromResource($"StockProductorCF.Imagenes.ciudadFuturaApaisada{Sufijo}.png");
			ImagenCabeceraProyectos = ImageSource.FromResource($"StockProductorCF.Imagenes.encabezadoProyectos{Sufijo}.png");
			ImagenCabeceraProyectosApaisada = ImageSource.FromResource($"StockProductorCF.Imagenes.encabezadoProyectosApaisada{Sufijo}.png");
		}

		[Android.Runtime.Preserve]
		public static ImageSource ObtenerImagenEncabezadoCiudadFutura()
		{
			return OrientacionApaisada ? ImagenCabeceraCiudadFuturaApaisada : ImagenCabeceraCiudadFutura;
		}

		[Android.Runtime.Preserve]
		public static ImageSource ObtenerImagenEncabezadoProyectos()
		{
			return OrientacionApaisada ? ImagenCabeceraProyectosApaisada : ImagenCabeceraProyectos;
		}

	}
}
