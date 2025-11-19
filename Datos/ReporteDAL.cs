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
    }
}
