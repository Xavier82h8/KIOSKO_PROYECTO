using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using KIOSKO_Proyecto.Modelos;

namespace KIOSKO_Proyecto.Datos
{
    public class InventarioDAL
    {
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
                        i.TOTAL_PRODUCTOS,
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
                                Cantidad = reader.GetInt32(reader.GetOrdinal("TOTAL_PRODUCTOS")),
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

        public bool RegistrarEntrada(Inventario registro)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Actualizar el stock del producto
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

                        // 2. Insertar el registro en la tabla de inventario
                        string queryInventario = @"
                            INSERT INTO INVENTARIO (ID_PRODUCTO, CANTIDAD, FECHA_REGISTRO, OBSERVACIONES, PROVEEDOR)

                            INSERT INTO INVENTARIO (ID_PRODUCTO, TOTAL_PRODUCTOS, FECHA_REGISTRO, OBSERVACIONES, PROVEEDOR) 
                            VALUES (@IdProducto, @Cantidad, @FechaRegistro, @Observaciones, @Proveedor)";

                        using (var inventarioCmd = new SqlCommand(queryInventario, conn, transaction))
                        {
                            inventarioCmd.Parameters.AddWithValue("@IdProducto", registro.IdProducto);
                            inventarioCmd.Parameters.AddWithValue("@Cantidad", registro.Cantidad);
                            inventarioCmd.Parameters.AddWithValue("@FechaRegistro", registro.FechaRegistro);
                            inventarioCmd.Parameters.AddWithValue("@Observaciones", (object)registro.Observaciones ?? DBNull.Value);
                            inventarioCmd.Parameters.AddWithValue("@Proveedor", (object)registro.Proveedor ?? DBNull.Value);
                            inventarioCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"Error al registrar entrada de inventario: {ex.Message}");
                        return false;
                    }
                }
            }
        }
    }
}
