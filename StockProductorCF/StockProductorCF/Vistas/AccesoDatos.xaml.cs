using System;
using Xamarin.Forms;

namespace StockProductorCF.Vistas
{
	public partial class AccesoDatos
	{
		private double _anchoActual;

		public AccesoDatos()
		{
			InitializeComponent();
			Cabecera.Children.Add(App.ObtenerImagen(TipoImagen.EncabezadoProyectos));
			SombraEncabezado.Source = ImageSource.FromResource(App.RutaImagenSombraEncabezado);
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

		protected override void OnSizeAllocated(double ancho, double alto)
		{
			base.OnSizeAllocated(ancho, alto);
			if (_anchoActual == ancho) return;
			SombraEncabezado.WidthRequest = ancho > alto ? App.AnchoApaisadoDePantalla : App.AnchoRetratoDePantalla;
			_anchoActual = ancho;
		}
	}
}
