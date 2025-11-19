using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KIOSKO_Proyecto.Modelos;
using KIOSKO_Proyecto.Datos;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace KIOSKO_Proyecto.BLL
{
    public class VentaBLL
    {
        private VentaDAL _ventaDAL = new VentaDAL();
        private ProductoBLL _productoBLL = new ProductoBLL(); // Para obtener nombres de productos

        public Venta RegistrarVenta(Venta venta)
        {
            if (venta == null || !venta.Detalles.Any()) return null;
            if (venta.TotalVenta < 0) return null; // Permitir ventas de 0 si hay promos
            if (venta.FechaVenta == default(DateTime)) venta.FechaVenta = DateTime.Now;
            return _ventaDAL.CrearVenta(venta);
        }

        public List<Venta> ObtenerVentasPorRango(DateTime desde, DateTime hasta)
        {
            return _ventaDAL.ObtenerVentasPorRango(desde, hasta);
        }

        public List<DetalleVenta> ObtenerDetalleVenta(int ventaId)
        {
            return _ventaDAL.ObtenerDetalleVenta(ventaId);
        }

        public void ExportarTicketPDF(Venta venta, string filePath)
        {
            if (venta == null || venta.Detalles == null || !venta.Detalles.Any())
            {
                throw new Exception("La venta no contiene detalles para exportar.");
            }

            // Configuración de fuentes y documento
            var doc = new Document(PageSize.A7, 25, 25, 30, 30);
            var writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
            doc.Open();

            var fontNormal = FontFactory.GetFont("Arial", 8, Font.NORMAL);
            var fontBold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            var fontTitle = FontFactory.GetFont("Arial", 10, Font.BOLD);
            var fontMono = FontFactory.GetFont(FontFactory.COURIER, 8, Font.NORMAL);

            // Encabezado
            doc.Add(new Paragraph("Kioskito ITH", fontTitle) { Alignment = Element.ALIGN_CENTER });
            doc.Add(new Paragraph($"Ticket de Venta #{venta.VentaID}", fontNormal) { Alignment = Element.ALIGN_CENTER });
            doc.Add(new Paragraph($"Fecha: {venta.FechaVenta:g}", fontNormal) { Alignment = Element.ALIGN_CENTER });
            doc.Add(Chunk.NEWLINE);

            // Detalles de la venta
            var table = new PdfPTable(4) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 4f, 1f, 2f, 2f });
            table.AddCell(new PdfPCell(new Phrase("Producto", fontBold)) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase("Cant", fontBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Precio", fontBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase("Subtotal", fontBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

            // Línea separadora
            var cell = new PdfPCell(new Phrase("")) { Colspan = 4, Border = Rectangle.BOTTOM_BORDER, BorderWidthBottom = 1f, PaddingBottom = 4 };
            table.AddCell(cell);

            foreach (var detalle in venta.Detalles)
            {
                var producto = _productoBLL.ObtenerProductoPorId(detalle.ProductoID);
                table.AddCell(new PdfPCell(new Phrase(producto?.Nombre ?? "N/A", fontMono)) { Border = 0, PaddingTop = 4 });
                table.AddCell(new PdfPCell(new Phrase(detalle.Cantidad.ToString(), fontMono)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER, PaddingTop = 4 });
                table.AddCell(new PdfPCell(new Phrase(detalle.PrecioUnitario.ToString("C2"), fontMono)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT, PaddingTop = 4 });
                table.AddCell(new PdfPCell(new Phrase(detalle.Subtotal.ToString("C2"), fontMono)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT, PaddingTop = 4 });
            }
            doc.Add(table);

            // Totales
            doc.Add(Chunk.NEWLINE);
            var paraTotal = new Paragraph();
            paraTotal.Alignment = Element.ALIGN_RIGHT;
            paraTotal.Add(new Chunk("TOTAL: ", fontBold));
            paraTotal.Add(new Chunk(venta.TotalVenta.ToString("C2"), fontTitle));
            doc.Add(paraTotal);

            if (venta.MontoEfectivo.HasValue)
                doc.Add(new Paragraph($"Pagado (Efectivo): {venta.MontoEfectivo.Value:C2}", fontNormal) { Alignment = Element.ALIGN_RIGHT });
            if (venta.MontoTarjeta.HasValue)
                doc.Add(new Paragraph($"Pagado (Tarjeta): {venta.MontoTarjeta.Value:C2}", fontNormal) { Alignment = Element.ALIGN_RIGHT });
            if (venta.Cambio.HasValue && venta.Cambio > 0)
                doc.Add(new Paragraph($"Cambio: {venta.Cambio.Value:C2}", fontNormal) { Alignment = Element.ALIGN_RIGHT });

            if (!string.IsNullOrEmpty(venta.MetodoPago))
                doc.Add(new Paragraph($"Método de Pago: {venta.MetodoPago}", fontNormal) { Alignment = Element.ALIGN_RIGHT });

            // Pie de página
            doc.Add(Chunk.NEWLINE);
            doc.Add(new Paragraph("¡Gracias por su compra!", fontNormal) { Alignment = Element.ALIGN_CENTER });

            doc.Close();
        }

        public List<VentaDetalladaReporte> ObtenerVentasDetalladasPorRango(DateTime desde, DateTime hasta)
        {
            return _ventaDAL.ObtenerVentasDetalladasPorRango(desde, hasta);
        }
    }
}
