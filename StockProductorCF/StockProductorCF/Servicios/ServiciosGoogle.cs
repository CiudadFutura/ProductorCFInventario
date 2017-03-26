using System.Net.Http;
using System.Threading.Tasks;
using StockProductorCF.Modelos;
using Newtonsoft.Json;

namespace StockProductorCF.Servicios
{
    public class ServiciosGoogle
    {
        public async Task<PerfilGoogle> ObtenerPerfilGoogleAsinc(string tokenDeAcceso)
        {
            var requestUrl =
                "https://graph.facebook.com/v2.7/me/?fields=name,picture,work,website,religion,location,locale,link,cover,age_range,bio,birthday,devices,email,first_name,last_name,gender,hometown,is_verified,languages&access_token="
                + tokenDeAcceso;

            var httpCliente = new HttpClient();

            var jsonDeUsuario = await httpCliente.GetStringAsync(requestUrl);

            var perfilGoogle = JsonConvert.DeserializeObject<PerfilGoogle>(jsonDeUsuario);

            return perfilGoogle;
        }
    }

}