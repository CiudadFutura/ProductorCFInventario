using System;
using Xamarin.Forms;

namespace StockProductorCF.Vistas
{
	public partial class AccesoDatos : ContentPage
	{
		public AccesoDatos()
		{
			InitializeComponent();
			Cabecera.Source = ImageSource.FromResource($"StockProductorCF.Imagenes.encabezadoProyectos{App.SufijoImagen}.png");
		}

		[Android.Runtime.Preserve]
		private void ConectarGoogle(object sender, EventArgs args)
		{
			var paginaAuntenticacion = new PaginaAuntenticacion();
			Navigation.PushAsync(paginaAuntenticacion);
		}

		[Android.Runtime.Preserve]
		private void ConectarBaseDatos(object sender, EventArgs args)
		{
			var paginaConexionBaseDatos = new PaginaConexionBaseDeDatos();
			Navigation.PushAsync(paginaConexionBaseDatos);
		}
	}
}
