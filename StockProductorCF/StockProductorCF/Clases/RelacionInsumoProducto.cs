
using Google.GData.Spreadsheets;
using StockProductorCF.Servicios;

namespace StockProductorCF.Clases
{
	public class RelacionesInsumoProducto
	{
		private CellFeed _celdas;

		public void ObtenerActualizarRelaciones(string linkHojaPrincipal, SpreadsheetsService servicio)
		{
			//Recibe el link de la hoja principal, obtiene el link de relaciones (si tiene) para la hoja actual y lo actualiza (por si se agregaron relaciones)
			var paginaRelaciones = CuentaUsuario.RecuperarValorDeCuentaLocal(linkHojaPrincipal + "|relacionesInsumoProducto");
			if (paginaRelaciones == null) return;

			_celdas = new ServiciosGoogle().ObtenerCeldasDeUnaHoja(paginaRelaciones, servicio);

			var relacionesTexto = "";
			foreach (CellEntry celda in _celdas.Entries)
			{
				if (celda.Row > 2)
				{
					if (celda.Column == 1 || celda.Column == 3) // 1: Código producto - 3: Código materia prima
						relacionesTexto += celda.Value + "|";
					if (celda.Column == 5) // 5: Cantidad de materia prima (3) que lleva el producto (1)
						relacionesTexto += celda.Value + "?";
				}
					
			}

			CuentaUsuario.AlmacenarRelacionesInsumoProducto(relacionesTexto.TrimEnd('?'));
		}
	}
}
