using KIOSKO_Proyecto.Modelos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;


namespace KIOSKO_Proyecto.Datos
{
    public class ReporteDAL
    {
        // 1. Obtener lista para reporte detallado (CSV)
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
            }
            return list;
        }

        // 2. Obtener datos calculados del sistema (para pre-corte)
        public CorteCaja ObtenerCorteCajaPorFecha(DateTime fecha)
        {
            var corte = new CorteCaja 
            { 
                Fecha = fecha.Date,
                Ventas = new List<Venta>() 
            };

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
                    }
                }
            }

            if (corte.Ventas.Any())
            {
                corte.TotalDia = corte.Ventas.Sum(v => v.TotalVenta);
                corte.TotalEfectivo = corte.Ventas.Sum(v => v.MontoEfectivo ?? 0);
                corte.TotalTarjeta = corte.Ventas.Sum(v => v.MontoTarjeta ?? 0);
            }

            return corte;
        }

        // 3. NUEVO: Guardar el Arqueo (Corte Final) en la Base de Datos
        public bool GuardarCorte(HistorialCorte corte)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                string query = @"
                    INSERT INTO HISTORIAL_CORTES 
                    (ID_EMPLEADO, FECHA_CORTE, TOTAL_SISTEMA, TOTAL_REAL, DIFERENCIA, TOTAL_EFECTIVO, TOTAL_TARJETA, COMENTARIOS)
                    VALUES 
                    (@IdEmp, @Fecha, @TotSis, @TotReal, @Dif, @TotEfe, @TotTar, @Com)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdEmp", corte.IdEmpleado);
                    cmd.Parameters.AddWithValue("@Fecha", corte.FechaCorte);
                    cmd.Parameters.AddWithValue("@TotSis", corte.TotalSistema);
                    cmd.Parameters.AddWithValue("@TotReal", corte.TotalReal);
                    cmd.Parameters.AddWithValue("@Dif", corte.Diferencia);
                    cmd.Parameters.AddWithValue("@TotEfe", corte.TotalEfectivo);
                    cmd.Parameters.AddWithValue("@TotTar", corte.TotalTarjeta);
                    cmd.Parameters.AddWithValue("@Com", (object)corte.Comentarios ?? DBNull.Value);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}