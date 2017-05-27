
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Auth;

namespace StockProductorCF.Clases
{
	public static class CuentaUsuario
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

		internal static void AlmacenarAccesoDatos(string acceso)
		{
			if(RecuperarValorDeCuentaLocal("accesoDatos") != acceso)
			{
				GuardarValorEnCuentaLocal("columnasParaVer", "");
				GuardarValorEnCuentaLocal("columnasInventario", "");
			}
			GuardarValorEnCuentaLocal("accesoDatos", acceso);
		}

		internal static void AlmacenarTokenDeGoogle(string tokenDeAcceso)
		{
			GuardarValorEnCuentaLocal("tokenDeGoogle", tokenDeAcceso);
		}

		internal static void AlmacenarFechaExpiracionToken(DateTime? fechaExpiracion)
		{
			GuardarValorEnCuentaLocal("fechaExpiracion", fechaExpiracion.ToString());
		}

		internal static void AlmacenarLinkHojaConsulta(string linkHojaConsulta)
		{
			GuardarValorEnCuentaLocal("linkHojaConsulta", linkHojaConsulta);
		}

		internal static bool VerificarHojaUsada(string linkHojaConsulta)
		{
			return _cuenta != null && _cuenta.Properties.ContainsKey(linkHojaConsulta + "|ver") 
				&& _cuenta.Properties.ContainsKey(linkHojaConsulta + "|inventario") && _cuenta.Properties.ContainsKey(linkHojaConsulta + "|nombre");
		}

		internal static bool VerificarHojaUsadaRecuperarColumnas(string linkHojaConsulta)
		{
			//Si no hay columnas para esta hoja deducimos que la hoja no se ha usado, si hay, las cargamos.
			if (!VerificarHojaUsada(linkHojaConsulta))
				return false;
			else
			{
				AlmacenarColumnasParaVer(RecuperarValorDeCuentaLocal(linkHojaConsulta + "|ver"));
				AlmacenarColumnasInventario(RecuperarValorDeCuentaLocal(linkHojaConsulta + "|inventario"));
				return true;
			}
		}

		internal static void AlmacenarColumnasParaVerDeHoja(string linkHojaConsulta, string columnasParaVer)
		{
			GuardarValorEnCuentaLocal("columnasParaVer", columnasParaVer);
			GuardarValorEnCuentaLocal(linkHojaConsulta + "|ver", columnasParaVer);
		}

		internal static void AlmacenarColumnasParaVer(string columnasParaVer)
		{
			GuardarValorEnCuentaLocal("columnasParaVer", columnasParaVer);
		}

		internal static void AlmacenarColumnasInventarioDeHoja(string linkHojaConsulta, string columnasInventario)
		{
			GuardarValorEnCuentaLocal("columnasInventario", columnasInventario);
			GuardarValorEnCuentaLocal(linkHojaConsulta + "|inventario", columnasInventario);
		}

		internal static void AlmacenarNombreDeHoja(string linkHojaConsulta, string nombreHojaConsulta)
		{
			GuardarValorEnCuentaLocal(linkHojaConsulta + "|nombre", nombreHojaConsulta);
		}

		internal static void AlmacenarNombreDeHojaHistorica(string linkHojaConsulta, string nombreHojaConsulta)
		{
			GuardarValorEnCuentaLocal(linkHojaConsulta + "|historico", nombreHojaConsulta);
		}

		internal static void AlmacenarColumnasInventario(string columnasInventario)
		{
			GuardarValorEnCuentaLocal("columnasInventario", columnasInventario);
		}

		internal static void AlmacenarNombreUsuarioGoogle(string nombreUsuarioGoogle)
		{
			GuardarValorEnCuentaLocal("nombreUsuarioGoogle", nombreUsuarioGoogle);
		}

		internal static void AlmacenarLinkHojaHistorial(string linkHojaHistorial)
		{
			GuardarValorEnCuentaLocal("linkHojaHistorial", linkHojaHistorial);
		}

