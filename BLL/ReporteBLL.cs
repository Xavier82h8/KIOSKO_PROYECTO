using System;
using System.Collections.Generic;
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

        public List<VentaDetalladaReporte> GenerarReporteVentasDetallado(DateTime fechaInicio, DateTime fechaFin)
        {
            return _reporteDAL.ObtenerVentasDetalladasPorFecha(fechaInicio, fechaFin);
        }

        public CorteCaja GenerarCorteCajaDiario(DateTime fecha)
        {
            return _reporteDAL.ObtenerCorteCajaPorFecha(fecha);
        }

        public void ExportarVentasDetalladasCSV(List<VentaDetalladaReporte> data, string filePath)
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
            var sb = new StringBuilder();
            sb.AppendLine("VentaID,FechaVenta,NombreEmpleado,NombreProducto,Cantidad,PrecioUnitario,Subtotal,TotalVenta,MetodoPago");

            foreach (var item in data)
            {
                sb.AppendLine($"{item.VentaID},{item.FechaVenta:g},\"{item.NombreEmpleado}\",\"{item.NombreProducto}\",{item.Cantidad},{item.PrecioUnitario},{item.Subtotal},{item.TotalVenta},\"{item.MetodoPago}\"");
            }
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        public void ExportarCorteCajaPDF(CorteCaja corte, string filePath)
        {
            var doc = new Document(PageSize.A4);
        public void GenerarCorteDeCajaPdf(Tuple<decimal, decimal> totales, DateTime fecha, string filePath)
        {
            var doc = new Document(PageSize.A4, 36, 36, 54, 36);
            try
            {
                PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                // --- Fuentes ---
                var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var fontSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.DARK_GRAY);
                var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

                // --- Encabezado ---
                doc.Add(new Paragraph("Corte de Caja Diario", fontTitulo) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph($"Fecha: {corte.Fecha:D}", fontSubtitulo) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph($"Generado el: {DateTime.Now:g}", fontSubtitulo) { Alignment = Element.ALIGN_CENTER });
                doc.Add(Chunk.NEWLINE);

                // --- Resumen ---
                var resumenTable = new PdfPTable(2) { WidthPercentage = 60, HorizontalAlignment = Element.ALIGN_LEFT, SpacingAfter = 20 };
                resumenTable.AddCell(new PdfPCell(new Phrase("Total del Día:", fontBold)) { Border = Rectangle.NO_BORDER });
                resumenTable.AddCell(new PdfPCell(new Phrase(corte.TotalDia.ToString("C2"), fontBold)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.NO_BORDER });
                resumenTable.AddCell(new PdfPCell(new Phrase("Total en Efectivo:", fontNormal)) { Border = Rectangle.NO_BORDER });
                resumenTable.AddCell(new PdfPCell(new Phrase(corte.TotalEfectivo.ToString("C2"), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.NO_BORDER });
                resumenTable.AddCell(new PdfPCell(new Phrase("Total en Tarjeta:", fontNormal)) { Border = Rectangle.NO_BORDER });
                resumenTable.AddCell(new PdfPCell(new Phrase(corte.TotalTarjeta.ToString("C2"), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.NO_BORDER });
                doc.Add(resumenTable);

                // --- Tabla de Ventas ---
                var table = new PdfPTable(6) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 1, 3, 2, 2, 2, 2 });
                // Headers
                var headers = new string[] { "ID", "Hora", "Total Venta", "Monto Efectivo", "Monto Tarjeta", "Empleado" };
                foreach (var header in headers)
                {
                    table.AddCell(new PdfPCell(new Phrase(header, fontHeader)) { BackgroundColor = new BaseColor(45, 140, 200), Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                }
                // Rows
                foreach (var venta in corte.Ventas)
                {
                    table.AddCell(venta.VentaID.ToString());
                    table.AddCell(venta.FechaVenta.ToString("T"));
                    table.AddCell(new PdfPCell(new Phrase(venta.TotalVenta.ToString("C2"), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                    table.AddCell(new PdfPCell(new Phrase((venta.MontoEfectivo ?? 0).ToString("C2"), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                    table.AddCell(new PdfPCell(new Phrase((venta.MontoTarjeta ?? 0).ToString("C2"), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                    table.AddCell(venta.NombreEmpleado);
                }
                doc.Add(table);
            }
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
