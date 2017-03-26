using System;
using System.Linq;
using Xamarin.Auth;
using Xamarin.Forms;

namespace StockProductorCF.Vistas
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            Cabecera.Source = ImageSource.FromResource("StockProductorCF.Imagenes.ciudadFutura.png");

            var account = AccountStore.Create().FindAccountsForService("Inventario - Productor de la Ciudad Futura").FirstOrDefault();
            var tokenAcceso = (account != null) ? account.Properties["Token"] : null;
        }

        void AbrirPaginaGrilla(object sender, EventArgs args)
        {
            var paginaGrilla = new PaginaGrilla(string.Empty);
            
            Navigation.PushAsync(paginaGrilla);
        }

        void AccederDatos(object sender, EventArgs args)
        {
            var paginaAccesoDatos = new AccesoDatos();

            Navigation.PushAsync(paginaAccesoDatos);
        }
        
    }
}
