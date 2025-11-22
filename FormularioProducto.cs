using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using KIOSKO_Proyecto.Modelos;
using KIOSKO_Proyecto.BLL;

namespace KIOSKO_Proyecto
{
    public class FormularioProducto : Form
    {
        public Modelos.Empleado EmpleadoAutenticado { get; set; }
        public Producto Producto { get; private set; }

        // Lógica de negocio
        private ProductoBLL _productoBLL;

        // Controles
        private TextBox txtNombre;
        private ComboBox cmbCategoria;
        private NumericUpDown numPrecio;
        private NumericUpDown numCantidad;
        private DateTimePicker dtpFechaCaducidad;
        private CheckBox chkNoAplicaFecha;

        // Botones
        private Button btnEliminar; // NUEVO BOTÓN
        private Button btnGuardar;
        private Button btnCancelar;

        public FormularioProducto(Producto producto = null)
        {
            _productoBLL = new ProductoBLL();
            this.Producto = producto ?? new Producto();

            InitializeComponent();
            CargarCategorias();

            // Configuración según si es NUEVO o EDICIÓN
            if (this.Producto.IdProducto > 0)
            {
                this.Text = "Editar Producto";
                CargarDatosProducto();
                btnEliminar.Visible = true; // Mostrar eliminar solo si existe
            }
            else
            {
                this.Text = "Agregar Producto";
                btnEliminar.Visible = false; // Ocultar si es nuevo
            }
        }

        private void InitializeComponent()
        {
            this.Size = new Size(500, 550);
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

            // --- CAMPOS ---

            // 1. Nombre
            AgregarControl(tableLayout, "Nombre:", txtNombre = new TextBox { Font = new Font("Segoe UI", 10) }, 0);

            // 2. Categoría (ComboBox editable)
            cmbCategoria = new ComboBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems,
                DropDownStyle = ComboBoxStyle.DropDown
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

            // --- BOTONES ---
            var panelBotones = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            // Botón Cancelar
            btnCancelar = new Button
            {
                Text = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Width = 100,
                Height = 40,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.Gainsboro,
                FlatStyle = FlatStyle.Flat
            };
            btnCancelar.FlatAppearance.BorderSize = 0;

            // Botón Guardar
            btnGuardar = new Button
            {
                Text = "Guardar",
                Width = 100,
                Height = 40,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(46, 204, 113), // Verde
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;

            // Botón Eliminar (Rojo)
            btnEliminar = new Button
            {
                Text = "Eliminar",
                Width = 100,
                Height = 40,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(231, 76, 60), // Rojo
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 10, 0) // Separación
            };
            btnEliminar.FlatAppearance.BorderSize = 0;
            btnEliminar.Click += BtnEliminar_Click;

            // Añadimos los botones al panel (orden inverso por RightToLeft)
            panelBotones.Controls.Add(btnCancelar);
            panelBotones.Controls.Add(btnGuardar);
            panelBotones.Controls.Add(btnEliminar); // Aparecerá a la izquierda de Guardar

            tableLayout.Controls.Add(panelBotones, 0, 6);
            tableLayout.SetColumnSpan(panelBotones, 2);

            // Ajustes de filas
            for (int i = 0; i < 6; i++) tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            this.Controls.Add(tableLayout);
        }

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
                List<string> categorias = _productoBLL.ObtenerCategorias();
                cmbCategoria.Items.Clear();
                cmbCategoria.Items.AddRange(categorias.ToArray());
            }
            catch { /* Ignorar errores de carga visual */ }
        }

        private void CargarDatosProducto()
        {
            txtNombre.Text = Producto.Nombre;
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

        // ============================================================
        // ACCIÓN: GUARDAR / EDITAR
        // ============================================================
        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(cmbCategoria.Text))
            {
                MessageBox.Show("Nombre y Categoría son obligatorios.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Asignar valores
            Producto.Nombre = txtNombre.Text.Trim();
            Producto.Categoria = cmbCategoria.Text.Trim();
            Producto.Precio = numPrecio.Value;
            Producto.CantidadDisponible = (int)numCantidad.Value;
            Producto.FechaCaducidad = chkNoAplicaFecha.Checked ? (DateTime?)null : dtpFechaCaducidad.Value;

            try
            {
                if (Producto.IdProducto == 0)
                {
                    // --- INSERTAR ---
                    _productoBLL.AgregarProducto(Producto);
                    MessageBox.Show("Producto creado exitosamente.");
                }
                else
                {
                    // --- EDITAR (ACTUALIZAR) ---
                    _productoBLL.ActualizarProducto(Producto);
                    MessageBox.Show("Producto actualizado exitosamente.");
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================================
        // ACCIÓN: ELIMINAR
        // ============================================================
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            var respuesta = MessageBox.Show(
                $"¿Estás seguro de ELIMINAR el producto '{Producto.Nombre}'?\nEsta acción no se puede deshacer.",
                "Confirmar Eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Stop);

            if (respuesta == DialogResult.Yes)
            {
                try
                {
                    _productoBLL.EliminarProducto(Producto.IdProducto);
                    MessageBox.Show("Producto eliminado correctamente.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Cerramos con OK para que la lista principal se actualice
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo eliminar. Es probable que este producto tenga ventas asociadas.\n\nDetalle técnico: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}