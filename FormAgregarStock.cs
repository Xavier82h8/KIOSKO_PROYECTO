using System;
using System.Drawing;
using System.Windows.Forms;
using KIOSKO_Proyecto.Modelos;
using KIOSKO_Proyecto.Datos;

namespace KIOSKO_Proyecto
{
    // Clase limpia sin 'partial' para evitar conflictos con el diseñador automático
    public class FormAgregarStock : Form
    {
        // Controles
        private TextBox txtIdProducto;
        private NumericUpDown numCantidad;
        private NumericUpDown numCostoTotal;
        private TextBox txtProveedor;
        private ComboBox cmbMetodoPago;
        private Button btnGuardar;
        private Button btnCancelar;

        public FormAgregarStock()
        {
            this.Text = "Registrar Compra a Proveedor";
            this.Size = new Size(400, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Construimos la interfaz manualmente para control total
            ConstruirInterfaz();
        }

        private void ConstruirInterfaz()
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Top, Height = 350, Padding = new Padding(20), RowCount = 6, ColumnCount = 2 };

            // 1. ID Producto
            layout.Controls.Add(new Label { Text = "ID Producto:", AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 10) }, 0, 0);
            txtIdProducto = new TextBox { Width = 200, Font = new Font("Segoe UI", 10) };
            layout.Controls.Add(txtIdProducto, 1, 0);

            // 2. Cantidad
            layout.Controls.Add(new Label { Text = "Cantidad (+):", AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 10) }, 0, 1);
            numCantidad = new NumericUpDown { Width = 200, Minimum = 1, Maximum = 10000, Font = new Font("Segoe UI", 10) };
            layout.Controls.Add(numCantidad, 1, 1);

            // 3. Costo Total
            layout.Controls.Add(new Label { Text = "Costo Total ($):", AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 10) }, 0, 2);
            numCostoTotal = new NumericUpDown { Width = 200, DecimalPlaces = 2, Maximum = 1000000, Font = new Font("Segoe UI", 10) };
            layout.Controls.Add(numCostoTotal, 1, 2);

            // 4. Proveedor
            layout.Controls.Add(new Label { Text = "Proveedor:", AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 10) }, 0, 3);
            txtProveedor = new TextBox { Width = 200, Font = new Font("Segoe UI", 10), MaxLength = 100 }; // Límite SQL
            layout.Controls.Add(txtProveedor, 1, 3);

            // 5. Método Pago
            layout.Controls.Add(new Label { Text = "Método Pago:", AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 10) }, 0, 4);
            cmbMetodoPago = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            // Estos valores ahora caben perfectamente en tu VARCHAR(50)
            cmbMetodoPago.Items.AddRange(new object[] { "Efectivo", "Tarjeta de Débito", "Tarjeta de Crédito", "Transferencia" });
            cmbMetodoPago.SelectedIndex = 0;
            layout.Controls.Add(cmbMetodoPago, 1, 4);

            this.Controls.Add(layout);

            // Botones
            var panelBtn = new Panel { Dock = DockStyle.Bottom, Height = 70 };
            btnGuardar = new Button { Text = "💾 Registrar", Location = new Point(80, 15), Width = 110, Height = 35, BackColor = Color.SeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCancelar = new Button { Text = "❌ Cancelar", Location = new Point(210, 15), Width = 100, Height = 35, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };

            btnGuardar.Click += BtnGuardar_Click;
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            panelBtn.Controls.Add(btnGuardar);
            panelBtn.Controls.Add(btnCancelar);
            this.Controls.Add(panelBtn);
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            // --- VALIDACIONES DE INTERFAZ ---
            if (!int.TryParse(txtIdProducto.Text, out int idProd))
            {
                MessageBox.Show("El ID del Producto debe ser un número.", "Error de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (numCantidad.Value <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a 0.", "Error de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validación de longitud (Vital para SQL)
            if (txtProveedor.Text.Length > 100)
            {
                MessageBox.Show("El nombre del proveedor es muy largo (Máx 100 caracteres).", "Error de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Crear objeto con datos
                var movimiento = new Inventario
                {
                    IdProducto = idProd,
                    Cantidad = (int)numCantidad.Value,
                    FechaRegistro = DateTime.Now,
                    Observaciones = "Compra Stock - Entrada Manual",
                    Proveedor = string.IsNullOrWhiteSpace(txtProveedor.Text) ? "Genérico" : txtProveedor.Text,

                    // Datos financieros
                    CostoTotal = numCostoTotal.Value,
                    MetodoPago = cmbMetodoPago.SelectedItem.ToString()
                };

                // Llamar al DAL (Aquí se hace la transacción SQL)
                bool exito = new InventarioDAL().RegistrarEntrada(movimiento);

                if (exito)
                {
                    MessageBox.Show("✅ Entrada registrada correctamente.\n\n- Stock actualizado\n- Pago registrado en caja", "Operación Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error al guardar en base de datos:\n{ex.Message}", "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}