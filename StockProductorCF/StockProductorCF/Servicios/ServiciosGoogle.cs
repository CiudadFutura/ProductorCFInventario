
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
			var servicio = new SpreadsheetsService("Productor de la Ciudad Futura");

			var parametros = new OAuth2Parameters {AccessToken = tokenDeAcceso};
			servicio.RequestFactory = new GOAuth2RequestFactory(null, "Productor de la Ciudad Futura", parametros);

			return servicio;
		}

		public AtomEntryCollection ObtenerListaLibros(SpreadsheetsService servicio)
		{
			var consulta = new SpreadsheetQuery();
			var libros = servicio.Query(consulta);
			return libros.Entries;
		}

		public AtomEntryCollection ObtenerListaHojas(string linkLibro, SpreadsheetsService servicio)
		{
			var consulta = new WorksheetQuery(linkLibro);
			var hojas = servicio.Query(consulta);
			return hojas.Entries;
		}

		public CellFeed ObtenerCeldasDeUnaHoja(string linkHojaConsulta, SpreadsheetsService servicio)
		{
			var consulta = new CellQuery(linkHojaConsulta);
			return servicio.Query(consulta);
		}

		public string ObtenerHistorico(CellEntry celdaMovimiento, double movimiento, double precio, string lugar, CellEntry[] producto, 
			string[] nombresColumnas, string[] listaColumnasInventario)
		{
			// Abre la fila
			var fila = "<entry xmlns=\"http://www.w3.org/2005/Atom\" xmlns:gsx=\"http://schemas.google.com/spreadsheets/2006/extended\">";
			// Agrega la fecha
			fila += "<gsx:fecha>" + DateTime.Now.ToString("MM-yyyy") + "</gsx:fecha>";
			// Agrega los valores del producto
			for (var i = 0; i < nombresColumnas.Length; i++)
			{
				var valor = producto[i].Value; //Si la columna no es de stock o es la que recibió el movimiento se inserta su valor en el histórico
				if (listaColumnasInventario[i] == "1" && i + 1 != celdaMovimiento.Column)
					valor = "-"; //Si la columna es de stock pero no la que recibió el movimiento el valor para el histórico es "-"

				var columna = Regex.Replace(nombresColumnas[i].ToLower(), @"\s+", "");
				fila += "<gsx:" + columna + ">" + valor + "</gsx:" + columna + ">";
			}

			// Agrega el Movimiento
			fila += "<gsx:cantidad>" + movimiento + "</gsx:cantidad>";
			// Agrega el Precio total
			fila += "<gsx:preciototal>" + Math.Abs(precio) + "</gsx:preciototal>";
			// Agrega el Costo unitario
			var costoUnitario = movimiento != 0 ? Math.Round(Math.Abs(precio / movimiento), 2) : 0;
			fila += "<gsx:costounitario>" + costoUnitario + "</gsx:costounitario>";
			// Agrega el Lugar (proveedor o punto de venta)
			fila += "<gsx:lugar>" + lugar + "</gsx:lugar>";
			// Agrega el Usuario
			var usuario = CuentaUsuario.ObtenerNombreUsuarioGoogle() ?? "-";
			fila += "<gsx:usuario>" + usuario + "</gsx:usuario>";
			// Agrega Eliminado
			fila += "<gsx:eliminado>-</gsx:eliminado>";
			// Agrega Eliminado por
			fila += "<gsx:eliminadopor>-</gsx:eliminadopor>";
			// Cierra la fila
			fila += "</entry>";

			return fila;
		}

		public void InsertarHistoricos(SpreadsheetsService servicio, CellEntry celdaMovimiento, double movimiento, double precio, string puntoVenta, CellEntry[] producto, 
			string[] nombresColumnas, string[] listaColumnasInventario)
		{
			var url = CuentaUsuario.ObtenerLinkHojaHistoricos();
			const string tipoDelContenido = "application/atom+xml";
			var fila = ObtenerHistorico(celdaMovimiento, movimiento, precio, puntoVenta, producto, nombresColumnas, listaColumnasInventario);
			// Convierte el contenido de la fila en stream
			var filaEnArregloDeBytes = Encoding.UTF8.GetBytes(fila);
			var filaEnStream = new MemoryStream(filaEnArregloDeBytes);
			// Inserta la fila en la hoja Historial (Google)
			servicio.Insert(new Uri(url), filaEnStream, tipoDelContenido, "");
		}
		
	}

}