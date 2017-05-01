using System;
using Xamarin.Forms;

namespace StockProductorCF.Vistas
{
	public partial class AccesoDatos : ContentPage
	{
		public AccesoDatos()
		{
			InitializeComponent();
			Cabecera.Source = ImageSource.FromResource(string.Format("StockProductorCF.Imagenes.encabezadoProyectos{0}.png", App.SufijoImagen));
		}

		[Android.Runtime.Preserve]
		void ConectarGoogle(object sender, EventArgs args)
		{
			var paginaAuntenticacion = new PaginaAuntenticacion();
			Navigation.PushAsync(paginaAuntenticacion);
		}

		[Android.Runtime.Preserve]
		void ConectarBaseDatos(object sender, EventArgs args)
		{
			var paginaConexionBaseDatos = new PaginaConexionBaseDeDatos();
			Navigation.PushAsync(paginaConexionBaseDatos);
		}
	}
}
