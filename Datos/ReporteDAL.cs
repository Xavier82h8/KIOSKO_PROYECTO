using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using KIOSKO_Proyecto.Modelos;

namespace KIOSKO_Proyecto.Datos
{
    public class ReporteDAL
    {
        // ============================================================
        // 1. REPORTE DETALLADO DE VENTAS
        // Manejo COMPLETO de valores NULL en TODOS los campos
        // ============================================================
        public List<VentaDetalladaReporte> ObtenerVentasDetalladasPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            var list = new List<VentaDetalladaReporte>();

            try
            {
                using (var conn = Conexion.ObtenerConexion())
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            V.ID_VENTA, 
                            V.FECHA, 
                            E.NOMBRE_EMP, 
                            P.NOMBRE AS NombreProducto,
                            D.CANTIDAD, 
                            D.PRECIO_UNITARIO, 
                            D.SUBTOTAL, 
                            V.TOTAL, 
                            V.METODO_PAGO,
                            V.MontoEfectivo,
                            V.MontoTarjeta
                        FROM VENTA V
                        INNER JOIN DETALLE_VENTA D ON V.ID_VENTA = D.ID_VENTA
                        INNER JOIN PRODUCTO P ON D.ID_PRODUCTO = P.ID_PRODUCTO
                        INNER JOIN EMPLEADO E ON V.ID_EMPLEADO = E.ID_EMPLEADO
                        WHERE V.FECHA BETWEEN @fInicio AND @fFin
                        ORDER BY V.FECHA DESC, V.ID_VENTA DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fInicio", fechaInicio);
                        cmd.Parameters.AddWithValue("@fFin", fechaFin);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // ============================================================
                                // MANEJO ROBUSTO: Crear objeto paso a paso
                                // Verificar CADA campo antes de asignarlo
                                // ============================================================
                                var venta = new VentaDetalladaReporte();

                                // --- CAMPOS OBLIGATORIOS (INT/DECIMAL/DATETIME) ---
                                venta.VentaID = reader.GetInt32(reader.GetOrdinal("ID_VENTA"));
                                venta.FechaVenta = reader.GetDateTime(reader.GetOrdinal("FECHA"));
                                venta.Cantidad = reader.GetInt32(reader.GetOrdinal("CANTIDAD"));
                                venta.PrecioUnitario = reader.GetDecimal(reader.GetOrdinal("PRECIO_UNITARIO"));
                                venta.Subtotal = reader.GetDecimal(reader.GetOrdinal("SUBTOTAL"));
                                venta.TotalVenta = reader.GetDecimal(reader.GetOrdinal("TOTAL"));

                                // --- CAMPOS STRING (PUEDEN SER NULL) ---
                                int idxEmpleado = reader.GetOrdinal("NOMBRE_EMP");
                                venta.NombreEmpleado = reader.IsDBNull(idxEmpleado)
                                    ? "Sin Asignar"
                                    : reader.GetString(idxEmpleado);

                                int idxProducto = reader.GetOrdinal("NombreProducto");
                                venta.NombreProducto = reader.IsDBNull(idxProducto)
                                    ? "Sin Nombre"
                                    : reader.GetString(idxProducto);

                                int idxMetodo = reader.GetOrdinal("METODO_PAGO");
                                venta.MetodoPago = reader.IsDBNull(idxMetodo)
                                    ? "No Especificado"
                                    : reader.GetString(idxMetodo);

                                // --- CAMPOS DECIMALES NULLABLE ---
                                int idxEfectivo = reader.GetOrdinal("MontoEfectivo");
                                venta.MontoEfectivo = reader.IsDBNull(idxEfectivo)
                                    ? (decimal?)null
                                    : reader.GetDecimal(idxEfectivo);

                                int idxTarjeta = reader.GetOrdinal("MontoTarjeta");
                                venta.MontoTarjeta = reader.IsDBNull(idxTarjeta)
                                    ? (decimal?)null
                                    : reader.GetDecimal(idxTarjeta);

                                list.Add(venta);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error de base de datos al obtener ventas: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener ventas detalladas: {ex.Message}", ex);
            }

            return list;
        }

