
using System;
using System.Linq;
using Xamarin.Auth;

namespace StockProductorCF.Clases
{
    static public class CuentaUsuario
    {
        private static Account _cuenta;

        private static void RecuperarCuentaLocal()
        {
            _cuenta = AccountStore.Create().FindAccountsForService("Inventario - Productor de la Ciudad Futura").FirstOrDefault();
        }

        public static void AlmacenarTokenFechaExpiracion(string tokenDeAcceso, DateTime? fechaExpiracion)
        {
            RecuperarCuentaLocal();
            if (_cuenta != null)
            {
                _cuenta.Properties.Remove("Token");
                _cuenta.Properties.Remove("FechaExpiracion");
                _cuenta.Properties.Add("Token", tokenDeAcceso);
                _cuenta.Properties.Add("FechaExpiracion", fechaExpiracion.ToString());
            }
            else
            {
                _cuenta = new Account { Username = "local" };

                _cuenta.Properties.Add("Token", tokenDeAcceso);
                _cuenta.Properties.Add("FechaExpiracion", fechaExpiracion.ToString());
            }

            AccountStore.Create().Save(_cuenta, "Inventario - Productor de la Ciudad Futura");
        }

        public static void AlmacenarLinkHojaConsulta(string linkHojaConsulta)
        {
            RecuperarCuentaLocal();
            if (_cuenta != null)
            {
                _cuenta.Properties.Remove("LinkHojaConsulta");
                _cuenta.Properties.Add("LinkHojaConsulta", linkHojaConsulta);
            }
            else
            {
                _cuenta = new Account { Username = "local" };

                _cuenta.Properties.Add("LinkHojaConsulta", linkHojaConsulta);
            }

            AccountStore.Create().Save(_cuenta, "Inventario - Productor de la Ciudad Futura");
        }

        public static string ObtenerTokenActual()
        {
            RecuperarCuentaLocal();
            return (_cuenta != null && _cuenta.Properties.ContainsKey("Token")) ? _cuenta.Properties["Token"] : null;
        }

        public static DateTime? ObtenerFechaExpiracion()
        {
            RecuperarCuentaLocal();
            return (_cuenta != null && _cuenta.Properties.ContainsKey("FechaExpiracion") && !string.IsNullOrEmpty(_cuenta.Properties["FechaExpiracion"])) ?
                Convert.ToDateTime(_cuenta.Properties["FechaExpiracion"])
                : (DateTime?)null;
        }

        public static string ObtenerLinkHojaConsulta()
        {
            RecuperarCuentaLocal();
            return (_cuenta != null && _cuenta.Properties.ContainsKey("LinkHojaConsulta")) ? _cuenta.Properties["LinkHojaConsulta"] : null;
        }

        public static bool ValidarToken()
        {
            if (string.IsNullOrEmpty(ObtenerTokenActual()) || ObtenerFechaExpiracion() <= DateTime.Now)
                return false;

            return true;
        }
    }
}
