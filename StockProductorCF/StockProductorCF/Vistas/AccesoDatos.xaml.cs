using System;

namespace StockProductorCF.Vistas
{
	public partial class AccesoDatos
	{
		public AccesoDatos()
		{
			InitializeComponent();
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

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
			App.OrientacionApaisada = width > height;
			Cabecera.Source = App.ObtenerImagenEncabezadoProyectos();
		}
	}
}