        // ============================================================
        // 2. CORTE DE CAJA DIARIO
        // ============================================================
        public CorteCaja ObtenerCorteCajaPorFecha(DateTime fecha)
        {
            var corte = new CorteCaja
            {
                Fecha = fecha.Date,
                Ventas = new List<Venta>(),
                TotalDia = 0,
                TotalEfectivo = 0,
                TotalTarjeta = 0
            };

            try
            {
                using (var conn = Conexion.ObtenerConexion())
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            V.ID_VENTA, 
                            V.FECHA, 
                            V.TOTAL, 
                            V.MontoEfectivo, 
                            V.MontoTarjeta, 
                            E.NOMBRE_EMP
                        FROM VENTA V
                        INNER JOIN EMPLEADO E ON V.ID_EMPLEADO = E.ID_EMPLEADO
                        WHERE CAST(V.FECHA AS DATE) = @fecha
                        ORDER BY V.FECHA DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fecha", fecha.Date);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var v = new Venta
                                {
                                    VentaID = reader.GetInt32(0),
                                    FechaVenta = reader.GetDateTime(1),
                                    TotalVenta = reader.GetDecimal(2),

                                    // Manejar NULL en MontoEfectivo
                                    MontoEfectivo = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),

                                    // Manejar NULL en MontoTarjeta
                                    MontoTarjeta = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),

                                    // Manejar NULL en NombreEmpleado
                                    NombreEmpleado = reader.IsDBNull(5) ? "Sin Asignar" : reader.GetString(5)
                                };
                                corte.Ventas.Add(v);
                            }
                        }
                    }
                }

                // Calcular totales en memoria de forma segura
                foreach (var v in corte.Ventas)
                {
                    corte.TotalDia += v.TotalVenta;
                    corte.TotalEfectivo += v.MontoEfectivo ?? 0m;
                    corte.TotalTarjeta += v.MontoTarjeta ?? 0m;
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error de base de datos al obtener corte de caja: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener corte de caja: {ex.Message}", ex);
            }

            return corte;
        }

        // ============================================================
        // 3. GUARDAR HISTORIAL DE CORTE
        // ============================================================
        public bool GuardarCorte(HistorialCorte corte)
        {
            if (corte == null)
            {
                throw new ArgumentNullException(nameof(corte), "El objeto HistorialCorte no puede ser nulo");
            }

            try
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

                        // Manejar NULL en comentarios
                        cmd.Parameters.AddWithValue("@Com",
                            string.IsNullOrWhiteSpace(corte.Comentarios)
                                ? (object)DBNull.Value
                                : corte.Comentarios);

                        int filasAfectadas = cmd.ExecuteNonQuery();
                        return filasAfectadas > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error de base de datos al guardar corte: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar el corte: {ex.Message}", ex);
            }
        }

        // ============================================================
        // 4. OBTENER HISTORIAL DE CORTES
        // ============================================================
        public List<HistorialCorte> ObtenerHistorialCortes(DateTime desde, DateTime hasta)
        {
            var lista = new List<HistorialCorte>();

            try
            {
                using (var conn = Conexion.ObtenerConexion())
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            H.ID_CORTE, 
                            H.ID_EMPLEADO, 
                            E.NOMBRE_EMP, 
                            H.FECHA_CORTE,
                            H.TOTAL_SISTEMA, 
                            H.TOTAL_REAL, 
                            H.DIFERENCIA,
                            H.TOTAL_EFECTIVO, 
                            H.TOTAL_TARJETA, 
                            H.COMENTARIOS
                        FROM HISTORIAL_CORTES H
                        INNER JOIN EMPLEADO E ON H.ID_EMPLEADO = E.ID_EMPLEADO
                        WHERE CAST(H.FECHA_CORTE AS DATE) BETWEEN @desde AND @hasta
                        ORDER BY H.FECHA_CORTE DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@desde", desde.Date);
                        cmd.Parameters.AddWithValue("@hasta", hasta.Date);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var corte = new HistorialCorte
                                {
                                    IdCorte = reader.GetInt32(0),
                                    IdEmpleado = reader.GetInt32(1),

                                    // Manejar NULL en nombre de empleado
                                    NombreEmpleado = reader.IsDBNull(2)
                                        ? "Sin Asignar"
                                        : reader.GetString(2),

                                    FechaCorte = reader.GetDateTime(3),
                                    TotalSistema = reader.GetDecimal(4),
                                    TotalReal = reader.GetDecimal(5),
                                    Diferencia = reader.GetDecimal(6),
                                    TotalEfectivo = reader.GetDecimal(7),
                                    TotalTarjeta = reader.GetDecimal(8),

                                    // Manejar NULL en comentarios
                                    Comentarios = reader.IsDBNull(9)
                                        ? ""
                                        : reader.GetString(9)
                                };

                                lista.Add(corte);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error de base de datos al obtener historial: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener historial de cortes: {ex.Message}", ex);
            }

            return lista;
        }
    }
}