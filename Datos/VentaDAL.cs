using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using KIOSKO_Proyecto.Modelos;

namespace KIOSKO_Proyecto.Datos
{
    public class VentaDAL
    {
        /// <summary>
        /// Guarda una venta completa, sus detalles y actualiza el stock en una transacción.
        /// </summary>
        /// <param name="venta">Objeto Venta con todos los datos a guardar.</param>
        /// <returns>El objeto Venta con su ID asignado si la transacción tuvo éxito; de lo contrario, null.</returns>
        public Venta CrearVenta(Venta venta)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Insertar la venta principal y obtener el ID
                        string queryVenta = @"
                            INSERT INTO VENTA (FECHA, HORA, ID_EMPLEADO, TOTAL, MontoEfectivo, MontoTarjeta, Cambio, METODO_PAGO) 
                            OUTPUT INSERTED.ID_VENTA 
                            VALUES (@Fecha, @Hora, @IdEmpleado, @Total, @MontoEfectivo, @MontoTarjeta, @Cambio, @MetodoPago)";

                        int ventaId;
                        using (var ventaCmd = new SqlCommand(queryVenta, conn, transaction))
                        {
                            ventaCmd.Parameters.AddWithValue("@Fecha", venta.FechaVenta.Date);
                            ventaCmd.Parameters.AddWithValue("@Hora", venta.FechaVenta.TimeOfDay);
                            ventaCmd.Parameters.AddWithValue("@IdEmpleado", venta.EmpleadoID);
                            ventaCmd.Parameters.AddWithValue("@Total", venta.TotalVenta);
                            ventaCmd.Parameters.AddWithValue("@MontoEfectivo", (object)venta.MontoEfectivo ?? DBNull.Value);
                            ventaCmd.Parameters.AddWithValue("@MontoTarjeta", (object)venta.MontoTarjeta ?? DBNull.Value);
                            ventaCmd.Parameters.AddWithValue("@Cambio", (object)venta.Cambio ?? DBNull.Value);
                            ventaCmd.Parameters.AddWithValue("@MetodoPago", (object)venta.MetodoPago ?? DBNull.Value);

                            ventaId = (int)ventaCmd.ExecuteScalar();
                            venta.VentaID = ventaId; // Asignar el nuevo ID al objeto
                        }

                        // 2. Insertar los detalles de la venta
                        foreach (var detalle in venta.Detalles)
                        {
                            string queryDetalle = @"
                                INSERT INTO DETALLE_VENTA (ID_VENTA, ID_PRODUCTO, CANTIDAD, PRECIO_UNITARIO, SUBTOTAL) 
                                VALUES (@IdVenta, @IdProducto, @Cantidad, @PrecioUnitario, @Subtotal)";

                            using (var detalleCmd = new SqlCommand(queryDetalle, conn, transaction))
                            {
                                detalleCmd.Parameters.AddWithValue("@IdVenta", ventaId);
                                detalleCmd.Parameters.AddWithValue("@IdProducto", detalle.ProductoID);
                                detalleCmd.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
                                detalleCmd.Parameters.AddWithValue("@PrecioUnitario", detalle.PrecioUnitario);
                                detalleCmd.Parameters.AddWithValue("@Subtotal", detalle.Subtotal);
                                detalleCmd.ExecuteNonQuery();
                            }
                        }

