
using Google.GData.Client;
using Google.GData.Spreadsheets;
using StockProductorCF.ModelosDeVista;
using StockProductorCF.Vistas;
using System.Text;
using Xamarin.Auth;
using Xamarin.Forms;

namespace StockProductorCF
{
    public partial class PaginaAuntenticacion : ContentPage
    {
        private string ClientId = "209664019803-t7i8ejmj640m136bqp3snkn8gil54s6d.apps.googleusercontent.com";

        public PaginaAuntenticacion()
        {
            InitializeComponent();
            Cabecera.Source = ImageSource.FromResource("StockProductorCF.Imagenes.ciudadFutura.png");

            var apiRequest =
                "https://accounts.google.com/o/oauth2/auth?client_id="
                + ClientId
                + "&scope=https://www.googleapis.com/auth/drive https://spreadsheets.google.com/feeds https://docs.google.com/feeds"
                + "&token_uri=https://accounts.google.com/o/oauth2/token"
                + "&response_type=token&redirect_uri=http://localhost";

            var webView = new WebView
            {
                Source = apiRequest,
                HeightRequest = 1
            };

            webView.Navigated += CuandoNavegaWebView;

            Content = webView;
        }

        private async void CuandoNavegaWebView(object sender, WebNavigatedEventArgs e)
        {

            var tokenDeAcceso = ExtraerTokenAccesoDesdeUrl(e.Url);

            if (tokenDeAcceso != "")
            {
                Account cuenta = new Account
                {
                    Username = "local"
                };
                cuenta.Properties.Add("Token", tokenDeAcceso);
                AccountStore.Create().Save(cuenta, "Inventario - Productor de la Ciudad Futura");

                SpreadsheetsService servicio = new SpreadsheetsService("Inventario - Productor de la Ciudad Futura");

                var parametros = new OAuth2Parameters();
                parametros.AccessToken = tokenDeAcceso;
                servicio.RequestFactory = new GOAuth2RequestFactory(null, "Inventario - Productor de la Ciudad Futura", parametros);

                SpreadsheetQuery consulta = new SpreadsheetQuery();
                SpreadsheetFeed feed = servicio.Query(consulta);

                StringBuilder listaHojas = new StringBuilder();
                foreach (SpreadsheetEntry entry in feed.Entries)
                {
                    listaHojas.AppendLine(entry.Title.Text);
                }

                var paginaGrilla = new PaginaGrilla(listaHojas.ToString());

                Navigation.InsertPageBefore(paginaGrilla, this);
                await Navigation.PopAsync();
                                
                //VER                

                //var vm = BindingContext as ModeloDeVistaGoogle;

                //await vm.EstablecerPerfilUsuarioGoogleAsinc(accessToken);

                //Content = MainStackLayout;
            }
        }

        private string ExtraerTokenAccesoDesdeUrl(string url)
        {
            if (url.Contains("access_token") && url.Contains("&expires_in="))
            {
                var at = url.Replace("http://localhost/#access_token=", "");

                if (Device.OS == TargetPlatform.WinPhone || Device.OS == TargetPlatform.Windows) //VER
                {
                    at = url.Replace("http://localhost/#access_token=", "");
                }

                var tokenDeAcceso = at.Remove(at.IndexOf("&token_type="));

                return tokenDeAcceso;
            }

            return string.Empty;
        }
    }
}
