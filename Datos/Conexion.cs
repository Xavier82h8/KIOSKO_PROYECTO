using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows.Forms; // Added for MessageBox

namespace KIOSKO_Proyecto.Datos
{
    public class Conexion
    {
        private const string ConnName = "KIOSKO_ITHConnectionString";
        private const string Fallback = "Data Source=KARY_LAP;Initial Catalog=KIOSKO_ITH;Integrated Security=True";

        public static SqlConnection ObtenerConexion()
        {
            // Simplificado: no asignacin innecesaria
            var cs = ConfigurationManager.ConnectionStrings[ConnName];
            var connectionString = cs?.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
                connectionString = Fallback;

            return new SqlConnection(connectionString);
        }

        public static void EnsureDatabaseSchema()
        {
            using (SqlConnection connection = ObtenerConexion())
            {
                try
                {
                    connection.Open();

                    // Check and create Reportes table
                    string createReportesTableSql = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reportes' and xtype='U')
                        BEGIN
                            CREATE TABLE Reportes (
                                IdReporte INT PRIMARY KEY IDENTITY(1,1),
                                FechaGeneracion DATETIME NOT NULL,
                                FechaInicio DATETIME NOT NULL,
                                FechaFin DATETIME NOT NULL,
                                TotalVentas DECIMAL(18, 2) NOT NULL,
                                GeneradoPorEmpleadoId INT NOT NULL
                            );
                        END;";
                    using (SqlCommand command = new SqlCommand(createReportesTableSql, connection))
                    {
                        command.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine("Checked/Created 'Reportes' table.");
                    }

                    // Check and create FK_Reportes_Empleado foreign key
                    string createForeignKeySql = @"
                        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Reportes_Empleado')
                        BEGIN
                            ALTER TABLE Reportes
                            ADD CONSTRAINT FK_Reportes_Empleado FOREIGN KEY (GeneradoPorEmpleadoId) REFERENCES Empleado(IdEmpleado);
                        END;";
                    using (SqlCommand command = new SqlCommand(createForeignKeySql, connection))
                    {
                        command.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine("Checked/Created 'FK_Reportes_Empleado' foreign key.");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error ensuring database schema: {ex.Message}");
                    MessageBox.Show($"Error al asegurar el esquema de la base de datos: {ex.Message}", "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}