using System;
using System.Windows.Forms;
using KIOSKO_Proyecto.Datos; // Asegúrate de que este namespace exista para Conexion
using KIOSKO_Proyecto.BLL;   // Para manejo de excepciones globales si fuera necesario

namespace KIOSKO_Proyecto
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // 1. Verificar base de datos antes de iniciar nada
                // Si esta línea falla, caerá en el catch de abajo
                Conexion.EnsureDatabaseSchema();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error crítico al conectar con la Base de Datos al inicio:\n{ex.Message}",
                    "Error Fatal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Cierra la app si no hay BD
            }

            // 2. Bucle infinito de la aplicación (Login <-> Principal)
            bool ejecutando = true;

            while (ejecutando)
            {
                // PASO A: Mostrar Login
                using (var loginForm = new FormLogin())
                {
                    // Mostramos el login como diálogo modal
                    var resultadoLogin = loginForm.ShowDialog();

                    if (resultadoLogin == DialogResult.OK)
                    {
                        // Login exitoso, obtenemos al usuario
                        var empleado = loginForm.EmpleadoAutenticado;

                        if (empleado != null)
                        {
                            // PASO B: Iniciar Formulario Principal
                            // Asumo que tu formulario principal se llama FormPrincipalPOS
                            // Asegúrate de que su constructor acepte el objeto 'empleado'
                            using (var mainForm = new FormPrincipalPOS(empleado))
                            {
                                // Mostramos el principal. El código se detiene aquí hasta que se cierre el Principal.
                                var resultadoMain = mainForm.ShowDialog();

                                if (resultadoMain == DialogResult.Retry)
                                {
                                    // El usuario dio clic en "Cerrar Sesión".
                                    // El bucle 'while' continúa, volviendo a crear un nuevo FormLogin arriba.
                                    continue;
                                }
                                else
                                {
                                    // El usuario cerró la app con la X o Alt+F4.
                                    ejecutando = false; // Rompe el bucle
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Error: Empleado autenticado es nulo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // Vuelve al inicio del while
                        }
                    }
                    else
                    {
                        // El usuario canceló o cerró el Login
                        ejecutando = false; // Salir de la app
                    }
                }
            } // Fin del While
        }
    }
}