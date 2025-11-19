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
            finally
            {
                doc.Close();
            }
        }
    }
}
