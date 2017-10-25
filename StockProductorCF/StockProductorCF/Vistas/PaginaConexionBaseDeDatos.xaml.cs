
using StockProductorCF.Clases;
using System;
using Xamarin.Forms;

namespace StockProductorCF.Vistas
{
	public partial class PaginaConexionBaseDeDatos
	{
		private double _anchoActual;
		private Image _listo;

		public PaginaConexionBaseDeDatos()
		{
			InitializeComponent();
			Cabecera.Children.Add(App.Instancia.ObtenerImagen(TipoImagen.EncabezadoProyectos));
			SombraEncabezado.Source = ImageSource.FromResource(App.RutaImagenSombraEncabezado);
			ConfigurarBotones();
			CuentaUsuario.AlmacenarAccesoDatos("B");
			Usuario.Text = CuentaUsuario.ObtenerUsuarioDeBaseDeDatos();
		}

		private void ConfigurarBotones()
		{
			_listo = App.Instancia.ObtenerImagen(TipoImagen.BotonListo);
			_listo.GestureRecognizers.Add(new TapGestureRecognizer(Conectar));

			ContenedorBotones.Children.Add(_listo);
		}

		[Android.Runtime.Preserve]
		private void Conectar(View arg1, object arg2)
		{
			_listo.Opacity = 0.5f;
			Device.StartTimer(TimeSpan.FromMilliseconds(300), () =>
			{
				if (Usuario.Text.ToUpper() == "HUGO" && Contrasena.Text == "Chavez")
				{
					CuentaUsuario.AlmacenarUsuarioDeBaseDeDatos(Usuario.Text.ToUpper());
					CuentaUsuario.AlmacenarColumnasParaVer("0,1,1,1,1");
					CuentaUsuario.AlmacenarColumnasInventario("0,0,0,0,1");
					CuentaUsuario.RemoverValorEnCuentaLocal("puntosVenta");
					CuentaUsuario.RemoverValorEnCuentaLocal("relacionesInsumoProducto");

					App.Instancia.LimpiarNavegadorLuegoIrPagina(new PaginaGrilla());
				}

				_listo.Opacity = 1f;
				return false;
			});
		}

		protected override async void OnSizeAllocated(double ancho, double alto)
		{
			base.OnSizeAllocated(ancho, alto);
			if (_anchoActual == ancho) return;
			if (ancho > alto)
			{
				if (_anchoActual != 0)
					await GrupoEncabezado.TranslateTo(0, -100, 1000);
				GrupoEncabezado.IsVisible = false;
			}
			else
			{
				GrupoEncabezado.IsVisible = true;
				if (_anchoActual != 0)
					await GrupoEncabezado.TranslateTo(0, 0, 1000);
			}
			_anchoActual = ancho;
		}
	}
}
