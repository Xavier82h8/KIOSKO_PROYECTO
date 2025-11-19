using System;
using System.Windows.Forms;

namespace KIOSKO_Proyecto
{
    public partial class FormPrincipalPOS : Form
    {
        // Este archivo puede contener métodos adicionales o lógica de negocio
        // que no están directamente relacionados con el diseño del formulario.

        private void InitializeUserInterface()
        {
            // Initialize new controls and assign event handlers
            lblEmpleado.Click += LblEmpleado_Click;
            btnInventario.Click += BtnInventario_Click;
            btnDetalleVentas.Click += BtnDetalleVentas_Click;

            menuEmpleado = new ContextMenuStrip();
            itemCerrarSesion = new ToolStripMenuItem("Cerrar Sesión");
            itemCerrarSesion.Click += BtnCerrarSesion_Click;
            menuEmpleado.Items.Add(itemCerrarSesion);

            // Actualizar lblEmpleado directamente
            lblEmpleado.Text = $"👤 {_empleadoAutenticado.NombreEmp} ({_empleadoAutenticado.Puesto})";

            // Debugging: Show the authenticated employee's role
            MessageBox.Show($"Rol autenticado: {_empleadoAutenticado.Puesto}", "Información de Rol", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Lógica de visibilidad basada en rol
            if (_empleadoAutenticado.Puesto.Equals("supervisor", StringComparison.OrdinalIgnoreCase) || 
                _empleadoAutenticado.Puesto.Equals("supervisora", StringComparison.OrdinalIgnoreCase) ||
                _empleadoAutenticado.Puesto.Equals("gerente", StringComparison.OrdinalIgnoreCase) ||
                _empleadoAutenticado.Puesto.Equals("gerenta", StringComparison.OrdinalIgnoreCase) ||
                _empleadoAutenticado.Puesto.Equals("encargado", StringComparison.OrdinalIgnoreCase) ||
                _empleadoAutenticado.Puesto.Equals("encargado de almacen", StringComparison.OrdinalIgnoreCase) || // New role
                _empleadoAutenticado.Puesto.Equals("encargada de almacen", StringComparison.OrdinalIgnoreCase)) // New role
            {
                btnInventario.Visible = true;
                btnDetalleVentas.Visible = true;
                btnVerReportes.Visible = true;
            }
        }
    }
}