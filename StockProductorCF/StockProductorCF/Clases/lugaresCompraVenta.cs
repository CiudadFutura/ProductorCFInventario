﻿
using Google.GData.Spreadsheets;
using StockProductorCF.Servicios;

namespace StockProductorCF.Clases
{
	public class LugaresCompraVenta
	{
		private CellFeed _celdas;

		public void ObtenerActualizarLugares(string linkHojaPrincipal, SpreadsheetsService servicio)
		{
			//Recibe el link de la hoja principal, obtiene el link de lugares (si tiene) para la hoja actual y lo actualiza (por si se agregaron lugares)
			var paginaLugares = CuentaUsuario.RecuperarValorDeCuentaLocal(linkHojaPrincipal + "|hojaPuntosVenta");
			if (paginaLugares == null) return;

			_celdas = new ServiciosGoogle().ObtenerCeldasDeUnaHoja(paginaLugares, servicio);

			var puntosVentaTexto = "";
			foreach (CellEntry celda in _celdas.Entries)
			{
				if (celda.Row != 1)
					puntosVentaTexto += celda.Value + "|";
			}

			CuentaUsuario.AlmacenarPuntosVenta(puntosVentaTexto.TrimEnd('|'));
		}
	}
}
