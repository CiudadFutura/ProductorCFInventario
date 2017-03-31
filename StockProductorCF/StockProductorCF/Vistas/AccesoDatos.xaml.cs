using System;
using Xamarin.Forms;

namespace StockProductorCF.Vistas
{
	public partial class AccesoDatos : ContentPage
	{
		public AccesoDatos ()
		{
			InitializeComponent ();
            //LogoEmprendimiento.Source = ImageSource.FromResource("StockProductorCF.Imagenes.logoEmprendimiento.png");
            Cabecera.Source = ImageSource.FromResource("StockProductorCF.Imagenes.ciudadFutura.png");

            NavigationPage.SetHasNavigationBar(this, false);
        }

        [Android.Runtime.Preserve]
        void ConectarGoogle(object sender, EventArgs args)
        {
            var paginaAuntenticacion = new PaginaAuntenticacion();

            Navigation.PushAsync(paginaAuntenticacion);
        }
    }
}
