using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using KIOSKO_Proyecto.Modelos;

namespace KIOSKO_Proyecto.Datos
{
    public class ProductoDAL
    {
        public List<Producto> ObtenerProductos()
        {
            var productos = new List<Producto>();
            try
            {
                using (var conn = Conexion.ObtenerConexion())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT ID_PRODUCTO, NOMBRE, CATEGORIA, PRECIO, CANTIDAD_DISPONIBLE, FECHA_CADUCIDAD FROM dbo.PRODUCTO", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    productos.Add(new Producto
                                    {
                                        IdProducto = reader.GetInt32(reader.GetOrdinal("ID_PRODUCTO")),
                                        Nombre = reader.GetString(reader.GetOrdinal("NOMBRE")),
                                        Categoria = reader.GetString(reader.GetOrdinal("CATEGORIA")),
                                        Precio = reader.GetDecimal(reader.GetOrdinal("PRECIO")),
                                        CantidadDisponible = reader.GetInt32(reader.GetOrdinal("CANTIDAD_DISPONIBLE")),
                                        FechaCaducidad = reader.IsDBNull(reader.GetOrdinal("FECHA_CADUCIDAD")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("FECHA_CADUCIDAD"))
                                    });
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine("ProductoDAL: error mapeando fila: " + ex.Message);
                                }
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Si el error es "Invalid column name 'FECHA_CADUCIDAD'", el usuario no ha actualizado la BD.
                if (ex.Message.Contains("Invalid column name"))
                {
                    throw new Exception("La base de datos no está actualizada. Falta la columna 'FECHA_CADUCIDAD' en la tabla 'PRODUCTO'. Por favor, aplica el script de 'sql_migrations.md'.", ex);
                }
                throw new Exception("Error de base de datos al obtener productos: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al obtener productos: " + ex.Message, ex);
            }
            return productos;
        }

        public Producto ObtenerProductoPorId(int idProducto)
        {
            try
            {
                using (var conn = Conexion.ObtenerConexion())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT ID_PRODUCTO, NOMBRE, CATEGORIA, PRECIO, CANTIDAD_DISPONIBLE, FECHA_CADUCIDAD FROM dbo.PRODUCTO WHERE ID_PRODUCTO = @IdProducto", conn))
                    {
                        cmd.Parameters.AddWithValue("@IdProducto", idProducto);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Producto
                                {
                                    IdProducto = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                    Nombre = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                    Categoria = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                    Precio = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),
                                    CantidadDisponible = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                    FechaCaducidad = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al obtener producto por ID: " + ex.Message);
            }
            return null;
        }

        public void AgregarProducto(Producto producto)
        {
            try
            {
                using (var conn = Conexion.ObtenerConexion())
                {
                    conn.Open();
                    string query = "INSERT INTO dbo.PRODUCTO (NOMBRE, CATEGORIA, PRECIO, CANTIDAD_DISPONIBLE, FECHA_CADUCIDAD) VALUES (@Nombre, @Categoria, @Precio, @CantidadDisponible, @FechaCaducidad)";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", producto.Nombre);
                        cmd.Parameters.AddWithValue("@Categoria", producto.Categoria);
                        cmd.Parameters.AddWithValue("@Precio", producto.Precio);
                        cmd.Parameters.AddWithValue("@CantidadDisponible", producto.CantidadDisponible);
                        cmd.Parameters.AddWithValue("@FechaCaducidad", (object)producto.FechaCaducidad ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error de base de datos al agregar el producto: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al agregar el producto: " + ex.Message, ex);
            }
        }

        public void ActualizarProducto(Producto producto)
        {
            try
            {
                using (var conn = Conexion.ObtenerConexion())
                {
                    conn.Open();
                    string query = "UPDATE dbo.PRODUCTO SET NOMBRE = @Nombre, CATEGORIA = @Categoria, PRECIO = @Precio, CANTIDAD_DISPONIBLE = @CantidadDisponible, FECHA_CADUCIDAD = @FechaCaducidad WHERE ID_PRODUCTO = @IdProducto";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", producto.Nombre);
                        cmd.Parameters.AddWithValue("@Categoria", producto.Categoria);
                        cmd.Parameters.AddWithValue("@Precio", producto.Precio);
                        cmd.Parameters.AddWithValue("@CantidadDisponible", producto.CantidadDisponible);
                        cmd.Parameters.AddWithValue("@FechaCaducidad", (object)producto.FechaCaducidad ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IdProducto", producto.IdProducto);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error de base de datos al actualizar el producto: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al actualizar el producto: " + ex.Message, ex);
            }
        }

        public void EliminarProducto(int idProducto)
        {
            try
            {
                using (var conn = Conexion.ObtenerConexion())
                {
                    conn.Open();
                    string query = "DELETE FROM dbo.PRODUCTO WHERE ID_PRODUCTO = @IdProducto";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdProducto", idProducto);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                // Check for foreign key constraint violation
                if (ex.Number == 547)
                {
                    throw new Exception("No se puede eliminar el producto porque está siendo referenciado en otras tablas (por ejemplo, en ventas existentes).", ex);
                }
                throw new Exception("Error de base de datos al eliminar el producto: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al eliminar el producto: " + ex.Message, ex);
            }
        }
    }
}
