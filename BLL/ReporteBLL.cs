using KIOSKO_Proyecto.Datos;
using KIOSKO_Proyecto.Modelos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace KIOSKO_Proyecto.BLL
{
    public class ReporteBLL
    {
        private readonly ReporteDAL _reporteDAL = new ReporteDAL();

        // ============================================================
        // 1. GENERAR REPORTE DE VENTAS DETALLADO
        // ============================================================
        public List<VentaDetalladaReporte> GenerarReporteVentasDetallado(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                // Validar fechas
                if (fechaInicio > fechaFin)
                {
                    throw new ArgumentException("La fecha de inicio no puede ser mayor que la fecha fin.");
                }

                // Validar rango de fechas razonable (máximo 1 año)
                var diferencia = fechaFin - fechaInicio;
                if (diferencia.TotalDays > 365)
                {
                    throw new ArgumentException("El rango de fechas no puede ser mayor a 1 año.");
                }

                var resultado = _reporteDAL.ObtenerVentasDetalladasPorFecha(fechaInicio, fechaFin);

                // Asegurar que nunca retornemos null
                return resultado ?? new List<VentaDetalladaReporte>();
            }
            catch (ArgumentException)
            {
                throw; // Re-lanzar errores de validación
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar reporte de ventas: {ex.Message}", ex);
            }
        }

        // ============================================================
        // 2. GENERAR CORTE DE CAJA DIARIO
        // ============================================================
        public CorteCaja GenerarCorteCajaDiario(DateTime fecha)
        {
            try
            {
                // Validar que la fecha no sea futura
                if (fecha.Date > DateTime.Now.Date)
                {
                    throw new ArgumentException("No se puede generar un corte de caja para fechas futuras.");
                }

                var resultado = _reporteDAL.ObtenerCorteCajaPorFecha(fecha);

                // Validar resultado
                if (resultado == null)
                {
                    resultado = new CorteCaja
                    {
                        Fecha = fecha.Date,
                        Ventas = new List<Venta>(),
                        TotalDia = 0,
                        TotalEfectivo = 0,
                        TotalTarjeta = 0
                    };
                }

                return resultado;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar corte de caja: {ex.Message}", ex);
            }
        }

        // ============================================================
        // 3. REGISTRAR CORTE DE CAJA
        // ============================================================
        public bool RegistrarCorteCaja(HistorialCorte corte)
        {
            try
            {
                // Validaciones
                if (corte == null)
                {
                    throw new ArgumentNullException(nameof(corte), "El objeto corte no puede ser nulo.");
                }

                if (corte.IdEmpleado <= 0)
                {
                    throw new ArgumentException("El ID de empleado no es válido.");
                }

                if (string.IsNullOrWhiteSpace(corte.NombreEmpleado))
                {
                    throw new ArgumentException("El nombre del empleado es requerido.");
                }

                if (corte.TotalSistema < 0 || corte.TotalReal < 0)
                {
                    throw new ArgumentException("Los montos no pueden ser negativos.");
                }

                return _reporteDAL.GuardarCorte(corte);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al registrar corte de caja: {ex.Message}", ex);
            }
        }

        // ============================================================
        // 4. OBTENER HISTORIAL DE CORTES
        // ============================================================
        public List<HistorialCorte> ObtenerHistorialCortesPorRango(DateTime desde, DateTime hasta)
        {
            try
            {
                // Validar fechas
                if (desde > hasta)
                {
                    throw new ArgumentException("La fecha 'desde' no puede ser mayor que 'hasta'.");
                }

                var resultado = _reporteDAL.ObtenerHistorialCortes(desde, hasta);

                // Asegurar que nunca retornemos null
                return resultado ?? new List<HistorialCorte>();
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar historial de cortes: {ex.Message}", ex);
            }
        }

        // ============================================================
        // 5. EXPORTAR CSV
        // ============================================================
        public void ExportarVentasDetalladasCSV(List<VentaDetalladaReporte> data, string filePath)
        {
            try
            {
                // Validaciones
                if (data == null || data.Count == 0)
                {
                    throw new ArgumentException("No hay datos para exportar.");
                }

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new ArgumentException("La ruta del archivo no es válida.");
                }

                // Verificar que el directorio exista
                string directorio = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directorio))
                {
                    Directory.CreateDirectory(directorio);
                }

                var sb = new StringBuilder();

                // Encabezado
                sb.AppendLine("VentaID,FechaVenta,NombreEmpleado,NombreProducto,Cantidad,PrecioUnitario,Subtotal,TotalVenta,MetodoPago");

                // Datos - Manejar valores NULL de forma segura
                foreach (var item in data)
                {
                    sb.AppendLine(string.Format(
                        "{0},{1:g},\"{2}\",\"{3}\",{4},{5:F2},{6:F2},{7:F2},\"{8}\"",
                        item.VentaID,
                        item.FechaVenta,
                        EscaparCSV(item.NombreEmpleado ?? "N/A"),
                        EscaparCSV(item.NombreProducto ?? "N/A"),
                        item.Cantidad,
                        item.PrecioUnitario,
                        item.Subtotal,
                        item.TotalVenta,
                        EscaparCSV(item.MetodoPago ?? "N/A")
                    ));
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (IOException ex)
            {
                throw new Exception($"Error de acceso al archivo: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar archivo CSV: {ex.Message}", ex);
            }
        }

        // ============================================================
        // 6. EXPORTAR PDF DE CORTE DE CAJA
        // ============================================================
        public void ExportarCorteCajaPDF(CorteCaja corte, HistorialCorte datosArqueo, string filePath)
        {
            // Validaciones iniciales
            if (corte == null)
            {
                throw new ArgumentNullException(nameof(corte), "El objeto CorteCaja no puede ser nulo.");
            }

            if (datosArqueo == null)
            {
                throw new ArgumentNullException(nameof(datosArqueo), "El objeto HistorialCorte no puede ser nulo.");
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("La ruta del archivo no es válida.", nameof(filePath));
            }

            // Verificar que el directorio exista
            string directorio = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            Document doc = null;

            try
            {
                doc = new Document(PageSize.A4, 50, 50, 25, 25);
                PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                // Fuentes
                var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var fontSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.DARK_GRAY);
                var fontNegrita = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                var fontRojo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.RED);
                var fontVerde = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, new BaseColor(0, 100, 0));

                // ======= ENCABEZADO =======
                var parrafoTitulo = new Paragraph("REPORTE DE CIERRE DE CAJA", fontTitulo)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                };
                doc.Add(parrafoTitulo);

                var parrafoFecha = new Paragraph($"Generado el: {DateTime.Now:g}", fontSubtitulo)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 5
                };
                doc.Add(parrafoFecha);

                doc.Add(new Paragraph("─────────────────────────────────────────", fontNormal)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 15
                });

                // ======= INFORMACIÓN DEL CAJERO =======
                var tablaInfo = new PdfPTable(2) { WidthPercentage = 100, SpacingAfter = 15 };

                var celdaCajero = new PdfPCell(new Phrase($"Cajero: {datosArqueo.NombreEmpleado ?? "No Especificado"}", fontNegrita))
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingBottom = 5
                };
                tablaInfo.AddCell(celdaCajero);

                var celdaFecha = new PdfPCell(new Phrase($"Fecha Corte: {corte.Fecha:D}", fontNegrita))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    PaddingBottom = 5
                };
                tablaInfo.AddCell(celdaFecha);

                doc.Add(tablaInfo);

                // ======= RESUMEN DEL SISTEMA =======
                var tableResumen = new PdfPTable(2)
                {
                    WidthPercentage = 70,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    SpacingAfter = 15
                };

                // Encabezado de la tabla
                var celdaEncResumen = new PdfPCell(new Phrase("RESUMEN DEL SISTEMA", fontNegrita))
                {
                    Colspan = 2,
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 8
                };
                tableResumen.AddCell(celdaEncResumen);

                // Datos del resumen
                AgregarFilaTabla(tableResumen, "Ventas Efectivo:", corte.TotalEfectivo.ToString("C2"), fontNormal);
                AgregarFilaTabla(tableResumen, "Ventas Tarjeta:", corte.TotalTarjeta.ToString("C2"), fontNormal);

                var celdaTotEsp1 = new PdfPCell(new Phrase("TOTAL ESPERADO:", fontNegrita)) { Padding = 8 };
                var celdaTotEsp2 = new PdfPCell(new Phrase(corte.TotalDia.ToString("C2"), fontNegrita))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 8
                };
                tableResumen.AddCell(celdaTotEsp1);
                tableResumen.AddCell(celdaTotEsp2);

                doc.Add(tableResumen);

                // ======= ARQUEO FÍSICO =======
                var tableArqueo = new PdfPTable(2)
                {
                    WidthPercentage = 70,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    SpacingAfter = 15
                };

                var celdaEncArqueo = new PdfPCell(new Phrase("ARQUEO DE CAJA (FÍSICO)", fontNegrita))
                {
                    Colspan = 2,
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 8
                };
                tableArqueo.AddCell(celdaEncArqueo);

                AgregarFilaTabla(tableArqueo, "Dinero Contado por Cajero:", datosArqueo.TotalReal.ToString("C2"), fontNormal);

                // Diferencia con color
                var celdaDifLabel = new PdfPCell(new Phrase("DIFERENCIA:", fontNegrita)) { Padding = 8 };
                var fontDiferencia = datosArqueo.Diferencia < 0 ? fontRojo : fontVerde;
                var celdaDifValor = new PdfPCell(new Phrase(datosArqueo.Diferencia.ToString("C2"), fontDiferencia))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 8
                };
                tableArqueo.AddCell(celdaDifLabel);
                tableArqueo.AddCell(celdaDifValor);

                doc.Add(tableArqueo);

                // ======= COMENTARIOS =======
                if (!string.IsNullOrWhiteSpace(datosArqueo.Comentarios))
                {
                    var parrafoComentarios = new Paragraph($"Comentarios: {datosArqueo.Comentarios}", fontNormal)
                    {
                        SpacingAfter = 20
                    };
                    doc.Add(parrafoComentarios);
                }

                // ======= FIRMAS =======
                doc.Add(Chunk.NEWLINE);
                doc.Add(Chunk.NEWLINE);

                var tablaFirmas = new PdfPTable(2)
                {
                    WidthPercentage = 80,
                    SpacingBefore = 30
                };

                var celdaFirmaCajero = new PdfPCell(new Phrase("_________________________\n\nFirma Cajero", fontNormal))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingTop = 20
                };
                tablaFirmas.AddCell(celdaFirmaCajero);

                var celdaFirmaSupervisor = new PdfPCell(new Phrase("_________________________\n\nFirma Supervisor", fontNormal))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingTop = 20
                };
                tablaFirmas.AddCell(celdaFirmaSupervisor);

                doc.Add(tablaFirmas);
            }
            catch (IOException ex)
            {
                throw new Exception($"Error de acceso al archivo PDF: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar el PDF: {ex.Message}", ex);
            }
            finally
            {
                if (doc != null && doc.IsOpen())
                {
                    try
                    {
                        doc.Close();
                    }
                    catch
                    {
                        // Ignorar errores al cerrar
                    }
                }
            }
        }

        // ============================================================
        // MÉTODOS AUXILIARES PRIVADOS
        // ============================================================

        /// <summary>
        /// Escapa caracteres especiales para CSV
        /// </summary>
        private string EscaparCSV(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return "";

            // Reemplazar comillas dobles por dos comillas dobles
            return valor.Replace("\"", "\"\"");
        }

        /// <summary>
        /// Agrega una fila a una tabla PDF
        /// </summary>
        private void AgregarFilaTabla(PdfPTable tabla, string etiqueta, string valor, Font fuente)
        {
            var celdaEtiqueta = new PdfPCell(new Phrase(etiqueta, fuente)) { Padding = 8 };
            var celdaValor = new PdfPCell(new Phrase(valor, fuente))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Padding = 8
            };
            tabla.AddCell(celdaEtiqueta);
            tabla.AddCell(celdaValor);
        }
    }
}