using KIOSKO_Proyecto.Datos;
using KIOSKO_Proyecto.Modelos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace KIOSKO_Proyecto.BLL
{
    public class ReporteBLL
    {
        private readonly ReporteDAL _reporteDAL = new ReporteDAL();

        public List<VentaDetalladaReporte> GenerarReporteVentasDetallado(DateTime fechaInicio, DateTime fechaFin)
        {
            try { return _reporteDAL.ObtenerVentasDetalladasPorFecha(fechaInicio, fechaFin); }
            catch (Exception ex) { throw new Exception("Error BLL Ventas: " + ex.Message); }
        }

        public CorteCaja GenerarCorteCajaDiario(DateTime fecha)
        {
            try { return _reporteDAL.ObtenerCorteCajaPorFecha(fecha); }
            catch (Exception ex) { throw new Exception("Error BLL Corte: " + ex.Message); }
        }

        // NUEVO: Registrar corte en BD
        public bool RegistrarCorteCaja(HistorialCorte corte)
        {
            try { return _reporteDAL.GuardarCorte(corte); }
            catch (Exception ex) { throw new Exception("Error guardando corte: " + ex.Message); }
        }

        public void ExportarVentasDetalladasCSV(List<VentaDetalladaReporte> data, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("VentaID,FechaVenta,NombreEmpleado,NombreProducto,Cantidad,PrecioUnitario,Subtotal,TotalVenta,MetodoPago");
            foreach (var item in data)
            {
                sb.AppendLine($"{item.VentaID},{item.FechaVenta:g},\"{item.NombreEmpleado}\",\"{item.NombreProducto}\",{item.Cantidad},{item.PrecioUnitario},{item.Subtotal},{item.TotalVenta},\"{item.MetodoPago}\"");
            }
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        // NUEVO: PDF mejorado con datos de arqueo
        public void ExportarCorteCajaPDF(CorteCaja corte, HistorialCorte datosArqueo, string filePath)
        {
            var doc = new Document(PageSize.A4);
            try
            {
                PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                // Estilos
                var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var fontSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.DARK_GRAY);
                var fontNegrita = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                var fontRojo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.RED);
                var fontVerde = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, new BaseColor(0, 100, 0));

                // 1. Encabezado
                doc.Add(new Paragraph("REPORTE DE CIERRE DE CAJA", fontTitulo) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph($"Generado el: {DateTime.Now:g}", fontSubtitulo) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph("------------------------------------------------------", fontNormal) { Alignment = Element.ALIGN_CENTER });
                doc.Add(Chunk.NEWLINE);

                // 2. Datos Generales
                var tablaInfo = new PdfPTable(2) { WidthPercentage = 100 };
                tablaInfo.AddCell(new PdfPCell(new Phrase($"Cajero: {datosArqueo.NombreEmpleado}", fontNegrita)) { Border = Rectangle.NO_BORDER });
                tablaInfo.AddCell(new PdfPCell(new Phrase($"Fecha Corte: {corte.Fecha:D}", fontNegrita)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
                doc.Add(tablaInfo);
                doc.Add(Chunk.NEWLINE);

                // 3. Resumen Financiero
                var tableResumen = new PdfPTable(2) { WidthPercentage = 60, HorizontalAlignment = Element.ALIGN_LEFT };
                tableResumen.AddCell(new PdfPCell(new Phrase("RESUMEN DEL SISTEMA", fontNegrita)) { Colspan = 2, BackgroundColor = BaseColor.LIGHT_GRAY, HorizontalAlignment = Element.ALIGN_CENTER });
                
                tableResumen.AddCell(new PdfPCell(new Phrase("Ventas Efectivo:", fontNormal)));
                tableResumen.AddCell(new PdfPCell(new Phrase(corte.TotalEfectivo.ToString("C2"), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                
                tableResumen.AddCell(new PdfPCell(new Phrase("Ventas Tarjeta:", fontNormal)));
                tableResumen.AddCell(new PdfPCell(new Phrase(corte.TotalTarjeta.ToString("C2"), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });

                tableResumen.AddCell(new PdfPCell(new Phrase("TOTAL ESPERADO:", fontNegrita)));
                tableResumen.AddCell(new PdfPCell(new Phrase(corte.TotalDia.ToString("C2"), fontNegrita)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                doc.Add(tableResumen);
                doc.Add(Chunk.NEWLINE);

                // 4. Arqueo (Blind Count)
                var tableArqueo = new PdfPTable(2) { WidthPercentage = 60, HorizontalAlignment = Element.ALIGN_LEFT };
                tableArqueo.AddCell(new PdfPCell(new Phrase("ARQUEO DE CAJA (FÍSICO)", fontNegrita)) { Colspan = 2, BackgroundColor = BaseColor.LIGHT_GRAY, HorizontalAlignment = Element.ALIGN_CENTER });

                tableArqueo.AddCell(new PdfPCell(new Phrase("Dinero Contado por Cajero:", fontNormal)));
                tableArqueo.AddCell(new PdfPCell(new Phrase(datosArqueo.TotalReal.ToString("C2"), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });

                tableArqueo.AddCell(new PdfPCell(new Phrase("DIFERENCIA:", fontNegrita)));
                var celdaDif = new PdfPCell(new Phrase(datosArqueo.Diferencia.ToString("C2"), datosArqueo.Diferencia < 0 ? fontRojo : fontVerde));
                celdaDif.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableArqueo.AddCell(celdaDif);

                doc.Add(tableArqueo);
                doc.Add(Chunk.NEWLINE);

                // 5. Firmas
                doc.Add(Chunk.NEWLINE); doc.Add(Chunk.NEWLINE);
                var tablaFirmas = new PdfPTable(2) { WidthPercentage = 80 };
                tablaFirmas.AddCell(new PdfPCell(new Phrase("__________________________\nFirma Cajero", fontNormal)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER });
                tablaFirmas.AddCell(new PdfPCell(new Phrase("__________________________\nFirma Supervisor", fontNormal)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER });
                doc.Add(tablaFirmas);
            }
            finally
            {
                if (doc != null && doc.IsOpen()) doc.Close();
            }
        }
    }
}