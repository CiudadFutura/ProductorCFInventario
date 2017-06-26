
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

		public SpreadsheetFeed ObtenerListaLibros(SpreadsheetsService servicio)
		{
			var consulta = new SpreadsheetQuery();
			return servicio.Query(consulta);
		}

		public WorksheetFeed ObtenerListaHojas(string linkLibro, SpreadsheetsService servicio)
		{
			var consulta = new WorksheetQuery(linkLibro);
			return servicio.Query(consulta);
		}

		public CellFeed ObtenerCeldasDeUnaHoja(string linkHojaConsulta, SpreadsheetsService servicio)
		{
			var consulta = new CellQuery(linkHojaConsulta);
			var celdas = servicio.Query(consulta);

			return celdas;
		}

		public string ObtenerHistorico(CellEntry celdaMovimiento, double movimiento, string precio, string puntoVenta, CellEntry[] producto, 
			string[] nombresColumnas, string[] listaColumnasInventario)
		{
			// Abre la fila
			var fila = "<entry xmlns=\"http://www.w3.org/2005/Atom\" xmlns:gsx=\"http://schemas.google.com/spreadsheets/2006/extended\">";
			// Agrega la fecha
			fila += "<gsx:fecha>" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "</gsx:fecha>";
			// Agrega los valores del producto
			for (var i = 0; i < nombresColumnas.Length; i++)
			{
				var valor = producto[i].Value; //Si la columna no es de stock o es la que recibió el movimiento se inserta su valor en el histórico
				if (listaColumnasInventario[i] == "1" && i + 1 != celdaMovimiento.Column)
					valor = "-"; //Si la columna es de stock pero no la que recibió el movimiento el valor para el histórico es "-"

				var columna = Regex.Replace(nombresColumnas[i].ToLower(), @"\s+", "");
				fila += "<gsx:" + columna + ">" + valor + "</gsx:" + columna + ">";
			}

			// Agrega el movimiento
			fila += "<gsx:movimiento>" + movimiento + "</gsx:movimiento>";
			// Agrega el precio
			if(puntoVenta != "No")
				fila += "<gsx:puntoventa>" + puntoVenta + "</gsx:puntoventa>";
			// Agrega el punto de venta
			fila += "<gsx:precio>" + precio + "</gsx:precio>";
			// Agrega el usuario
			var usuario = CuentaUsuario.ObtenerNombreUsuarioGoogle() ?? "";
			fila += "<gsx:usuario>" + usuario + "</gsx:usuario>";
			// Cierra la fila
			fila += "</entry>";

			return fila;
		}

		public void InsertarHistoricos(SpreadsheetsService servicio, CellEntry celdaMovimiento, double movimiento, string precio, string puntoVenta, CellEntry[] producto, 
			string[] nombresColumnas, string[] listaColumnasInventario)
		{
			var url = CuentaUsuario.ObtenerLinkHojaHistoricos();
			const string tipoDelContenido = "application/atom+xml";
			var fila = ObtenerHistorico(celdaMovimiento, movimiento, precio, puntoVenta, producto, nombresColumnas, listaColumnasInventario);
			// Convierte el contenido de la fila en stream
			byte[] filaEnArregloDeBytes = Encoding.UTF8.GetBytes(fila);
			MemoryStream filaEnStream = new MemoryStream(filaEnArregloDeBytes);
			// Inserta la fila en la hoja Historial (Google)
			servicio.Insert(new Uri(url), filaEnStream, tipoDelContenido, "");
		}
		
	}

}