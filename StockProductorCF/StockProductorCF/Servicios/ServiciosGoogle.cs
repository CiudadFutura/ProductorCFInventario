
using Google.GData.Client;
using Google.GData.Spreadsheets;
using StockProductorCF.Clases;
using System.IO;
using System.Text;
using System;
using System.Text.RegularExpressions;

namespace StockProductorCF.Servicios
{
	public class ServiciosGoogle
	{
		public SpreadsheetsService ObtenerServicioParaConsultaGoogleSpreadsheets(string tokenDeAcceso)
		{
			SpreadsheetsService servicio = new SpreadsheetsService("Productor de la Ciudad Futura");

			var parametros = new OAuth2Parameters();
			parametros.AccessToken = tokenDeAcceso;
			servicio.RequestFactory = new GOAuth2RequestFactory(null, "Productor de la Ciudad Futura", parametros);

			return servicio;
		}

		public SpreadsheetFeed ObtenerListaLibros(SpreadsheetsService servicio)
		{
			SpreadsheetQuery consulta = new SpreadsheetQuery();
			return servicio.Query(consulta);
		}

		public WorksheetFeed ObtenerListaHojas(AtomEntry libro, SpreadsheetsService servicio)
		{
			AtomLink link = libro.Links.FindService(GDataSpreadsheetsNameTable.WorksheetRel, null);

			WorksheetQuery consulta = new WorksheetQuery(link.HRef.ToString());
			return servicio.Query(consulta);
		}

		public CellFeed ObtenerCeldasDeUnaHoja(string linkHojaConsulta, SpreadsheetsService servicio)
		{
			CellQuery consulta = new CellQuery(linkHojaConsulta);
			CellFeed celdas = servicio.Query(consulta);

			return celdas;
		}

		public string ObtenerHistorico(CellEntry celdaMovimiento, double movimiento, CellEntry[] producto, string[] nombresColumnas, string[] listaColumnasInventario)
		{
			var columna = "";
			var valor = "";

			// Abre la fila
			var fila = "<entry xmlns=\"http://www.w3.org/2005/Atom\" xmlns:gsx=\"http://schemas.google.com/spreadsheets/2006/extended\">";
			// Agrega la fecha
			fila += "<gsx:fecha>" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "</gsx:fecha>";
			// Agrega los valores del producto
			for (var i = 0; i < nombresColumnas.Length; i++)
			{
				if (listaColumnasInventario[i] == "1" && i + 1 != celdaMovimiento.Column)
					valor = "-"; // Si la columna es de stock pero no la que recibió el movimiento el valor para el histórico es "-"
				else
					valor = producto[i].Value;

				columna = Regex.Replace(nombresColumnas[i].ToLower(), @"\s+", "");
				fila += "<gsx:" + columna + ">" + valor + "</gsx:" + columna + ">";
			}

			// Agrega el movimiento
			fila += "<gsx:movimiento>" + movimiento + "</gsx:movimiento>";
			// Agrega el usuario
			var usuario = CuentaUsuario.ObtenerNombreUsuarioGoogle() ?? "";
			fila += "<gsx:usuario>" + usuario + "</gsx:usuario>";
			// Cierra la fila
			fila += "</entry>";

			return fila;
		}

		public void InsertarHistoricos(SpreadsheetsService servicio, CellEntry celdaMovimiento, double movimiento, CellEntry[] producto, string[] nombresColumnas, string[] listaColumnasInventario)
		{
			var url = CuentaUsuario.ObtenerLinkHojaHistorial();
			var tipoDelContenido = "application/atom+xml";
			var fila = ObtenerHistorico(celdaMovimiento, movimiento, producto, nombresColumnas, listaColumnasInventario);
			// Convierte el contenido de la fila en stream
			byte[] filaEnArregloDeBytes = Encoding.UTF8.GetBytes(fila);
			MemoryStream filaEnStream = new MemoryStream(filaEnArregloDeBytes);
			// Inserta la fila en la hoja Historial (Google)
			servicio.Insert(new Uri(url), filaEnStream, tipoDelContenido, "");
		}
		
	}

}