
using StockProductorCF.Clases;
using Xamarin.Forms;
using StockProductorCF.Vistas;

namespace StockProductorCF
{
	public partial class App
	{
		public static int AnchoDePantalla;
		public static int Ancho;
		public static string Sufijo;
		public static ImageSource ImagenCabeceraCiudadFutura;
		public static ImageSource ImagenCabeceraProyectos;

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

			if (string.IsNullOrEmpty(linkHojaConsulta) || string.IsNullOrEmpty(columnasParaVer) || string.IsNullOrEmpty(columnasInventario) ||
				!CuentaUsuario.ValidarTokenDeGoogle())
					MainPage = new NavigationPage(new PaginaAuntenticacion(!string.IsNullOrEmpty(linkHojaConsulta) && !string.IsNullOrEmpty(columnasParaVer) && !string.IsNullOrEmpty(columnasInventario)));
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
		public static void AlmacenarAnchoPantalla(int anchoSobreDensidad, int anchoCrudo)
		{
			AnchoDePantalla = anchoSobreDensidad;
			Ancho = anchoCrudo;
			
			Sufijo = "380";
			if (anchoCrudo > 1000)
				Sufijo = "1080";

			ImagenCabeceraCiudadFutura = ImageSource.FromResource($"StockProductorCF.Imagenes.ciudadFutura{Sufijo}.png");
			ImagenCabeceraProyectos = ImageSource.FromResource($"StockProductorCF.Imagenes.encabezadoProyectos{Sufijo}.png");
		}
	}
}
