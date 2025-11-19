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

        public void GenerarCorteDeCajaPdf(List<VentaDetalladaReporte> ventas, DateTime desde, DateTime hasta, string filePath)
        {
            if (ventas == null || !ventas.Any())
            {
                throw new ArgumentException("No hay datos de ventas para generar el reporte.");
            }

            var doc = new Document(PageSize.A4, 36, 36, 54, 36);
            try
            {
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                // --- Fuentes ---
                var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.BLACK);
                var fontSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.DARK_GRAY);
                var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
                var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.BLACK);

                // --- Encabezado ---
                doc.Add(new Paragraph("Corte de Caja", fontTitulo) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph($"Periodo del {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}", fontSubtitulo) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph($"Generado el: {DateTime.Now:g}", fontSubtitulo) { Alignment = Element.ALIGN_CENTER });
                doc.Add(Chunk.NEWLINE);

                // --- Resumen ---
                decimal totalVentas = ventas.Sum(v => v.Subtotal);
                decimal totalEfectivo = ventas.Where(v => v.MetodoPago == "Efectivo" || v.MetodoPago == "Mixto").GroupBy(v => v.VentaID).Select(g => g.First().MontoEfectivo ?? 0).Sum();
                decimal totalTarjeta = ventas.Where(v => v.MetodoPago == "Tarjeta" || v.MetodoPago == "Mixto").GroupBy(v => v.VentaID).Select(g => g.First().MontoTarjeta ?? 0).Sum();
                int numeroVentas = ventas.Select(v => v.VentaID).Distinct().Count();
                int totalProductos = ventas.Sum(v => v.Cantidad);

                var resumenTable = new PdfPTable(2) { WidthPercentage = 50, HorizontalAlignment = Element.ALIGN_LEFT };
                resumenTable.SetWidths(new float[] { 3, 2 });
                resumenTable.AddCell(new PdfPCell(new Phrase("RESUMEN GENERAL", fontBold)) { Colspan = 2, Padding = 5, BackgroundColor = BaseColor.LIGHT_GRAY, Border = Rectangle.NO_BORDER });
                resumenTable.AddCell(new PdfPCell(new Phrase("Ingreso Total:", fontNormal)) { Border = Rectangle.NO_BORDER, Padding = 4 });
                resumenTable.AddCell(new PdfPCell(new Phrase(totalVentas.ToString("C2"), fontBold)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                resumenTable.AddCell(new PdfPCell(new Phrase("Total en Efectivo:", fontNormal)) { Border = Rectangle.NO_BORDER, Padding = 4 });
                resumenTable.AddCell(new PdfPCell(new Phrase(totalEfectivo.ToString("C2"), fontNormal)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                resumenTable.AddCell(new PdfPCell(new Phrase("Total en Tarjeta:", fontNormal)) { Border = Rectangle.NO_BORDER, Padding = 4 });
                resumenTable.AddCell(new PdfPCell(new Phrase(totalTarjeta.ToString("C2"), fontNormal)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                resumenTable.AddCell(new PdfPCell(new Phrase("Número de Ventas:", fontNormal)) { Border = Rectangle.NO_BORDER, Padding = 4 });
                resumenTable.AddCell(new PdfPCell(new Phrase(numeroVentas.ToString(), fontNormal)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                resumenTable.AddCell(new PdfPCell(new Phrase("Total de Productos Vendidos:", fontNormal)) { Border = Rectangle.NO_BORDER, Padding = 4 });
                resumenTable.AddCell(new PdfPCell(new Phrase(totalProductos.ToString(), fontNormal)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                doc.Add(resumenTable);
                doc.Add(Chunk.NEWLINE);

                // --- Tabla de Detalles ---
                doc.Add(new Paragraph("Detalle de Ventas", fontSubtitulo));
                var table = new PdfPTable(7) { WidthPercentage = 100, SpacingBefore = 10f };
                table.SetWidths(new float[] { 8, 15, 25, 20, 10, 10, 12 });

                // Headers
                var headers = new string[] { "Venta ID", "Fecha", "Producto", "Empleado", "Cant.", "Precio", "Subtotal" };
                foreach (var header in headers)
                {
                    table.AddCell(new PdfPCell(new Phrase(header, fontHeader)) { BackgroundColor = new BaseColor(45, 140, 200), Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                }

                // Rows
                foreach (var item in ventas)
                {
                    table.AddCell(new PdfPCell(new Phrase(item.VentaID.ToString(), fontNormal)) { Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.FechaVenta.ToString("g"), fontNormal)) { Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.NombreProducto, fontNormal)) { Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.NombreEmpleado, fontNormal)) { Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.Cantidad.ToString(), fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.PrecioUnitario.ToString("C2"), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.Subtotal.ToString("C2"), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                }
                doc.Add(table);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                doc.Close();
            }
        }
    }
}
