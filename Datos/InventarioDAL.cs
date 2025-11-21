using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using KIOSKO_Proyecto.Modelos;

namespace KIOSKO_Proyecto.Datos
{
    public class InventarioDAL
    {
        // 1. Obtener el historial de movimientos
        public List<Inventario> ObtenerHistorialInventario()
        {
            var historial = new List<Inventario>();
            using (var conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                string query = @"
                    SELECT 
                        i.ID_INVENTARIO, 
                        i.ID_PRODUCTO,
                        p.NOMBRE as NombreProducto,
                        i.CANTIDAD, 
                        i.FECHA_REGISTRO, 
                        i.OBSERVACIONES, 
                        i.PROVEEDOR 
                    FROM INVENTARIO i
                    JOIN PRODUCTO p ON i.ID_PRODUCTO = p.ID_PRODUCTO
                    ORDER BY i.FECHA_REGISTRO DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            historial.Add(new Inventario
                            {
                                IdInventario = reader.GetInt32(reader.GetOrdinal("ID_INVENTARIO")),
                                IdProducto = reader.GetInt32(reader.GetOrdinal("ID_PRODUCTO")),
                                NombreProducto = reader.GetString(reader.GetOrdinal("NombreProducto")),
                                Cantidad = reader.GetInt32(reader.GetOrdinal("CANTIDAD")),
                                FechaRegistro = reader.GetDateTime(reader.GetOrdinal("FECHA_REGISTRO")),
                                Observaciones = reader.IsDBNull(reader.GetOrdinal("OBSERVACIONES")) ? "" : reader.GetString(reader.GetOrdinal("OBSERVACIONES")),
                                Proveedor = reader.IsDBNull(reader.GetOrdinal("PROVEEDOR")) ? "" : reader.GetString(reader.GetOrdinal("PROVEEDOR"))
                            });
                        }
                    }
                }
            }
            return historial;
        }

        // 2. Registrar entrada de mercancía (ROBUSTO: Toca Stock, Inventario y Pagos)
        public bool RegistrarEntrada(Inventario registro)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // A. Actualizar el stock del producto (Tabla PRODUCTO)
                        string queryStock = @"
                            UPDATE PRODUCTO SET CANTIDAD_DISPONIBLE = CANTIDAD_DISPONIBLE + @Cantidad
                            WHERE ID_PRODUCTO = @IdProducto";

                        using (var stockCmd = new SqlCommand(queryStock, conn, transaction))
                        {
                            stockCmd.Parameters.AddWithValue("@Cantidad", registro.Cantidad);
                            stockCmd.Parameters.AddWithValue("@IdProducto", registro.IdProducto);

                            int rowsAffected = stockCmd.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                throw new Exception("El producto no fue encontrado, no se pudo actualizar el stock.");
                            }
                        }

                        // B. Insertar el registro en el historial (Tabla INVENTARIO) y obtener ID
                        string queryInventario = @"
                            INSERT INTO INVENTARIO (ID_PRODUCTO, CANTIDAD, FECHA_REGISTRO, OBSERVACIONES, PROVEEDOR)
                            VALUES (@IdProducto, @Cantidad, @FechaRegistro, @Observaciones, @Proveedor);
                            SELECT SCOPE_IDENTITY();"; // Recuperar el ID recién creado

                        int idInventarioGenerado = 0;

                        using (var inventarioCmd = new SqlCommand(queryInventario, conn, transaction))
                        {
                            inventarioCmd.Parameters.AddWithValue("@IdProducto", registro.IdProducto);
                            inventarioCmd.Parameters.AddWithValue("@Cantidad", registro.Cantidad);
                            inventarioCmd.Parameters.AddWithValue("@FechaRegistro", registro.FechaRegistro);
                            inventarioCmd.Parameters.AddWithValue("@Observaciones", (object)registro.Observaciones ?? DBNull.Value);
                            inventarioCmd.Parameters.AddWithValue("@Proveedor", (object)registro.Proveedor ?? DBNull.Value);

                            // Ejecutar y obtener el ID
                            object result = inventarioCmd.ExecuteScalar();
                            idInventarioGenerado = Convert.ToInt32(result);
                        }

                        // C. Insertar el PAGO (Salida de dinero - Gasto)
                        // Solo si hay un costo asociado (CostoTotal > 0)
                        if (registro.CostoTotal > 0)
                        {
                            string queryPago = @"
                                INSERT INTO PAGO (FECHA_PAGO, MONTO, TIPO_PAGO, ID_VENTA, ID_INVENTARIO)
                                VALUES (@Fecha, @Monto, @Tipo, NULL, @IdInv)";

                            using (var pagoCmd = new SqlCommand(queryPago, conn, transaction))
                            {
                                pagoCmd.Parameters.AddWithValue("@Fecha", DateTime.Now);
                                pagoCmd.Parameters.AddWithValue("@Monto", registro.CostoTotal);
                                // Construimos una descripción útil
                                string metodo = string.IsNullOrEmpty(registro.MetodoPago) ? "Efectivo" : registro.MetodoPago;
                                pagoCmd.Parameters.AddWithValue("@Tipo", "COMPRA PROVEEDOR - " + metodo);
                                pagoCmd.Parameters.AddWithValue("@IdInv", idInventarioGenerado);

                                pagoCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit(); // Confirmar cambios
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // Deshacer cambios si algo falla
                        System.Diagnostics.Debug.WriteLine($"Error al registrar entrada de inventario: {ex.Message}");
                        // Relanzamos la excepción para que la capa superior (Form) pueda mostrar el mensaje al usuario
                        throw new Exception("Error en base de datos: " + ex.Message);
                    }
                }
            }
        }
    }
}