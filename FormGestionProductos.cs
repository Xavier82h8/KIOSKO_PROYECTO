using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using KIOSKO_Proyecto.Modelos;
using KIOSKO_Proyecto.BLL;

namespace KIOSKO_Proyecto
{
    public class FormGestionProductos : Form
    {
        private ProductoBLL _productoBLL = new ProductoBLL();
        private DataGridView dgvProductos;
        private Button btnAgregar;
        private Button btnEditar;
        private Button btnEliminar;

        public FormGestionProductos()
        {
            this.Text = "Gestión de Productos";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new System.Drawing.Size(800, 600);
            ConfigurarFormulario();
            DefinirColumnasGrid();
            CargarProductos();
        }

        private void ConfigurarFormulario()
        {
            // Configuración del DataGridView
            dgvProductos = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 35 },
                AllowUserToResizeRows = false
            };

            // Estilo del header
            dgvProductos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvProductos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProductos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvProductos.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Estilo de las celdas
            dgvProductos.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvProductos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgvProductos.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvProductos.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);

            this.Controls.Add(dgvProductos);

            // Panel para botones
            Panel panelBotones = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(41, 128, 185)
            };
            this.Controls.Add(panelBotones);

            btnAgregar = new Button
            {
                Text = "➕ Agregar Producto",
                Location = new Point(20, 10),
                Size = new Size(150, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAgregar.FlatAppearance.BorderSize = 0;
            btnAgregar.Click += BtnAgregar_Click;
            panelBotones.Controls.Add(btnAgregar);

            btnEditar = new Button
            {
                Text = "✏️ Editar Producto",
                Location = new Point(180, 10),
                Size = new Size(150, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnEditar.FlatAppearance.BorderSize = 0;
            btnEditar.Click += BtnEditar_Click;
            panelBotones.Controls.Add(btnEditar);

            btnEliminar = new Button
            {
                Text = "🗑️ Eliminar Producto",
                Location = new Point(340, 10),
                Size = new Size(150, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnEliminar.FlatAppearance.BorderSize = 0;
            btnEliminar.Click += BtnEliminar_Click;
            panelBotones.Controls.Add(btnEliminar);
        }

        private void DefinirColumnasGrid()
        {
            dgvProductos.Columns.Clear();
            dgvProductos.Columns.Add("IdProducto", "ID");
            dgvProductos.Columns.Add("Nombre", "Nombre");
            dgvProductos.Columns.Add("Categoria", "Categoría");
            dgvProductos.Columns.Add("Precio", "Precio");
            dgvProductos.Columns.Add("CantidadDisponible", "Stock");
            dgvProductos.Columns.Add("FechaCaducidad", "Fecha Caducidad");

            dgvProductos.Columns["IdProducto"].Visible = false;
            dgvProductos.Columns["Precio"].DefaultCellStyle.Format = "C2";
        }

        private void CargarProductos()
        {
            try
            {
                dgvProductos.Rows.Clear();
                List<Producto> productos = _productoBLL.ObtenerTodosLosProductos();

                if (productos != null)
                {
                    foreach (var producto in productos)
                    {
                        dgvProductos.Rows.Add(
                            producto.IdProducto,
                            producto.Nombre,
                            producto.Categoria,
                            producto.Precio,
                            producto.CantidadDisponible,
                            producto.FechaCaducidad.HasValue ? producto.FechaCaducidad.Value.ToShortDateString() : "N/A"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los productos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            using (var form = new FormularioProducto())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _productoBLL.AgregarProducto(form.Producto);
                        MessageBox.Show("Producto agregado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CargarProductos();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al agregar el producto: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count > 0)
            {
                try
                {
                    int idProducto = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["IdProducto"].Value);
                    Producto productoAEditar = _productoBLL.ObtenerProductoPorId(idProducto);

                    if (productoAEditar == null)
                    {
                        MessageBox.Show("El producto seleccionado ya no existe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        CargarProductos();
                        return;
                    }

                    using (var form = new FormularioProducto(productoAEditar))
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            _productoBLL.ActualizarProducto(form.Producto);
                            MessageBox.Show("Producto actualizado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            CargarProductos();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al editar el producto: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Seleccione un producto para editar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count > 0)
            {
                try
                {
                    int idProducto = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["IdProducto"].Value);
                    string nombreProducto = dgvProductos.SelectedRows[0].Cells["Nombre"].Value.ToString();

                    if (MessageBox.Show($"¿Está seguro de que desea eliminar el producto '{nombreProducto}'?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        _productoBLL.EliminarProducto(idProducto);
                        MessageBox.Show("Producto eliminado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CargarProductos();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar el producto: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Seleccione un producto para eliminar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
