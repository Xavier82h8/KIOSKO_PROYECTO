using System;
using System.Windows.Forms;
using KIOSKO_Proyecto; // Explicitly add this
using KIOSKO_Proyecto.Datos; // Added for Conexion

namespace KIOSKO_Proyecto
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Ensure database schema is up to date
            Conexion.EnsureDatabaseSchema();

            // Bucle: mostrar login, luego ventana principal. Si se recibe DialogResult.Retry => volver al login.
            while (true)
            {
                using (var loginForm = new KIOSKO_Proyecto.FormLogin())
                {
                    var loginResult = loginForm.ShowDialog();
                    if (loginResult != DialogResult.OK)
                        break;

                    // Obtener el objeto Empleado autenticado
                    var empleadoAutenticado = loginForm.EmpleadoAutenticado;
                    if (empleadoAutenticado == null)
                    {
                        MessageBox.Show("Error al obtener los datos del empleado. Intente de nuevo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue; // Volver al login
                    }

                    using (var mainForm = new FormPrincipalPOS(empleadoAutenticado))
                    {
                        var mainResult = mainForm.ShowDialog();
                        if (mainResult == DialogResult.Retry)
                        {
                            // Cerrar sesión: volver a mostrar login
                            continue;
                        }

                        // Cerrar la aplicación en cualquier otro caso
                        break;
                    }
                }
            }
        }
    }
}
