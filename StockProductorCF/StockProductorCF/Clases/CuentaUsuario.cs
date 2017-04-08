
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
			//Recupera la cuenta local
			if (_cuenta == null)
				_cuenta = AccountStore.Create().FindAccountsForService("InventarioProductorCiudadFutura").FirstOrDefault();
		}

		private static void GuardarValorEnCuentaLocal(string llave, string valor)
		{
			RecuperarCuentaLocal();

			if (_cuenta != null)
			{
				//Si existe la cuenta, agrega el valor a la cuenta
				_cuenta.Properties.Remove(llave);
				_cuenta.Properties.Add(llave, valor);
			}
			else
			{
				//Si no existe la cuenta, la crea y luego agrega el valor a la cuenta
				_cuenta = new Account { Username = "local" };
				_cuenta.Properties.Add(llave, valor);
			}

			//Almacena la cuenta local
			AccountStore.Create().Save(_cuenta, "InventarioProductorCiudadFutura");
		}

		private static string RecuperarValorDeCuentaLocal(string llave)
		{
			RecuperarCuentaLocal();
			return (_cuenta != null && _cuenta.Properties.ContainsKey(llave)) ? _cuenta.Properties[llave] : null;
		}

		public static void AlmacenarToken(string tokenDeAcceso)
		{
			GuardarValorEnCuentaLocal("token", tokenDeAcceso);
		}

		public static void AlmacenarFechaExpiracion(DateTime? fechaExpiracion)
		{
			GuardarValorEnCuentaLocal("fechaExpiracion", fechaExpiracion.ToString());
		}

		public static void AlmacenarLinkHojaConsulta(string linkHojaConsulta)
		{
			GuardarValorEnCuentaLocal("linkHojaConsulta", linkHojaConsulta);
		}

		public static void AlmacenarColumnasParaVer(string columnasParaVer)
		{
			GuardarValorEnCuentaLocal("columnasParaVer", columnasParaVer);
		}

		public static void AlmacenarColumnasInventario(string columnasInventario)
		{
			GuardarValorEnCuentaLocal("columnasInventario", columnasInventario);
		}

		public static void AlmacenarNombreUsuarioGoogle(string nombreUsuarioGoogle)
		{
			GuardarValorEnCuentaLocal("nombreUsuarioGoogle", nombreUsuarioGoogle);
		}

		public static void AlmacenarLinkHojaHistorial(string linkHojaHistorial)
		{
			GuardarValorEnCuentaLocal("linkHojaHistorial", linkHojaHistorial);
		}

		public static string ObtenerTokenActual()
		{
			return RecuperarValorDeCuentaLocal("token");
		}

		public static DateTime? ObtenerFechaExpiracion()
		{
			var valor = RecuperarValorDeCuentaLocal("fechaExpiracion");
			return !string.IsNullOrEmpty(valor) ? Convert.ToDateTime(valor) : (DateTime?)null;
		}

		public static string ObtenerLinkHojaInventario()
		{
			return RecuperarValorDeCuentaLocal("linkHojaConsulta");
		}

		public static string ObtenerColumnasParaVer()
		{
			return RecuperarValorDeCuentaLocal("columnasParaVer");
		}

		public static string ObtenerColumnasInventario()
		{
			return RecuperarValorDeCuentaLocal("columnasInventario");
		}

		public static string ObtenerNombreUsuarioGoogle()
		{
			return RecuperarValorDeCuentaLocal("nombreUsuarioGoogle");
		}

		public static string ObtenerLinkHojaHistorial()
		{
			return RecuperarValorDeCuentaLocal("linkHojaHistorial");
		}

		public static bool ValidarToken()
		{
			if (string.IsNullOrEmpty(ObtenerTokenActual()) || ObtenerFechaExpiracion() <= DateTime.Now)
				return false;

			return true;
		}
	}
}
