using System;
using System.Drawing;
using System.Windows.Forms;
using KIOSKO_Proyecto.Modelos;
using KIOSKO_Proyecto.BLL; // Importante: Agregar referencia a la BLL
using System.Collections.Generic;

namespace KIOSKO_Proyecto
{
    public class FormularioProducto : Form
    {
        public Modelos.Empleado EmpleadoAutenticado { get; set; }
        public Producto Producto { get; private set; }

        // Instancia de la lógica de negocio
        private ProductoBLL _productoBLL;

        private TextBox txtNombre;

        // CAMBIO: Usamos ComboBox para sugerir categorías existentes o permitir nuevas
        private ComboBox cmbCategoria;

        private NumericUpDown numPrecio;
        private NumericUpDown numCantidad;
        private DateTimePicker dtpFechaCaducidad;
        private CheckBox chkNoAplicaFecha;

        public FormularioProducto(Producto producto = null)
        {
            // Inicializar BLL
            _productoBLL = new ProductoBLL();

            this.Producto = producto ?? new Producto();
            InitializeComponent();

            // Cargar las categorías existentes en el ComboBox
            CargarCategorias();

            if (producto != null && producto.IdProducto > 0)
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
            this.Size = new Size(480, 500); // Un poco más alto
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

            // 1. Nombre
            AgregarControl(tableLayout, "Nombre:", txtNombre = new TextBox { Font = new Font("Segoe UI", 10) }, 0);

            // 2. Categoría (Ahora es ComboBox)
            cmbCategoria = new ComboBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems,
                DropDownStyle = ComboBoxStyle.DropDown // Permite escribir valores nuevos
            };
            AgregarControl(tableLayout, "Categoría:", cmbCategoria, 1);

            // 3. Precio
            numPrecio = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                DecimalPlaces = 2,
                Maximum = 100000,
                Minimum = 0
            };
            AgregarControl(tableLayout, "Precio:", numPrecio, 2);

            // 4. Cantidad
            numCantidad = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                Maximum = 10000,
                Minimum = 0
            };
            AgregarControl(tableLayout, "Cantidad Disponible:", numCantidad, 3);

            // 5. Fecha de Caducidad
            dtpFechaCaducidad = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                Format = DateTimePickerFormat.Short
            };
            AgregarControl(tableLayout, "Fecha de Caducidad:", dtpFechaCaducidad, 4);

            // 6. Checkbox No Aplica
            chkNoAplicaFecha = new CheckBox
            {
                Text = "No tiene fecha de caducidad",
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Fill,
                AutoSize = true
            };
            chkNoAplicaFecha.CheckedChanged += (s, e) => dtpFechaCaducidad.Enabled = !chkNoAplicaFecha.Checked;

            tableLayout.Controls.Add(chkNoAplicaFecha, 1, 5);

            // 7. Botones
            var panelBotones = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            var btnGuardar = new Button
            {
                Text = "Guardar",
                Width = 120,
                Height = 40,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Width = 120,
                Height = 40,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.Gainsboro,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderSize = 0;

            panelBotones.Controls.Add(btnGuardar);
            panelBotones.Controls.Add(btnCancelar);
            tableLayout.Controls.Add(panelBotones, 0, 6);
            tableLayout.SetColumnSpan(panelBotones, 2);

            // Espaciado vertical
            for (int i = 0; i < 6; i++) tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            this.Controls.Add(tableLayout);
        }

        // Helper para agregar controles a la tabla limpiamente
        private void AgregarControl(TableLayoutPanel panel, string labelText, Control control, int row)
        {
            panel.Controls.Add(new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            }, 0, row);

            control.Dock = DockStyle.Fill;
            panel.Controls.Add(control, 1, row);
        }

        private void CargarCategorias()
        {
            try
            {
                // Obtenemos las categorías existentes de la base de datos
                List<string> categorias = _productoBLL.ObtenerCategorias();
                cmbCategoria.Items.Clear();
                cmbCategoria.Items.AddRange(categorias.ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron cargar las categorías: " + ex.Message);
            }
        }

        private void CargarDatosProducto()
        {
            txtNombre.Text = Producto.Nombre;
            // Asigna el valor al combo. Si no está en la lista, simplemente lo muestra como texto.
            cmbCategoria.Text = Producto.Categoria;
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
                chkNoAplicaFecha.Checked = true;
            }
            dtpFechaCaducidad.Enabled = !chkNoAplicaFecha.Checked;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            // 1. Validaciones básicas
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }

            // Validamos cmbCategoria.Text en lugar de SelectedItem para capturar valores nuevos escritos
            if (string.IsNullOrWhiteSpace(cmbCategoria.Text))
            {
                MessageBox.Show("La categoría es obligatoria.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCategoria.Focus();
                return;
            }

            if (numPrecio.Value <= 0)
            {
                MessageBox.Show("El precio debe ser mayor a 0.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numPrecio.Focus();
                return;
            }

            // 2. Asignar valores al objeto Producto
            Producto.Nombre = txtNombre.Text.Trim();
            Producto.Categoria = cmbCategoria.Text.Trim(); // Aquí toma lo que el usuario escribió o seleccionó
            Producto.Precio = numPrecio.Value;
            Producto.CantidadDisponible = (int)numCantidad.Value;
            Producto.FechaCaducidad = chkNoAplicaFecha.Checked ? (DateTime?)null : dtpFechaCaducidad.Value;

            // 3. Guardar en Base de Datos usando la BLL
            try
            {
                if (Producto.IdProducto == 0)
                {
                    // Es un producto NUEVO -> Insertar
                    _productoBLL.AgregarProducto(Producto);
                    MessageBox.Show("Producto guardado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Es un producto EXISTENTE -> Actualizar
                    _productoBLL.ActualizarProducto(Producto);
                    MessageBox.Show("Producto actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Establecemos DialogResult en OK para que el formulario padre sepa que se guardó
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error al guardar en la base de datos:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // No cerramos el formulario para permitir al usuario corregir
            }
        }
    }
}