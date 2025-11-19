using System;
using System.Windows.Forms;
using KIOSKO_Proyecto.BLL;
using KIOSKO_Proyecto.Modelos;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;

namespace KIOSKO_Proyecto
{
    public partial class FormInventario : Form
    {
        private ProductoBLL _productoBLL = new ProductoBLL(); // Changed to ProductoBLL
        private DataGridView dgvProductos; // Renamed to dgvProductos
        private Button btnNuevoProducto;
        private Button btnEditarProducto;
        private Button btnEliminarProducto;

        public FormInventario()
        {
            this.Text = "Gestión de Productos e Inventario"; // Updated title
            this.Size = new System.Drawing.Size(1000, 700); // Increased form size
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Panel para controles de entrada y botones
            var panelControles = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50, // Reduced height
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10)
            };

            // Buttons for CRUD operations
            btnNuevoProducto = new Button { Text = "Nuevo Producto", Location = new Point(10, 10), Width = 120, Height = 30, BackColor = Color.FromArgb(92, 184, 92), ForeColor = Color.White };
            btnEditarProducto = new Button { Text = "Editar Producto", Location = new Point(140, 10), Width = 120, Height = 30, BackColor = Color.FromArgb(91, 192, 222), ForeColor = Color.White };
            btnEliminarProducto = new Button { Text = "Eliminar Producto", Location = new Point(270, 10), Width = 120, Height = 30, BackColor = Color.FromArgb(217, 83, 79), ForeColor = Color.White };

            // Add controls to panelControles
            panelControles.Controls.Add(btnNuevoProducto);
            panelControles.Controls.Add(btnEditarProducto);
            panelControles.Controls.Add(btnEliminarProducto);

            this.Controls.Add(panelControles);

            dgvProductos = new DataGridView // Renamed
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                TabIndex = 0,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };
            this.Controls.Add(dgvProductos);
            dgvProductos.BringToFront(); // Ensure DGV is on top of panelControles

            ConfigurarDgvProductos(); // Configure columns for products
            CargarProductos(); // Load products

            // Event Handlers
            btnNuevoProducto.Click += BtnNuevoProducto_Click;
            btnEditarProducto.Click += BtnEditarProducto_Click;
            btnEliminarProducto.Click += BtnEliminarProducto_Click;
        }

        private void ConfigurarDgvProductos() // Renamed
        {
            dgvProductos.Columns.Clear();
            dgvProductos.Columns.Add("IdProducto", "ID");
            dgvProductos.Columns.Add("Nombre", "Nombre");
            dgvProductos.Columns.Add("Categoria", "Categoría");
            dgvProductos.Columns.Add("Precio", "Precio");
            dgvProductos.Columns.Add("CantidadDisponible", "Stock");

            dgvProductos.Columns["IdProducto"].Width = 50;
            dgvProductos.Columns["Precio"].DefaultCellStyle.Format = "C2";
            dgvProductos.Columns["Precio"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvProductos.Columns["CantidadDisponible"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void CargarProductos() // Renamed
        {
            dgvProductos.Rows.Clear();
            var productos = _productoBLL.ObtenerProductos(); // Using ProductoBLL
            foreach (var p in productos)
            {
                dgvProductos.Rows.Add(p.IdProducto, p.Nombre, p.Categoria, p.Precio, p.CantidadDisponible);
            }
        }

        

        

        private void BtnNuevoProducto_Click(object sender, EventArgs e)
        {
            using (FormularioProducto formProducto = new FormularioProducto())
            {
                if (formProducto.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _productoBLL.AgregarProducto(formProducto.Producto);
                        MessageBox.Show("Producto agregado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CargarProductos();
                        LimpiarCampos();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al agregar producto: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnEditarProducto_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione un producto para editar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idProducto = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["IdProducto"].Value);
            Producto productoAEditar = _productoBLL.ObtenerProductoPorId(idProducto);

            if (productoAEditar == null)
            {
                MessageBox.Show("No se pudo encontrar el producto seleccionado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (FormularioProducto formProducto = new FormularioProducto(productoAEditar))
            {
                if (formProducto.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _productoBLL.ActualizarProducto(formProducto.Producto);
                        MessageBox.Show("Producto actualizado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CargarProductos();
                        LimpiarCampos();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al actualizar producto: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnEliminarProducto_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione un producto para eliminar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("¿Está seguro de que desea eliminar el producto seleccionado?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    int idProducto = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["IdProducto"].Value);
                    _productoBLL.EliminarProducto(idProducto);
                    MessageBox.Show("Producto eliminado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarProductos();
                    LimpiarCampos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar producto: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LimpiarCampos()
        {
            // No input fields to clear in this form.
        }
    }
}
