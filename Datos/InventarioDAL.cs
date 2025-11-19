using System.Collections.Generic;
using System.Data.SqlClient;
using KIOSKO_Proyecto.Modelos;

namespace KIOSKO_Proyecto.Datos
{
    public class InventarioDAL
    {
        public List<Inventario> ObtenerInventario()
        {
            var inventario = new List<Inventario>();
            using (var conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT ID_INVENTARIO, FECHA_REGISTRO, TOTAL_PRODUCTOS, OBSERVACIONES, PROVEEDOR FROM INVENTARIO", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            inventario.Add(new Inventario
                            {
                                IdInventario = reader.GetInt32(0),
                                FechaRegistro = reader.GetDateTime(1),
                                TotalProductos = reader.GetInt32(2),
                                Observaciones = reader.GetString(3),
                                Proveedor = reader.GetString(4)
                            });
                        }
                    }
                }
            }
            return inventario;
        }
    }
}
