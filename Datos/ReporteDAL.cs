using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using KIOSKO_Proyecto.Modelos;

namespace KIOSKO_Proyecto.Datos
{
    public class ReporteDAL
    {
        public void GuardarReporte(Reporte reporte)
        {
            using (SqlConnection connection = Conexion.ObtenerConexion())
            {
                string query = "INSERT INTO Reportes (FechaGeneracion, FechaInicio, FechaFin, TotalVentas, GeneradoPorEmpleadoId) " +
                               "VALUES (@FechaGeneracion, @FechaInicio, @FechaFin, @TotalVentas, @GeneradoPorEmpleadoId)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FechaGeneracion", reporte.FechaGeneracion);
                    command.Parameters.AddWithValue("@FechaInicio", reporte.FechaInicio);
                    command.Parameters.AddWithValue("@FechaFin", reporte.FechaFin);
                    command.Parameters.AddWithValue("@TotalVentas", reporte.TotalVentas);
                    command.Parameters.AddWithValue("@GeneradoPorEmpleadoId", reporte.GeneradoPorEmpleadoId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Reporte> ObtenerTodosLosReportes()
        {
            List<Reporte> reportes = new List<Reporte>();
            using (SqlConnection connection = Conexion.ObtenerConexion())
            {
                connection.Open();
                string query = @"
            SELECT 
                R.IdReporte,
                R.FechaGeneracion,
                R.FechaInicio,
                R.FechaFin,
                R.TotalVentas,
                R.GeneradoPorEmpleadoId,
                E.NOMBRE_EMP AS NombreEmpleadoGenerador
            FROM Reportes R
            JOIN EMPLEADO E ON R.GeneradoPorEmpleadoId = E.ID_EMPLEADO";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var reporte = new Reporte
                        {
                            IdReporte = Convert.ToInt32(reader["IdReporte"]),
                            FechaGeneracion = Convert.ToDateTime(reader["FechaGeneracion"]),
                            FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                            FechaFin = Convert.ToDateTime(reader["FechaFin"]),
                            TotalVentas = Convert.ToDecimal(reader["TotalVentas"]),
                            GeneradoPorEmpleadoId = Convert.ToInt32(reader["GeneradoPorEmpleadoId"]),
                            NombreEmpleadoGenerador = reader["NombreEmpleadoGenerador"].ToString()
                        };
                        reportes.Add(reporte);
                    }
                }
            }
            return reportes;
        }

        public List<VentaDetalladaReporte> ObtenerVentasDetalladasPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            var ventas = new List<VentaDetalladaReporte>();
            using (var conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                string query = @"
                    SELECT
                        v.ID_VENTA,
                        v.FECHA,
                        p.NOMBRE AS NombreProducto,
                        dv.CANTIDAD,
                        dv.PRECIO_UNITARIO,
                        dv.SUBTOTAL,
                        v.METODO_PAGO,
                        e.NOMBRE_EMP AS NombreEmpleado
                    FROM VENTA v
                    JOIN DETALLE_VENTA dv ON v.ID_VENTA = dv.ID_VENTA
                    JOIN PRODUCTO p ON dv.ID_PRODUCTO = p.ID_PRODUCTO
                    JOIN EMPLEADO e ON v.ID_EMPLEADO = e.ID_EMPLEADO
                    WHERE v.FECHA BETWEEN @FechaInicio AND @FechaFin
                    ORDER BY v.FECHA, v.ID_VENTA;
                ";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ventas.Add(new VentaDetalladaReporte
                            {
                                VentaID = Convert.ToInt32(reader["ID_VENTA"]),
                                FechaVenta = Convert.ToDateTime(reader["FECHA"]),
                                NombreProducto = reader["NombreProducto"].ToString(),
                                Cantidad = Convert.ToInt32(reader["CANTIDAD"]),
                                PrecioUnitario = Convert.ToDecimal(reader["PRECIO_UNITARIO"]),
                                Subtotal = Convert.ToDecimal(reader["SUBTOTAL"]),
                                MetodoPago = reader["METODO_PAGO"].ToString(),
                                NombreEmpleado = reader["NombreEmpleado"].ToString()
                            });
                        }
                    }
                }
            }
            return ventas;
        }

        public Tuple<decimal, decimal> ObtenerTotalesCorteCaja(DateTime fecha)
        {
            decimal totalEfectivo = 0;
            decimal totalTarjeta = 0;

            using (var conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                string query = @"
                    SELECT
                        SUM(ISNULL(MontoEfectivo, 0)) AS TotalEfectivo,
                        SUM(ISNULL(MontoTarjeta, 0)) AS TotalTarjeta
                    FROM VENTA
                    WHERE FECHA = @Fecha;
                ";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Fecha", fecha.Date);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            totalEfectivo = reader["TotalEfectivo"] != DBNull.Value ? Convert.ToDecimal(reader["TotalEfectivo"]) : 0;
                            totalTarjeta = reader["TotalTarjeta"] != DBNull.Value ? Convert.ToDecimal(reader["TotalTarjeta"]) : 0;
                        }
                    }
                }
            }
            return Tuple.Create(totalEfectivo, totalTarjeta);
        }
    }
}
