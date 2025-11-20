using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using KIOSKO_Proyecto.Modelos;
using KIOSKO_Proyecto.Datos;

namespace KIOSKO_Proyecto.Datos
{
    public class ReporteDAL
    {
        public List<VentaDetalladaReporte> ObtenerVentasDetalladasPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            var list = new List<VentaDetalladaReporte>();
            string query = @"
                SELECT
                    V.ID_VENTA, V.FECHA, E.NOMBRE_EMP, P.NOMBRE,
                    DV.CANTIDAD, DV.PRECIO_UNITARIO, DV.SUBTOTAL,
                    V.TOTAL, V.METODO_PAGO
                FROM VENTA V
                JOIN DETALLE_VENTA DV ON V.ID_VENTA = DV.ID_VENTA
                JOIN PRODUCTO P ON DV.ID_PRODUCTO = P.ID_PRODUCTO
                JOIN EMPLEADO E ON V.ID_EMPLEADO = E.ID_EMPLEADO
                WHERE V.FECHA BETWEEN @fechaInicio AND @fechaFin
                ORDER BY V.FECHA, V.ID_VENTA;";

            using (var conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@fechaFin", fechaFin);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new VentaDetalladaReporte
                            {
                                VentaID = reader.GetInt32(0),
                                FechaVenta = reader.GetDateTime(1),
                                NombreEmpleado = reader.GetString(2),
                                NombreProducto = reader.GetString(3),
                                Cantidad = reader.GetInt32(4),
                                PrecioUnitario = reader.GetDecimal(5),
                                Subtotal = reader.GetDecimal(6),
                                TotalVenta = reader.GetDecimal(7),
                                MetodoPago = reader.GetString(8)
                            });
                        }
                    }
                }
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
            return list;
        }

        public CorteCaja ObtenerCorteCajaPorFecha(DateTime fecha)
        {
            var corte = new CorteCaja { Fecha = fecha.Date };
            string query = @"
                SELECT
                    V.ID_VENTA, V.FECHA, V.TOTAL,
                    V.MontoEfectivo, V.MontoTarjeta, E.NOMBRE_EMP
                FROM VENTA V
                JOIN EMPLEADO E ON V.ID_EMPLEADO = E.ID_EMPLEADO
                WHERE CAST(V.FECHA AS DATE) = @fecha;";

            using (var conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@fecha", fecha.Date);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            corte.Ventas.Add(new Venta
                            {
                                VentaID = reader.GetInt32(0),
                                FechaVenta = reader.GetDateTime(1),
                                TotalVenta = reader.GetDecimal(2),
                                MontoEfectivo = reader.IsDBNull(3) ? (decimal?)null : reader.GetDecimal(3),
                                MontoTarjeta = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4),
                                NombreEmpleado = reader.GetString(5)
                            });
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

            corte.TotalDia = corte.Ventas.Sum(v => v.TotalVenta);
            corte.TotalEfectivo = corte.Ventas.Sum(v => v.MontoEfectivo ?? 0);
            corte.TotalTarjeta = corte.Ventas.Sum(v => v.MontoTarjeta ?? 0);

            return corte;
        }
    }
}