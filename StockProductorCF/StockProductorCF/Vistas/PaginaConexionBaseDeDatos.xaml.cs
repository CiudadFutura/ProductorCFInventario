
using StockProductorCF.Clases;
using System;
using System.Net.Http;
using Xamarin.Forms;

namespace StockProductorCF.Vistas
{
	public partial class PaginaConexionBaseDeDatos
	{
		private double _anchoActual;

		public PaginaConexionBaseDeDatos()
		{
			InitializeComponent();
			Cabecera.Children.Add(App.ObtenerImagen(TipoImagen.EncabezadoProyectos));
			SombraEncabezado.Source = ImageSource.FromResource(App.RutaImagenSombraEncabezado);
			CuentaUsuario.AlmacenarAccesoDatos("B");
			Usuario.Text = CuentaUsuario.ObtenerUsuarioDeBaseDeDatos();
		}

		[Android.Runtime.Preserve]
		void Conectar(object sender, EventArgs args)
		{
			RecuperarTokenRecuperarProductos(Usuario.Text, Contrasena.Text);
		}

		private async void RecuperarTokenRecuperarProductos(string usuario, string contrasena)
		{
			string token;
			var url = string.Format(@"http://169.254.80.80/PruebaMision/Service.asmx/AutenticarEnMision?usuario={0}&contrasena={1}", usuario, contrasena);

			using (var cliente = new HttpClient())
			{
				token = await cliente.GetStringAsync(url);
				token = token.Substring(token.IndexOf(".org/\">") + 8);
				token = token.Remove(token.IndexOf("\""));
				if (!string.IsNullOrEmpty(token))
				{
					CuentaUsuario.AlmacenarTokenDeBaseDeDatos(token);
					CuentaUsuario.AlmacenarUsuarioDeBaseDeDatos(usuario);
					CuentaUsuario.AlmacenarColumnasParaVer("0,1,1");
					CuentaUsuario.AlmacenarColumnasInventario("0,0,1");
					var paginaGrilla = new PaginaGrilla();
					await Navigation.PushAsync(paginaGrilla);
				}
			}
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
