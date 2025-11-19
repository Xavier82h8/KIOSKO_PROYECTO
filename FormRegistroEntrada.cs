using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using KIOSKO_Proyecto.BLL;
using KIOSKO_Proyecto.Modelos;

namespace KIOSKO_Proyecto
{
    public partial class FormRegistroEntrada : Form
    {
        private ProductoBLL productoBLL = new ProductoBLL();
        public Inventario NuevoRegistro { get; private set; }

        private ComboBox cmbProducto;
        private NumericUpDown numCantidad;
        private TextBox txtProveedor;
        private TextBox txtObservaciones;
        private Button btnGuardar;
        private Button btnCancelar;

        public FormRegistroEntrada()
        {
            InitializeComponent();
            CargarProductos();
        }

        private void InitializeComponent()
        {
            this.Text = "Registrar Entrada de Stock";
            this.Size = new System.Drawing.Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Controles
            var lblProducto = new Label { Text = "Producto:", Location = new System.Drawing.Point(20, 23), AutoSize = true };
            cmbProducto = new ComboBox { Location = new System.Drawing.Point(120, 20), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblCantidad = new Label { Text = "Cantidad:", Location = new System.Drawing.Point(20, 63), AutoSize = true };
            numCantidad = new NumericUpDown { Location = new System.Drawing.Point(120, 60), Width = 100, Minimum = 1, Maximum = 1000 };

            var lblProveedor = new Label { Text = "Proveedor:", Location = new System.Drawing.Point(20, 103), AutoSize = true };
            txtProveedor = new TextBox { Location = new System.Drawing.Point(120, 100), Width = 240 };

            var lblObservaciones = new Label { Text = "Observaciones:", Location = new System.Drawing.Point(20, 143), AutoSize = true };
            txtObservaciones = new TextBox { Location = new System.Drawing.Point(120, 140), Width = 240, Height = 60, Multiline = true };

            btnGuardar = new Button { Text = "Guardar", Location = new System.Drawing.Point(120, 220), Width = 100 };
            btnCancelar = new Button { Text = "Cancelar", Location = new System.Drawing.Point(260, 220), Width = 100 };

            // Eventos
            btnGuardar.Click += BtnGuardar_Click;
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; };

            this.Controls.Add(lblProducto);
            this.Controls.Add(cmbProducto);
            this.Controls.Add(lblCantidad);
            this.Controls.Add(numCantidad);
            this.Controls.Add(lblProveedor);
            this.Controls.Add(txtProveedor);
            this.Controls.Add(lblObservaciones);
            this.Controls.Add(txtObservaciones);
            this.Controls.Add(btnGuardar);
            this.Controls.Add(btnCancelar);
        }

        private void CargarProductos()
        {
            var productos = productoBLL.ObtenerProductosDisponibles();
            cmbProducto.DataSource = productos;
            cmbProducto.DisplayMember = "Nombre";
            cmbProducto.ValueMember = "IdProducto";
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (cmbProducto.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar un producto.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            NuevoRegistro = new Inventario
            {
                IdProducto = (int)cmbProducto.SelectedValue,
                Cantidad = (int)numCantidad.Value,
                Proveedor = txtProveedor.Text,
                Observaciones = txtObservaciones.Text,
                FechaRegistro = DateTime.Now
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
