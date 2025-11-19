using System;
using System.Drawing;
using System.Windows.Forms;
using KIOSKO_Proyecto.BLL;
using KIOSKO_Proyecto.Modelos;

namespace KIOSKO_Proyecto
{
    public partial class FormGestionInventario : Form
    {
        private InventarioBLL inventarioBLL = new InventarioBLL();
        private DataGridView dgvHistorial;
        private Button btnRegistrarEntrada;

        public FormGestionInventario()
        {
            InitializeComponent();
            CargarHistorial();
        }

        private void InitializeComponent()
        {
            this.Text = "Gestión de Inventario - Historial de Entradas";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Panel superior
            var panelSuperior = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(10) };
            btnRegistrarEntrada = new Button { Text = "Registrar Nueva Entrada", Width = 180, Height = 40, BackColor = Color.FromArgb(92, 184, 92), ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnRegistrarEntrada.Click += BtnRegistrarEntrada_Click;
            panelSuperior.Controls.Add(btnRegistrarEntrada);
            this.Controls.Add(panelSuperior);

            // DataGridView
            dgvHistorial = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            this.Controls.Add(dgvHistorial);
            dgvHistorial.BringToFront();

            ConfigurarDgv();
        }

        private void ConfigurarDgv()
        {
            dgvHistorial.Columns.Clear();
            dgvHistorial.Columns.Add("FechaRegistro", "Fecha de Registro");
            dgvHistorial.Columns.Add("NombreProducto", "Producto");
            dgvHistorial.Columns.Add("Cantidad", "Cantidad Ingresada");
            dgvHistorial.Columns.Add("Proveedor", "Proveedor");
            dgvHistorial.Columns.Add("Observaciones", "Observaciones");

            dgvHistorial.Columns["FechaRegistro"].DefaultCellStyle.Format = "g";
            dgvHistorial.Columns["Observaciones"].FillWeight = 150;
        }

        private void CargarHistorial()
        {
            try
            {
                dgvHistorial.Rows.Clear();
                var historial = inventarioBLL.ObtenerHistorialInventario();
                foreach (var registro in historial)
                {
                    dgvHistorial.Rows.Add(
                        registro.FechaRegistro,
                        registro.NombreProducto,
                        registro.Cantidad,
                        registro.Proveedor,
                        registro.Observaciones
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el historial: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRegistrarEntrada_Click(object sender, EventArgs e)
        {
            using (var formRegistro = new FormRegistroEntrada())
            {
                if (formRegistro.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        bool exito = inventarioBLL.RegistrarEntrada(formRegistro.NuevoRegistro);
                        if (exito)
                        {
                            MessageBox.Show("Entrada de stock registrada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            CargarHistorial(); // Recargar el historial para ver el nuevo registro
                        }
                        else
                        {
                            MessageBox.Show("No se pudo registrar la entrada de stock.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ocurrió un error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
