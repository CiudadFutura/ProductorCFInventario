﻿
using Google.GData.Client;
using Google.GData.Spreadsheets;
using StockProductorCF.Clases;
using System.IO;
using System.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace StockProductorCF.Servicios
{
	public class ServiciosGoogle
	{
		public SpreadsheetsService ObtenerServicioParaConsultaGoogleSpreadsheets(string tokenDeAcceso)
		{
			var servicio = new SpreadsheetsService("Productor de la Ciudad Futura");

			var parametros = new OAuth2Parameters { AccessToken = tokenDeAcceso };
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
			var consulta = new CellQuery(linkHojaConsulta) {ReturnEmpty = ReturnEmptyCells.yes};
			return servicio.Query(consulta);
		}

		public string ObtenerHistorico(CellEntry celdaMovimiento, double cantidad, double precio, string lugar, CellEntry[] producto,
			string[] nombresColumnas, string[] listaColumnasInventario, string comentario)
		{
			// Abre la fila
			var fila = "<entry xmlns=\"http://www.w3.org/2005/Atom\" xmlns:gsx=\"http://schemas.google.com/spreadsheets/2006/extended\">";
														// Agrega la fecha
			fila += "<gsx:fecha>" + DateTime.Now.ToString("dd-MM-yyyy") + "</gsx:fecha>";
			// Agrega los valores del producto
			for (var i = 0; i < nombresColumnas.Length; i++)
			{
				var valor = producto[i].Value; //Si la columna no es de stock o es la que recibió el movimiento se inserta su valor en el histórico
				if (listaColumnasInventario[i] == "1" && i + 1 != celdaMovimiento.Column)
					valor = "-"; //Si la columna es de stock pero no la que recibió el movimiento el valor para el histórico es "-"

				var columna = Regex.Replace(nombresColumnas[i].ToLower(), @"\s+", "");
				fila += "<gsx:" + columna + ">" + WebUtility.HtmlEncode(valor) + "</gsx:" + columna + ">";
			}

			// Agrega el Movimiento
			fila += "<gsx:cantidad>" + cantidad.ToString().Replace('.', ',') + "</gsx:cantidad>";
			// Agrega el Precio total
			fila += "<gsx:preciototal>" + Math.Abs(precio) + "</gsx:preciototal>";
			// Agrega el Lugar (proveedor o punto de venta)
			fila += "<gsx:lugar>" + WebUtility.HtmlEncode(lugar) + "</gsx:lugar>";
			// Agrega Comentario
			comentario = string.IsNullOrEmpty(comentario) ? "-" : WebUtility.HtmlEncode(comentario.Trim());
			fila += "<gsx:comentario>" + comentario + "</gsx:comentario>";
			// Agrega el Usuario
			var usuario = CuentaUsuario.ObtenerNombreUsuarioGoogle() ?? "-";
			fila += "<gsx:usuario>" + WebUtility.HtmlEncode(usuario) + "</gsx:usuario>";
			// Agrega Eliminado
			fila += "<gsx:eliminado>-</gsx:eliminado>";
			// Agrega Eliminado por
			fila += "<gsx:eliminadopor>-</gsx:eliminadopor>";

			//fila += "<batch:id>item" + u + "</batch:id>";
			//fila += "<batch:operation type=\"insert\"/>";
			//fila += "<g:item_type>recipes</g:item_type>";

			// Cierra la fila
			fila += "</entry>";

			return fila;
		}

		public void EnviarMovimiento(SpreadsheetsService servicio, CellEntry celdaMovimiento, double cantidad, double precio, string puntoVenta, string comentario,
			CellEntry[] producto, string[] nombresColumnas, string[] listaColumnasInventario, string url)
		{
			var movimiento = AgregarHistorico(celdaMovimiento, cantidad, precio, puntoVenta, producto, nombresColumnas, listaColumnasInventario, comentario);
			EnviarFilas(movimiento, servicio, url);
		}

		private string AgregarHistorico(CellEntry celdaMovimiento, double cantidad, double precio, string puntoVenta, CellEntry[] producto,
			string[] nombresColumnas, string[] listaColumnasInventario, string comentario)
		{
			return ObtenerHistorico(celdaMovimiento, cantidad, precio, puntoVenta, producto, nombresColumnas, listaColumnasInventario, comentario);
		}

		private static void EnviarFilas(string filas, SpreadsheetsService servicio, string url)
		{
			const string tipoDelContenido = "application/atom+xml";
			// Convierte el contenido de la fila en stream
			var filaEnArregloDeBytes = Encoding.UTF8.GetBytes(filas);
			var filaEnStream = new MemoryStream(filaEnArregloDeBytes);
			// Inserta la fila en la hoja Historial (Google)
			servicio.Insert(new Uri(url), filaEnStream, tipoDelContenido, "");
			//servicio.StreamSend(new Uri(url), filaEnStream, GDataRequestType.Batch,  tipoDelContenido, "", "");
		}

		internal void InsertarMovimientosRelaciones(SpreadsheetsService servicio, double cantidad, CellEntry[] producto)
		{
			//Valida que sea la hoja de productos a la que se le asocia la relaciones
			var paginaRelaciones = CuentaUsuario.RecuperarValorDeCuentaLocal(CuentaUsuario.ObtenerLinkHojaConsulta() + "|relacionesInsumoProducto");
			if (paginaRelaciones == null) return;

			var linkInsumos = CuentaUsuario.ObtenerLinkHojaPorNombre("Materias Primas App");
			var linkHistoricoInsumos = CuentaUsuario.ObtenerLinkHojaHistoricosParaLinkHoja(linkInsumos);
			var relaciones = CuentaUsuario.ObtenerRelacionesInsumoProducto();

			if (string.IsNullOrEmpty(relaciones) || string.IsNullOrEmpty(linkInsumos) || string.IsNullOrEmpty(linkHistoricoInsumos)) return;
			var relacionesArreglo = relaciones.Split('?');

			var celdas = new ServiciosGoogle().ObtenerCeldasDeUnaHoja(linkInsumos, servicio);
			var columnasInventario = CuentaUsuario.RecuperarValorDeCuentaLocal(linkInsumos + "|inventario").Split(',');

			var insumosRelacion = new Dictionary<string, string>();
			foreach (var arreglo in relacionesArreglo)
			{
				var relacion = arreglo.Split('|');
				if (relacion[0] == producto[0].Value) //Compara código de prod de la relación y del producto seleccionado
					insumosRelacion.Add(relacion[1], relacion[2].Replace('.', ',')); //Inserta el código del insumo de la relación
			}

			//Obtener el arreglo del insumo para actualizar
			var nombresColumnas = new string[celdas.ColCount.Count];
			var insumoSeleccionado = new CellEntry[celdas.ColCount.Count];
			var fila = -1;
			double cantInsumoRelacion = 0;
			var celdaMovimiento = new CellEntry();
			//var movimientos = "";
			foreach (CellEntry celda in celdas.Entries)
			{
				if (celda.Row != 1)
				{
					if (celda.Column == 1 && insumosRelacion.ContainsKey(celda.Value)) //Compara código de insumo con insumo de relación
					{
						fila = (int)celda.Row;
						double.TryParse(insumosRelacion[celda.Value], NumberStyles.AllowDecimalPoint, new CultureInfo("es-ES"), out cantInsumoRelacion);
					}
					if (celda.Row == fila)
						insumoSeleccionado.SetValue(celda, (int)celda.Column - 1); //Va recuperando los valores del Insumo

					if (columnasInventario[(int)celda.Column - 1] == "1") //Toma celda de stock del insumo para descontar (si tiene 1 toma esa, si tiene más de 1 toma la última)
						celdaMovimiento = celda;

					//Si encontró producto (fila > -1) y ya pasó alpróximo producto (celda.Row > fila) o es el último producto (celda.Column == _celdas.ColCount.Count)
					if (fila > -1 && (celda.Row > fila || celda.Column == celdas.ColCount.Count))
					{
						//Se agregan filas de movimientos de insumos para enviar al final
						var movimiento = AgregarHistorico(celdaMovimiento, -1 * cantidad * cantInsumoRelacion, 0, "-", insumoSeleccionado, nombresColumnas, columnasInventario,
							"Disminución por producción de " + cantidad + " producto/s (" + producto[1].Value + ").");
						EnviarFilas(movimiento, servicio, linkHistoricoInsumos);
						fila = -1;
					}
				}
				else
					nombresColumnas.SetValue(celda.Value, (int)celda.Column - 1);
			}

			//Si hay, se envían las filas de movimientos de insumos
			//if (!string.IsNullOrEmpty(movimientos))
			//{
			//	movimientos = "<feed xmlns=\"http://www.w3.org/2005/Atom\" xmlns:openSearch=\"http://a9.com/-/spec/opensearchrss/1.0/\" xmlns:g=\"http://base.google.com/ns/1.0\" xmlns:batch=\"http://schemas.google.com/gdata/batch\">"
			//		+ movimientos + "</feed>";
			//	EnviarFilas(movimientos, servicio, linkHistoricoInsumos);
			//}
			
		}
	}

}