using System;
using System.Data.SqlClient;
using KIOSKO_Proyecto.Modelos;

namespace KIOSKO_Proyecto.Datos
{
    public class EmpleadoDAL
    {
        public Empleado ObtenerEmpleadoPorId(int id)
        {
            // Fallback SQL directo (seguro si la columna existe en BD)
            using (var conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT ID_EMPLEADO, NOMBRE_EMP, EDAD, DIRECCION, TELEFONO, PUESTO, TURNO, SALARIO, CONTRASENA FROM EMPLEADO WHERE ID_EMPLEADO = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Empleado
                            {
                                IdEmpleado = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                NombreEmp = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                Edad = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                Direccion = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                Telefono = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                Puesto = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                Turno = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                                Salario = reader.IsDBNull(7) ? 0m : reader.GetDecimal(7),
                                Contrasena = reader.IsDBNull(8) ? string.Empty : reader.GetString(8)
                            };
                        }
                    }
                }
            }

            return null;
        }

        public int? ValidarCredenciales(string nombreEmp, string contrasena)
        {
            // Fallback SQL
            using (var conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT ID_EMPLEADO FROM EMPLEADO WHERE NOMBRE_EMP = @nombreEmp AND CONTRASENA = @contrasena", conn))
                {
                    cmd.Parameters.AddWithValue("@nombreEmp", nombreEmp);
                    cmd.Parameters.AddWithValue("@contrasena", contrasena);
                    object res = cmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value) return Convert.ToInt32(res);
                }
            }

            return null;
        }

        public bool ActualizarContrasena(int idEmpleado, string nuevaContrasena)
        {
            using (var conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                using (var cmd = new SqlCommand("UPDATE EMPLEADO SET CONTRASENA = @pass WHERE ID_EMPLEADO = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@pass", nuevaContrasena);
                    cmd.Parameters.AddWithValue("@id", idEmpleado);
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }
    }
}