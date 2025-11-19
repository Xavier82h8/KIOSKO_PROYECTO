using System;
using System.Data;
using KIOSKO_Proyecto.Datos;
using KIOSKO_Proyecto.Modelos;

namespace KIOSKO_Proyecto.BLL
{
    public class ReporteBLL
    {
        private readonly ReporteDAL _reporteDAL = new ReporteDAL();

        // Reporte de Ventas Detalladas
        public DataTable ObtenerVentasDetalladas(DateTime desde, DateTime hasta)
        {
            try
            {
                return _reporteDAL.ObtenerVentasDetalladas(desde, hasta);
            }
            catch (Exception ex)
            {
                throw new Exception("Error BLL - Ventas detalladas: " + ex.Message);
            }
        }

        // Corte de Caja Diario
        public DataTable ObtenerCorteCajaDiario(DateTime fecha)
        {
            try
            {
                return _reporteDAL.ObtenerCorteCajaDiario(fecha);
            }
            catch (Exception ex)
            {
                throw new Exception("Error BLL - Corte de caja: " + ex.Message);
            }
        }

        // Totales del día (efectivo + tarjeta)
        public (decimal totalEfectivo, decimal totalTarjeta, decimal granTotal) ObtenerTotalesDia(DateTime fecha)
        {
            try
            {
                return _reporteDAL.ObtenerTotalesDia(fecha);
            }
            catch (Exception ex)
            {
                throw new Exception("Error BLL - Totales: " + ex.Message);
            }
        }
    }
}