		internal static void AlmacenarTokenDeBaseDeDatos(string tokenDeAcceso)
		{
			GuardarValorEnCuentaLocal("tokenDeBaseDeDatos", tokenDeAcceso);
		}

		internal static void AlmacenarUsuarioDeBaseDeDatos(string usuario)
		{
			GuardarValorEnCuentaLocal("usuarioDeBaseDeDatos", usuario);
		}

		internal static string ObtenerTokenActualDeGoogle()
		{
			return RecuperarValorDeCuentaLocal("tokenDeGoogle");
		}

		internal static DateTime? ObtenerFechaExpiracionToken()
		{
			var valor = RecuperarValorDeCuentaLocal("fechaExpiracion");
			return !string.IsNullOrEmpty(valor) ? Convert.ToDateTime(valor) : (DateTime?)null;
		}

		internal static string ObtenerLinkHojaConsulta()
		{
			return RecuperarValorDeCuentaLocal("linkHojaConsulta");
		}

		internal static string ObtenerColumnasParaVer()
		{
			return RecuperarValorDeCuentaLocal("columnasParaVer");
		}

		internal static string ObtenerColumnasInventario()
		{
			return RecuperarValorDeCuentaLocal("columnasInventario");
		}

		internal static string ObtenerNombreUsuarioGoogle()
		{
			return RecuperarValorDeCuentaLocal("nombreUsuarioGoogle");
		}

		internal static string ObtenerLinkHojaHistorial()
		{
			return RecuperarValorDeCuentaLocal("linkHojaHistorial");
		}

		internal static string ObtenerTokenActualDeBaseDeDatos()
		{
			return RecuperarValorDeCuentaLocal("tokenDeBaseDeDatos");
		}

		internal static string ObtenerUsuarioDeBaseDeDatos()
		{
			return RecuperarValorDeCuentaLocal("usuarioDeBaseDeDatos");
		}

		internal static string ObtenerAccesoDatos()
		{
			return RecuperarValorDeCuentaLocal("accesoDatos");
		}

		internal static string ObtenerNombreHoja(string linkHojaConsulta)
		{
			return RecuperarValorDeCuentaLocal(linkHojaConsulta + "|nombre");
		}

		internal static List<string> ObtenerTodosLosNombresDeHojas()
		{
			var nombres = new List<string>();

			RecuperarCuentaLocal();
			if(_cuenta != null)
			{
				foreach(var llaveValor in _cuenta.Properties)
				{
					if (llaveValor.Key.Contains("|nombre"))
						nombres.Add(llaveValor.Value);
				}
			}

			return nombres;
		}

		internal static string ObtenerLinkHojaSeleccionada(string nombre)
		{
			var link = "";
			RecuperarCuentaLocal();
			if (_cuenta != null)
			{
				foreach (var llaveValor in _cuenta.Properties)
				{
					if (llaveValor.Key.Contains("|nombre") && llaveValor.Value.Equals(nombre))
					{
						link = llaveValor.Key.Split('|')[0];
						AlmacenarLinkHojaConsulta(link);
						AlmacenarColumnasParaVer(RecuperarValorDeCuentaLocal(link + "|ver"));
						AlmacenarColumnasInventario(RecuperarValorDeCuentaLocal(link + "|inventario"));
						break;
					}
				}
				foreach (var llaveValorH in _cuenta.Properties)
				{
					if (llaveValorH.Key.Contains("|historico") && llaveValorH.Value.Equals(nombre))
					{
						AlmacenarLinkHojaHistorial(llaveValorH.Key.Split('|')[0]);
						break;
					}
				}
			}

			return link;
		}

		internal static bool ValidarTokenDeGoogle()
		{
			if(string.IsNullOrEmpty(ObtenerTokenActualDeGoogle()) || ObtenerFechaExpiracionToken() <= DateTime.Now)
				return false;
			return true;
		}
	}
}
