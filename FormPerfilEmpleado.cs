using System;
using System.Drawing;
using System.Windows.Forms;
using KIOSKO_Proyecto.BLL;

namespace KIOSKO_Proyecto
{
    public class FormPerfilEmpleado : Form
    {
        private EmpleadoBLL _empleadoBLL = new EmpleadoBLL();
        private Modelos.Empleado _empleadoActual;

        private TextBox txtNombre;
        private TextBox txtPuesto;
        private TextBox txtContrasena;
        private TextBox txtContrasenaConfirm;
        private Button btnGuardar;

        public FormPerfilEmpleado(Modelos.Empleado empleado)
        {
            _empleadoActual = empleado;
            this.Text = "Perfil";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(420, 220);

            txtNombre = new TextBox { Location = new Point(120, 12), Width = 260, ReadOnly = true };
            txtPuesto = new TextBox { Location = new Point(120, 42), Width = 260, ReadOnly = true };
            txtContrasena = new TextBox { Location = new Point(120, 72), Width = 260, UseSystemPasswordChar = true };
            txtContrasenaConfirm = new TextBox { Location = new Point(120, 102), Width = 260, UseSystemPasswordChar = true };
            btnGuardar = new Button { Text = "Guardar", Location = new Point(300, 140), Size = new Size(80, 30) };
            btnGuardar.Click += BtnGuardar_Click;

            this.Controls.Add(new Label { Text = "Nombre:", Location = new Point(12, 14) });
            this.Controls.Add(txtNombre);
            this.Controls.Add(new Label { Text = "Puesto:", Location = new Point(12, 44) });
            this.Controls.Add(txtPuesto);
            this.Controls.Add(new Label { Text = "Nueva contrase�a:", Location = new Point(12, 74) });
            this.Controls.Add(txtContrasena);
            this.Controls.Add(new Label { Text = "Confirmar contrase�a:", Location = new Point(12, 104) });
            this.Controls.Add(txtContrasenaConfirm);
            this.Controls.Add(btnGuardar);

            Load += (s, e) => CargarDatos();
        }

        private void CargarDatos()
        {
            if (_empleadoActual != null)
            {
                txtNombre.Text = _empleadoActual.NombreEmp;
                txtPuesto.Text = _empleadoActual.Puesto;
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            var pass = txtContrasena.Text ?? string.Empty;
            var pass2 = txtContrasenaConfirm.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Introduce la nueva contrase�a.", "Atenci�n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtContrasena.Focus();
                return;
            }
            if (pass != pass2)
            {
                MessageBox.Show("Las contrase�as no coinciden.", "Atenci�n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtContrasenaConfirm.Focus();
                return;
            }

            bool ok = _empleadoBLL.CambiarContrasena(_empleadoActual.IdEmpleado, pass);
            if (ok)
            {
                MessageBox.Show("Contrase�a actualizada.", "�xito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("No se pudo actualizar la contrase�a.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}