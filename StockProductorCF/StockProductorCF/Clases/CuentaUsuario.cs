
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

		public static void AlmacenarAccesoDatos(string acceso)
		{
			if(RecuperarValorDeCuentaLocal("accesoDatos") != acceso)
			{
				GuardarValorEnCuentaLocal("columnasParaVer", "");
				GuardarValorEnCuentaLocal("columnasInventario", "");
			}
			GuardarValorEnCuentaLocal("accesoDatos", acceso);
		}

		public static void AlmacenarTokenDeGoogle(string tokenDeAcceso)
		{
			GuardarValorEnCuentaLocal("tokenDeGoogle", tokenDeAcceso);
		}

		public static void AlmacenarFechaExpiracionToken(DateTime? fechaExpiracion)
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

		public static void AlmacenarTokenDeBaseDeDatos(string tokenDeAcceso)
		{
			GuardarValorEnCuentaLocal("tokenDeBaseDeDatos", tokenDeAcceso);
		}

		public static void AlmacenarUsuarioDeBaseDeDatos(string usuario)
		{
			GuardarValorEnCuentaLocal("usuarioDeBaseDeDatos", usuario);
		}

		public static string ObtenerTokenActualDeGoogle()
		{
			return RecuperarValorDeCuentaLocal("tokenDeGoogle");
		}

		public static DateTime? ObtenerFechaExpiracionToken()
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

		public static string ObtenerTokenActualDeBaseDeDatos()
		{
			return RecuperarValorDeCuentaLocal("tokenDeBaseDeDatos");
		}

		public static string ObtenerUsuarioDeBaseDeDatos()
		{
			return RecuperarValorDeCuentaLocal("usuarioDeBaseDeDatos");
		}

		public static string ObtenerAccesoDatos()
		{
			return RecuperarValorDeCuentaLocal("accesoDatos");
		}

		public static bool ValidarTokenDeGoogle()
		{
			if (string.IsNullOrEmpty(ObtenerTokenActualDeGoogle()) || ObtenerFechaExpiracionToken() <= DateTime.Now)
				return false;

			return true;
		}
	}
}
