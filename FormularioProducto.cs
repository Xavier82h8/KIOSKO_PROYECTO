using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using KIOSKO_Proyecto.Modelos;

namespace KIOSKO_Proyecto
{
    public class FormularioProducto : Form
    {
        public Producto Producto { get; private set; }

        private TextBox txtNombre;
        private TextBox txtCategoria;
        private NumericUpDown numPrecio;
        private NumericUpDown numCantidad;
        private DateTimePicker dtpFechaCaducidad;
        private CheckBox chkNoAplicaFecha;

        public FormularioProducto(Producto producto = null)
        {
            this.Producto = producto ?? new Producto();
            InitializeComponent();
            if (producto != null)
            {
                this.Text = "Editar Producto";
                CargarDatosProducto();
            }
            else
            {
                this.Text = "Agregar Producto";
            }
        }

        private void InitializeComponent()
        {
            this.Size = new Size(450, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Padding = new Padding(20);
            this.BackColor = Color.White;

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7
            };
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Nombre
            tableLayout.Controls.Add(new Label { Text = "Nombre:", Font = new Font("Segoe UI", 10), TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            txtNombre = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
            tableLayout.Controls.Add(txtNombre, 1, 0);

            // Categoría
            tableLayout.Controls.Add(new Label { Text = "Categoría:", Font = new Font("Segoe UI", 10), TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 1);
            txtCategoria = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
            tableLayout.Controls.Add(txtCategoria, 1, 1);

            // Precio
            tableLayout.Controls.Add(new Label { Text = "Precio:", Font = new Font("Segoe UI", 10), TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 2);
            numPrecio = new NumericUpDown { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), DecimalPlaces = 2, Maximum = 10000 };
            tableLayout.Controls.Add(numPrecio, 1, 2);

            // Cantidad
            tableLayout.Controls.Add(new Label { Text = "Cantidad Disponible:", Font = new Font("Segoe UI", 10), TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 3);
            numCantidad = new NumericUpDown { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), Maximum = 10000 };
            tableLayout.Controls.Add(numCantidad, 1, 3);

            // Fecha de Caducidad
            tableLayout.Controls.Add(new Label { Text = "Fecha de Caducidad:", Font = new Font("Segoe UI", 10), TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 4);
            dtpFechaCaducidad = new DateTimePicker { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), Format = DateTimePickerFormat.Short };
            tableLayout.Controls.Add(dtpFechaCaducidad, 1, 4);

            // Checkbox para fecha N/A
            chkNoAplicaFecha = new CheckBox { Text = "No aplica", Font = new Font("Segoe UI", 9), Dock = DockStyle.Fill };
            chkNoAplicaFecha.CheckedChanged += (s, e) => {
                dtpFechaCaducidad.Enabled = !chkNoAplicaFecha.Checked;
            };
            tableLayout.Controls.Add(chkNoAplicaFecha, 1, 5);


            // Botones
            var panelBotones = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill
            };

            var btnGuardar = new Button { Text = "Guardar", DialogResult = DialogResult.OK, Width = 120, Height = 40, Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;

            var btnCancelar = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Width = 120, Height = 40, Font = new Font("Segoe UI", 10), BackColor = Color.Gainsboro, FlatStyle = FlatStyle.Flat };
            btnCancelar.FlatAppearance.BorderSize = 0;

            panelBotones.Controls.Add(btnGuardar);
            panelBotones.Controls.Add(btnCancelar);
            tableLayout.Controls.Add(panelBotones, 0, 6);
            tableLayout.SetColumnSpan(panelBotones, 2);

            for (int i = 0; i < tableLayout.RowCount; i++)
            {
                tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            }
            tableLayout.RowStyles[6] = new RowStyle(SizeType.Absolute, 60);


            this.Controls.Add(tableLayout);
        }

        private void CargarDatosProducto()
        {
            txtNombre.Text = Producto.Nombre;
            txtCategoria.Text = Producto.Categoria;
            numPrecio.Value = Producto.Precio;
            numCantidad.Value = Producto.CantidadDisponible;
            if (Producto.FechaCaducidad.HasValue)
            {
                dtpFechaCaducidad.Value = Producto.FechaCaducidad.Value;
                chkNoAplicaFecha.Checked = false;
            }
            else
            {
                dtpFechaCaducidad.Value = DateTime.Now;
                dtpFechaCaducidad.Enabled = false;
                chkNoAplicaFecha.Checked = true;
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre del producto es obligatorio.", "Dato Requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None; // Evita que el formulario se cierre
                txtNombre.Focus();
                return;
            }

            Producto.Nombre = txtNombre.Text;
            Producto.Categoria = txtCategoria.Text;
            Producto.Precio = numPrecio.Value;
            Producto.CantidadDisponible = (int)numCantidad.Value;
            Producto.FechaCaducidad = chkNoAplicaFecha.Checked ? (DateTime?)null : dtpFechaCaducidad.Value;

            this.DialogResult = DialogResult.OK;
        }
    }
}
