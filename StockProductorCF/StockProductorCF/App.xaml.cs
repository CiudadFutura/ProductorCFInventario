
using StockProductorCF.Clases;
using Xamarin.Forms;

namespace StockProductorCF
{
    public partial class App : Application
    {
        public static int AnchoDePantalla;
        public App()
        {
            InitializeComponent();
           
            var linkHojaConsulta = CuentaUsuario.ObtenerLinkHojaConsulta();
            var columnasParaVer = CuentaUsuario.ObtenerColumnasParaVer();
            var columnasInventario = CuentaUsuario.ObtenerColumnasInventario();

            if (!string.IsNullOrEmpty(linkHojaConsulta) && !string.IsNullOrEmpty(columnasParaVer) && !string.IsNullOrEmpty(columnasInventario))
            {
                if (!CuentaUsuario.ValidarToken())
                    MainPage = new NavigationPage(new PaginaAuntenticacion(true));
                else
                    MainPage = new NavigationPage(new Vistas.PaginaGrilla(linkHojaConsulta, null));
            }
            else
                MainPage = new NavigationPage(new Vistas.AccesoDatos());
        }

        [Android.Runtime.Preserve]
        public static void AlmacenarAnchoPantalla(int ancho)
        {
            AnchoDePantalla = ancho;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }
        
        protected override void OnResume()
        {
            // Handle when your app resumes
        }
        
    }
}
