using System;
using System.Data;
using System.Data.SqlClient;
using KIOSKO_Proyecto.Datos;

namespace KIOSKO_Proyecto.Datos
{
    public class ReporteDAL
    {
        // Ventas detalladas entre fechas
        public DataTable ObtenerVentasDetalladas(DateTime desde, DateTime hasta)
        {
            string query = @"
                SELECT 
                    v.VentaID,
                    v.FechaVenta,
                    e.Nombre + ' ' + e.Apellido AS NombreEmpleado,
                    p.NombreProducto,
                    dv.Cantidad,
                    dv.PrecioUnitario,
                    dv.Subtotal
                FROM Ventas v
                INNER JOIN DetalleVenta dv ON v.VentaID = dv.VentaID
                INNER JOIN Producto p ON dv.ProductoID = p.ProductoID
                INNER JOIN Empleado e ON v.EmpleadoID = e.EmpleadoID
                WHERE CAST(v.FechaVenta AS DATE) BETWEEN @desde AND @hasta
                ORDER BY v.FechaVenta DESC";

            using (SqlConnection con = Conexion.ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@desde", desde.Date);
                cmd.Parameters.AddWithValue("@hasta", hasta.Date);

                DataTable dt = new DataTable();
                con.Open();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }

        // Corte de caja de un día
        public DataTable ObtenerCorteCajaDiario(DateTime fecha)
        {
            string query = @"
                SELECT 
                    v.VentaID,
                    v.FechaVenta,
                    e.Nombre + ' ' + e.Apellido AS Cajero,
                    v.Total,
                    v.MetodoPago
                FROM Ventas v
                INNER JOIN Empleado e ON v.EmpleadoID = e.EmpleadoID
                WHERE CAST(v.FechaVenta AS DATE) = @fecha
                ORDER BY v.FechaVenta";

            using (SqlConnection con = Conexion.ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@fecha", fecha.Date);

                DataTable dt = new DataTable();
                con.Open();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }

        // Totales del día
        public (decimal totalEfectivo, decimal totalTarjeta, decimal granTotal) ObtenerTotalesDia(DateTime fecha)
        {
            string query = @"
                SELECT 
                    SUM(CASE WHEN MetodoPago = 'Efectivo' THEN Total ELSE 0 END) AS Efectivo,
                    SUM(CASE WHEN MetodoPago = 'Tarjeta'  THEN Total ELSE 0 END) AS Tarjeta,
                    SUM(Total) AS GranTotal
                FROM Ventas
                WHERE CAST(FechaVenta AS DATE) = @fecha";

            using (SqlConnection con = Conexion.ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@fecha", fecha.Date);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        decimal efectivo = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                        decimal tarjeta = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                        decimal total = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                        return (efectivo, tarjeta, total);
                    }
                    return (0, 0, 0);
                }
            }
        }
    }
}