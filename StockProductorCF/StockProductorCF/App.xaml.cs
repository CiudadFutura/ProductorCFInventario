using StockProductorCF.Clases;
using Xamarin.Forms;

namespace StockProductorCF
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            
            var linkHojaConsulta = CuentaUsuario.ObtenerLinkHojaConsulta();

            if (!string.IsNullOrEmpty(linkHojaConsulta))
            {
                if (!CuentaUsuario.ValidarToken())
                    MainPage = new NavigationPage(new PaginaAuntenticacion(true));
                else
                    MainPage = new NavigationPage(new Vistas.PaginaGrilla(linkHojaConsulta, null));
            }
            else
                MainPage = new NavigationPage(new Vistas.AccesoDatos());
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
