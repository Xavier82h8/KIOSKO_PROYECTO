using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using KIOSKO_Proyecto.Datos;
using KIOSKO_Proyecto.Modelos;

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
    }
}
