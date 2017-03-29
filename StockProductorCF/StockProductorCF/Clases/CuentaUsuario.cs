
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
            _cuenta = AccountStore.Create().FindAccountsForService("InventarioProductorCiudadFutura").FirstOrDefault();
        }

        public static void AlmacenarToken(string tokenDeAcceso)
        {
            RecuperarCuentaLocal();
            if (_cuenta != null)
            {
                _cuenta.Properties.Remove("token");
                _cuenta.Properties.Add("token", tokenDeAcceso);
            }
            else
            {
                _cuenta = new Account { Username = "local" };

                _cuenta.Properties.Add("token", tokenDeAcceso);
            }

            AccountStore.Create().Save(_cuenta, "InventarioProductorCiudadFutura");
        }

        public static void AlmacenarFechaExpiracion(DateTime? fechaExpiracion)
        {
            RecuperarCuentaLocal();
            if (_cuenta != null)
            {
                _cuenta.Properties.Remove("fechaExpiracion");
                _cuenta.Properties.Add("fechaExpiracion", fechaExpiracion.ToString());
            }
            else
            {
                _cuenta = new Account { Username = "local" };
                
                _cuenta.Properties.Add("fechaExpiracion", fechaExpiracion.ToString());
            }

            AccountStore.Create().Save(_cuenta, "InventarioProductorCiudadFutura");
        }

        public static void AlmacenarLinkHojaConsulta(string linkHojaConsulta)
        {
            RecuperarCuentaLocal();
            if (_cuenta != null)
            {
                _cuenta.Properties.Remove("linkHojaConsulta");
                _cuenta.Properties.Add("linkHojaConsulta", linkHojaConsulta);
            }
            else
            {
                _cuenta = new Account { Username = "local" };

                _cuenta.Properties.Add("linkHojaConsulta", linkHojaConsulta);
            }

            AccountStore.Create().Save(_cuenta, "InventarioProductorCiudadFutura");
        }

        public static void AlmacenarColumnasParaVer(string columnasParaVer)
        {
            RecuperarCuentaLocal();
            if (_cuenta != null)
            {
                _cuenta.Properties.Remove("columnasParaVer");
                _cuenta.Properties.Add("columnasParaVer", columnasParaVer);
            }
            else
            {
                _cuenta = new Account { Username = "local" };

                _cuenta.Properties.Add("columnasParaVer", columnasParaVer);
            }

            AccountStore.Create().Save(_cuenta, "InventarioProductorCiudadFutura");
        }

        public static void AlmacenarColumnasInventario(string columnasInventario)
        {
            RecuperarCuentaLocal();
            if (_cuenta != null)
            {
                _cuenta.Properties.Remove("columnasInventario");
                _cuenta.Properties.Add("columnasInventario", columnasInventario);
            }
            else
            {
                _cuenta = new Account { Username = "local" };

                _cuenta.Properties.Add("columnasInventario", columnasInventario);
            }

            AccountStore.Create().Save(_cuenta, "InventarioProductorCiudadFutura");
        }

        public static string ObtenerTokenActual()
        {
            RecuperarCuentaLocal();
            return (_cuenta != null && _cuenta.Properties.ContainsKey("token")) ? _cuenta.Properties["token"] : null;
        }

        public static DateTime? ObtenerFechaExpiracion()
        {
            RecuperarCuentaLocal();
            return (_cuenta != null && _cuenta.Properties.ContainsKey("fechaExpiracion") && !string.IsNullOrEmpty(_cuenta.Properties["fechaExpiracion"])) ?
                Convert.ToDateTime(_cuenta.Properties["fechaExpiracion"])
                : (DateTime?)null;
        }

        public static string ObtenerLinkHojaConsulta()
        {
            RecuperarCuentaLocal();
            return (_cuenta != null && _cuenta.Properties.ContainsKey("linkHojaConsulta")) ? _cuenta.Properties["linkHojaConsulta"] : null;
        }

        public static string ObtenerColumnasParaVer()
        {
            RecuperarCuentaLocal();
            return (_cuenta != null && _cuenta.Properties.ContainsKey("columnasParaVer")) ? _cuenta.Properties["columnasParaVer"] : null;
        }

        public static string ObtenerColumnasInventario()
        {
            RecuperarCuentaLocal();
            return (_cuenta != null && _cuenta.Properties.ContainsKey("columnasInventario")) ? _cuenta.Properties["columnasInventario"] : null;
        }

        public static bool ValidarToken()
        {
            if (string.IsNullOrEmpty(ObtenerTokenActual()) || ObtenerFechaExpiracion() <= DateTime.Now)
                return false;

            return true;
        }
    }
}
