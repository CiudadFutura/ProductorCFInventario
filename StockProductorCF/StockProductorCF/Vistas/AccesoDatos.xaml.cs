using System;
using Xamarin.Forms;

namespace StockProductorCF.Vistas
{
	public partial class AccesoDatos : ContentPage
	{
		public AccesoDatos ()
		{
			InitializeComponent ();
            Cabecera.Source = ImageSource.FromResource("StockProductorCF.Imagenes.ciudadFutura.png");
        }

        void ConectarGoogle(object sender, EventArgs args)
        {
            var paginaAuntenticacion = new PaginaAuntenticacion();

            Navigation.PushAsync(paginaAuntenticacion);
        }
    }
}
