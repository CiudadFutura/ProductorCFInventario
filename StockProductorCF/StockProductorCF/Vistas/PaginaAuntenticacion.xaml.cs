
using Google.GData.Spreadsheets;
using StockProductorCF.Clases;
using StockProductorCF.Servicios;
using StockProductorCF.Vistas;
using System;
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
            _conexionExistente = conexionExistente; //Si es verdadero debe llevarnos a la Grilla en lugar de avanzar hacia la página de selección de libros

            var webView = new WebView();

            if (!CuentaUsuario.ValidarToken())
            {
                var solicitud =
                        "https://accounts.google.com/o/oauth2/auth?client_id=" + _clientId
                        + "&scope=https://www.googleapis.com/auth/drive https://spreadsheets.google.com/feeds https://docs.google.com/feeds"
                        + "&token_uri=https://accounts.google.com/o/oauth2/token"
                        + "&response_type=token&redirect_uri=http://localhost";

                webView.Source = solicitud;
            }
            else
            {
                webView.Source = "http://localhost/#access_token=" + CuentaUsuario.ObtenerTokenActual() + "&token_type=&expires_in=&noActualizarFecha";
            }

            webView.HeightRequest = 1;
            webView.Navigated += CuandoNavegaWebView;
            Content = webView;
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
                    CuentaUsuario.AlmacenarFechaExpiracion(fechaExpiracion);
                }
                var tokenDeAcceso = at.Remove(at.IndexOf("&token_type="));

                CuentaUsuario.AlmacenarToken(tokenDeAcceso);
                DeterminarProcesoParaCargaDatos(tokenDeAcceso);
            }
        }
    }

}