                        // 3. Actualizar el stock de cada producto
                        foreach (var detalle in venta.Detalles)
                        {
                            string queryStock = @"
                                UPDATE PRODUCTO SET CANTIDAD_DISPONIBLE = CANTIDAD_DISPONIBLE - @Cantidad 
                                WHERE ID_PRODUCTO = @IdProducto";

                            using (var stockCmd = new SqlCommand(queryStock, conn, transaction))
                            {
                                stockCmd.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
                                stockCmd.Parameters.AddWithValue("@IdProducto", detalle.ProductoID);
                                stockCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return venta; // Devolver el objeto Venta completo
                    }
                    catch (Exception ex)
                    {
                        // Si algo falla, revertir toda la transacción
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"Error al crear la venta: {ex.Message}");
                        return null; // Devolver null en caso de error
                    }
                }
            }
        }

        // Se mantienen los métodos de obtención de datos, ajustando los nombres de propiedades si es necesario.
        public List<Venta> ObtenerVentasPorRango(DateTime desde, DateTime hasta)
        {
            var lista = new List<Venta>();
            using (var conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                string query = @"
                    SELECT V.ID_VENTA, V.FECHA, V.HORA, V.ID_EMPLEADO, V.TOTAL, V.MontoEfectivo, V.MontoTarjeta, V.Cambio, V.METODO_PAGO
                    FROM VENTA V
                    WHERE V.FECHA >= @desde AND V.FECHA <= @hasta 
                    ORDER BY V.FECHA DESC, V.HORA DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@desde", desde.Date);
                    cmd.Parameters.AddWithValue("@hasta", hasta.Date);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var v = new Venta
                            {
                                VentaID = reader.GetInt32(reader.GetOrdinal("ID_VENTA")),
                                FechaVenta = reader.GetDateTime(reader.GetOrdinal("FECHA")).Add((TimeSpan)reader.GetValue(reader.GetOrdinal("HORA"))),
                                EmpleadoID = reader.GetInt32(reader.GetOrdinal("ID_EMPLEADO")),
                                TotalVenta = reader.GetDecimal(reader.GetOrdinal("TOTAL")),
                                MontoEfectivo = reader.IsDBNull(reader.GetOrdinal("MontoEfectivo")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("MontoEfectivo")),
                                MontoTarjeta = reader.IsDBNull(reader.GetOrdinal("MontoTarjeta")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("MontoTarjeta")),
                                Cambio = reader.IsDBNull(reader.GetOrdinal("Cambio")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Cambio")),
                                MetodoPago = reader.IsDBNull(reader.GetOrdinal("METODO_PAGO")) ? null : reader.GetString(reader.GetOrdinal("METODO_PAGO")),
                                Detalles = new List<DetalleVenta>()
                            };
                            lista.Add(v);
                        }
                    }
                }
            }
            return lista;
        }

        public List<DetalleVenta> ObtenerDetalleVenta(int ventaId)
        {
            var lista = new List<DetalleVenta>();
            using (var conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                string query = @"
                    SELECT ID_DETALLE, ID_VENTA, ID_PRODUCTO, CANTIDAD, PRECIO_UNITARIO, SUBTOTAL
                    FROM DETALLE_VENTA WHERE ID_VENTA = @idVenta";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idVenta", ventaId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var detalle = new DetalleVenta
                            {
                                DetalleVentaID = reader.GetInt32(reader.GetOrdinal("ID_DETALLE")),
                                VentaID = reader.GetInt32(reader.GetOrdinal("ID_VENTA")),
                                ProductoID = reader.GetInt32(reader.GetOrdinal("ID_PRODUCTO")),
                                Cantidad = reader.GetInt32(reader.GetOrdinal("CANTIDAD")),
                                PrecioUnitario = reader.GetDecimal(reader.GetOrdinal("PRECIO_UNITARIO")),
                                Subtotal = reader.GetDecimal(reader.GetOrdinal("SUBTOTAL"))
                            };
                            lista.Add(detalle);
                        }
                    }
                }
            }
            return lista;
        }

        public List<VentaDetalladaReporte> ObtenerVentasDetalladasPorRango(DateTime desde, DateTime hasta)
        {
            var lista = new List<VentaDetalladaReporte>();
            using (var conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                string query = @"
                    SELECT 
                        V.ID_VENTA,
                        V.FECHA,
                        V.HORA,
                        E.NOMBRE_EMP,
                        P.NOMBRE AS NombreProducto,
                        DV.CANTIDAD,
                        DV.PRECIO_UNITARIO,
                        DV.SUBTOTAL,
                        V.MontoEfectivo,
                        V.MontoTarjeta,
                        V.METODO_PAGO
                    FROM VENTA V
                    INNER JOIN DETALLE_VENTA DV ON V.ID_VENTA = DV.ID_VENTA
                    INNER JOIN EMPLEADO E ON V.ID_EMPLEADO = E.ID_EMPLEADO
                    INNER JOIN PRODUCTO P ON DV.ID_PRODUCTO = P.ID_PRODUCTO
                    WHERE V.FECHA >= @desde AND V.FECHA <= @hasta
                    ORDER BY V.FECHA DESC, V.HORA DESC, V.ID_VENTA, P.NOMBRE;
                ";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@desde", desde.Date);
                    cmd.Parameters.AddWithValue("@hasta", hasta.Date);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string metodoPago = reader.IsDBNull(reader.GetOrdinal("METODO_PAGO")) ? "N/A" : reader.GetString(reader.GetOrdinal("METODO_PAGO"));

                            var reporte = new VentaDetalladaReporte
                            {
                                VentaID = reader.GetInt32(reader.GetOrdinal("ID_VENTA")),
                                FechaVenta = reader.GetDateTime(reader.GetOrdinal("FECHA")).Add((TimeSpan)reader.GetValue(reader.GetOrdinal("HORA"))),
                                NombreEmpleado = reader.GetString(reader.GetOrdinal("NOMBRE_EMP")),
                                NombreProducto = reader.GetString(reader.GetOrdinal("NombreProducto")),
                                Cantidad = reader.GetInt32(reader.GetOrdinal("CANTIDAD")),
                                PrecioUnitario = reader.GetDecimal(reader.GetOrdinal("PRECIO_UNITARIO")),
                                Subtotal = reader.GetDecimal(reader.GetOrdinal("SUBTOTAL")),
                                MetodoPago = metodoPago,
                                MontoEfectivo = reader.IsDBNull(reader.GetOrdinal("MontoEfectivo")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("MontoEfectivo")),
                                MontoTarjeta = reader.IsDBNull(reader.GetOrdinal("MontoTarjeta")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("MontoTarjeta"))
                            };
                            lista.Add(reporte);
                        }
                    }
                }
            }
            return lista;
        }
    }
}