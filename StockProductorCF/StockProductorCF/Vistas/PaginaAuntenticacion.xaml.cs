
using Google.GData.Spreadsheets;
using StockProductorCF.Clases;
using StockProductorCF.Servicios;
using StockProductorCF.Vistas;
using System;
using System.Net.Http;
using Xamarin.Forms;

namespace StockProductorCF
{
	public partial class PaginaAuntenticacion : ContentPage
	{
		private string _clientId = "209664019803-t7i8ejmj640m136bqp3snkn8gil54s6d.apps.googleusercontent.com";
		private bool _conexionExistente;

		public PaginaAuntenticacion(bool conexionExistente = false)
		{
			InitializeComponent();
			Cabecera.Source = ImageSource.FromResource(string.Format("StockProductorCF.Imagenes.encabezadoProyectos{0}.png", App.SufijoImagen));
			CuentaUsuario.AlmacenarAccesoDatos("G");
			_conexionExistente = conexionExistente; //Si es verdadero debe llevarnos a la Grilla en lugar de avanzar hacia la página de selección de libros

			var webView = new WebView();

			if (!CuentaUsuario.ValidarTokenDeGoogle())
			{
				var solicitud =
								"https://accounts.google.com/o/oauth2/auth?client_id=" + _clientId
								+ "&scope=https://www.googleapis.com/auth/drive https://spreadsheets.google.com/feeds https://www.googleapis.com/auth/plus.login"
								+ "&token_uri=https://accounts.google.com/o/oauth2/token"
								+ "&response_type=token&redirect_uri=http://localhost";

				webView.Source = solicitud;
			}
			else
			{
				webView.Source = "http://localhost/#access_token=" + CuentaUsuario.ObtenerTokenActualDeGoogle() + "&token_type=&expires_in=&noActualizarFecha";
			}

			webView.HeightRequest = 1;
			webView.Navigated += CuandoNavegaWebView;
			webView.HorizontalOptions = LayoutOptions.FillAndExpand;
			webView.VerticalOptions = LayoutOptions.FillAndExpand;
			Contenedor.Children.Add(webView);
		}

		private void CuandoNavegaWebView(object sender, WebNavigatedEventArgs e)
		{
			ExtraerTokenAccesoDesdeUrl(e.Url);
		}

		private async void DeterminarProcesoParaCargaDatos(string tokenDeAcceso)
		{
			if (_conexionExistente) //Si es verdadero debe llevarnos a la Grilla en lugar de avanzar hacia la página de selección de libros
			{
				var linkHojaConsulta = CuentaUsuario.ObtenerLinkHojaConsulta();
				Navigation.InsertPageBefore(new PaginaGrilla(linkHojaConsulta, null), this);
			}
			else
			{
				var servicioGoogle = new ServiciosGoogle();
				SpreadsheetsService servicio = servicioGoogle.ObtenerServicioParaConsultaGoogleSpreadsheets(tokenDeAcceso);

				SpreadsheetFeed libros = servicioGoogle.ObtenerListaLibros(servicio);

				var paginaListaLibros = new ListaLibrosGoogle(servicio, libros.Entries);

				Navigation.InsertPageBefore(paginaListaLibros, this);
			}

			await Navigation.PopAsync();
		}

		private void ExtraerTokenAccesoDesdeUrl(string url)
		{
			if (url.Contains("access_token") && url.Contains("&expires_in="))
			{
				Content = null;

				var at = url.Replace("http://localhost/#access_token=", "");

				if (Device.OS == TargetPlatform.WinPhone || Device.OS == TargetPlatform.Windows) //VER
				{
					at = url.Replace("http://localhost/#access_token=", "");
				}

				if (!url.Contains("&noActualizarFecha"))
				{
					//Expira en 1 hora, por las dudas, lo actualizamos a los 55 minutos para evitar potencial desfasaje en el horario del servidor.
					var fechaExpiracion = DateTime.Now.AddMinutes(55);
					CuentaUsuario.AlmacenarFechaExpiracionToken(fechaExpiracion);
				}
				var tokenDeAcceso = at.Remove(at.IndexOf("&token_type="));

				CuentaUsuario.AlmacenarTokenDeGoogle(tokenDeAcceso);

				//Recuperar el nombre de usuario para el historial de movimientos
				if(string.IsNullOrEmpty(CuentaUsuario.ObtenerNombreUsuarioGoogle()))
					RecuperarNombreUsuarioGoogle(tokenDeAcceso);
				//A partir de la procedencia determinar si irá hacia la página de grilla o hacia la de libros
				DeterminarProcesoParaCargaDatos(tokenDeAcceso);
			}
		}

		private async void RecuperarNombreUsuarioGoogle(string tokenDeAcceso)
		{
			string url = @"https://www.googleapis.com/oauth2/v1/userinfo?access_token=" + tokenDeAcceso;

			using (var cliente = new HttpClient())
			{
				var usuario = await cliente.GetStringAsync(url);
				usuario = usuario.Substring(usuario.IndexOf("\"name\": \"") + 9);
				usuario = usuario.Remove(usuario.IndexOf("\",\n"));
				CuentaUsuario.AlmacenarNombreUsuarioGoogle(usuario);
			}
		}
	}

}
