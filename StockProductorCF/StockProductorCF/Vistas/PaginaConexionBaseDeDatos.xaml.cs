
using StockProductorCF.Clases;
using System;
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
		private async void Conectar(object sender, EventArgs args)
		{
			if (Usuario.Text.ToUpper() == "HUGO" && Contrasena.Text == "Chavez")
			{
				//var url = string.Format(@"http://169.254.80.80/PruebaMision/Service.asmx/AutenticarEnMision?usuario={0}&contrasena={1}", usuario, contrasena);

				//using (var cliente = new HttpClient())
				//{
				//	var token = await cliente.GetStringAsync(url);
				//	token = token.Substring(token.IndexOf(".org/\">") + 8);
				//	token = token.Remove(token.IndexOf("\""));
				const string token = "token1234";
				if (string.IsNullOrEmpty(token)) return;

				CuentaUsuario.AlmacenarTokenDeBaseDeDatos(token);
				CuentaUsuario.AlmacenarUsuarioDeBaseDeDatos(Usuario.Text.ToUpper());
				CuentaUsuario.AlmacenarColumnasParaVer("0,1,1");
				CuentaUsuario.AlmacenarColumnasInventario("0,0,1");

				var paginaGrilla = new PaginaGrilla();
				await Navigation.PushAsync(paginaGrilla);
				//}
			}
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
