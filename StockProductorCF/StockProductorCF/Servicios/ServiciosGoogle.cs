

using Google.GData.Client;
using Google.GData.Spreadsheets;

namespace StockProductorCF.Servicios
{
    public class ServiciosGoogle
    {
        public SpreadsheetsService ObtenerServicioParaConsultaGoogleSpreadsheets(string tokenDeAcceso)
        {
            SpreadsheetsService servicio = new SpreadsheetsService("Inventario - Productor de la Ciudad Futura");

            var parametros = new OAuth2Parameters();
            parametros.AccessToken = tokenDeAcceso;
            servicio.RequestFactory = new GOAuth2RequestFactory(null, "Inventario - Productor de la Ciudad Futura", parametros);

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


        public void ActualizarCelda(SpreadsheetsService servicio, AtomEntry celda)
        {
            servicio.Update(celda);
        }
    }

}