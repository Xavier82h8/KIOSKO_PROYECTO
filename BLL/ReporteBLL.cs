using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using KIOSKO_Proyecto.Datos;
using KIOSKO_Proyecto.Modelos;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace KIOSKO_Proyecto.BLL
{
    public class ReporteBLL
    {
        private ReporteDAL _reporteDAL = new ReporteDAL();

        public void GuardarReporte(Reporte reporte)
        {
            // Aquí se podría añadir lógica de negocio adicional antes de guardar
            _reporteDAL.GuardarReporte(reporte);
        }

        public List<Reporte> ObtenerTodosLosReportes()
        {
            return _reporteDAL.ObtenerTodosLosReportes();
        }

        public List<VentaDetalladaReporte> ObtenerVentasDetalladasPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            return _reporteDAL.ObtenerVentasDetalladasPorFecha(fechaInicio, fechaFin);
        }

        public Tuple<decimal, decimal> ObtenerTotalesCorteCaja(DateTime fecha)
        {
            return _reporteDAL.ObtenerTotalesCorteCaja(fecha);
        }

        public void ExportarReportesCSV(List<Reporte> reportes, string filePath)
        {
            if (reportes == null || !reportes.Any())
            {
                throw new ArgumentException("La lista de reportes no puede estar vacía.");
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Escribir encabezados
                    sw.WriteLine("IdReporte,FechaGeneracion,FechaInicio,FechaFin,TotalVentas,GeneradoPorEmpleadoId,NombreEmpleadoGenerador");

                    // Escribir datos
                    foreach (var reporte in reportes)
                    {
                        sw.WriteLine($"{reporte.IdReporte},{reporte.FechaGeneracion:yyyy-MM-dd HH:mm:ss},{reporte.FechaInicio:yyyy-MM-dd HH:mm:ss},{reporte.FechaFin:yyyy-MM-dd HH:mm:ss},{reporte.TotalVentas},{reporte.GeneradoPorEmpleadoId},{reporte.NombreEmpleadoGenerador}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar reportes a CSV: {ex.Message}", ex);
            }
        }

        public void GenerarCorteDeCajaPdf(Tuple<decimal, decimal> totales, DateTime fecha, string filePath)
        {
            var doc = new Document(PageSize.A4, 36, 36, 54, 36);
            try
            {
                PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.BLACK);
                var fontSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.DARK_GRAY);
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
                var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);

                doc.Add(new Paragraph("Corte de Caja", fontTitulo) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph($"Fecha: {fecha:dd/MM/yyyy}", fontSubtitulo) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph($"Generado el: {DateTime.Now:g}", fontSubtitulo) { Alignment = Element.ALIGN_CENTER });
                doc.Add(Chunk.NEWLINE);

                decimal totalVentas = totales.Item1 + totales.Item2;

                var resumenTable = new PdfPTable(2) { WidthPercentage = 60, HorizontalAlignment = Element.ALIGN_CENTER, SpacingBefore = 20f };
                resumenTable.SetWidths(new float[] { 3, 2 });

                resumenTable.AddCell(new PdfPCell(new Phrase("CONCEPTO", fontBold)) { Padding = 8, BackgroundColor = BaseColor.LIGHT_GRAY });
                resumenTable.AddCell(new PdfPCell(new Phrase("MONTO", fontBold)) { Padding = 8, BackgroundColor = BaseColor.LIGHT_GRAY, HorizontalAlignment = Element.ALIGN_RIGHT });

                resumenTable.AddCell(new PdfPCell(new Phrase("Total en Efectivo:", fontNormal)) { Padding = 6 });
                resumenTable.AddCell(new PdfPCell(new Phrase(totales.Item1.ToString("C2"), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 6 });

                resumenTable.AddCell(new PdfPCell(new Phrase("Total en Tarjeta:", fontNormal)) { Padding = 6 });
                resumenTable.AddCell(new PdfPCell(new Phrase(totales.Item2.ToString("C2"), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 6 });

                resumenTable.AddCell(new PdfPCell(new Phrase("Ingreso Total del Día:", fontBold)) { Padding = 8, BackgroundColor = BaseColor.LIGHT_GRAY });
                resumenTable.AddCell(new PdfPCell(new Phrase(totalVentas.ToString("C2"), fontBold)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 8, BackgroundColor = BaseColor.LIGHT_GRAY });

                doc.Add(resumenTable);
                doc.Add(Chunk.NEWLINE);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al generar el PDF del corte de caja.", ex);
            }
            finally
            {
                doc.Close();
            }
        }
    }
}
