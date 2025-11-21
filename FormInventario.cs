using System;
using System.Drawing;
using System.Windows.Forms;
using KIOSKO_Proyecto.BLL;
using KIOSKO_Proyecto.Modelos;
using Microsoft.VisualBasic; // Asegúrate de tener esta referencia agregada (Click derecho en Referencias -> Agregar -> Microsoft.VisualBasic)

namespace KIOSKO_Proyecto
{
    public partial class FormInventario : Form
    {
        private ProductoBLL productoBLL = new ProductoBLL();
        private Empleado _empleado;

        // Definición de controles manuales (Si el diseñador falla o no existe)
        private DataGridView dgvInventario;
        private Button btnAgregarStock;
        private TextBox txtBuscar;

        public FormInventario(Empleado empleado)
        {
            // 1. Llama al diseñador automático (si existe)
            InitializeComponent();

            // 2. Configuración extra manual
            _empleado = empleado;
            this.Text = $"Gestión de Inventario - Usuario: {_empleado.NombreEmp}";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(1000, 600); // Tamaño inicial cómodo

            // 3. Inicialización defensiva de controles
            InicializarControlesManuales();

            ConfigurarGrid();
            CargarInventario();
        }

        // Método para crear controles si el diseñador no lo hizo
        private void InicializarControlesManuales()
        {
            // Si el Grid ya fue creado por el InitializeComponent automático, no hacemos nada
            if (this.dgvInventario != null) return;

            this.dgvInventario = new DataGridView();
            this.btnAgregarStock = new Button();
            this.txtBuscar = new TextBox();

            // Panel Superior
            Panel panelTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(10) };

            txtBuscar.Text = "Buscar producto...";
            txtBuscar.Location = new Point(20, 20);
            txtBuscar.Width = 200;
            txtBuscar.TextChanged += (s, e) => CargarInventario(txtBuscar.Text);
            txtBuscar.Enter += (s, e) => { if (txtBuscar.Text == "Buscar producto...") txtBuscar.Text = ""; };
            txtBuscar.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtBuscar.Text)) txtBuscar.Text = "Buscar producto..."; };

            btnAgregarStock.Text = "+ Agregar Stock";
            btnAgregarStock.Location = new Point(250, 18);
            btnAgregarStock.Width = 150;
            btnAgregarStock.Height = 30;
            btnAgregarStock.BackColor = Color.SeaGreen;
            btnAgregarStock.ForeColor = Color.White;
            btnAgregarStock.FlatStyle = FlatStyle.Flat;
            btnAgregarStock.Click += BtnAgregarStock_Click;

            panelTop.Controls.Add(txtBuscar);
            panelTop.Controls.Add(btnAgregarStock);

            // Grid
            dgvInventario.Dock = DockStyle.Fill;
            dgvInventario.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvInventario.BackgroundColor = Color.White;
            dgvInventario.ReadOnly = true;
            dgvInventario.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvInventario.RowHeadersVisible = false;

            this.Controls.Add(dgvInventario);
            this.Controls.Add(panelTop);
        }

        private void ConfigurarGrid()
        {
            // Limpiamos para asegurar configuración limpia
            dgvInventario.Columns.Clear();

            // Definición de Columnas
            dgvInventario.Columns.Add("Id", "ID");
            dgvInventario.Columns.Add("Producto", "Producto");

            // Columna Cantidad
            var colCantidad = new DataGridViewTextBoxColumn { Name = "Cantidad", HeaderText = "Movimiento" };
            colCantidad.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvInventario.Columns.Add(colCantidad);

            // Columna Fecha con Formato
            var colFecha = new DataGridViewTextBoxColumn { Name = "Fecha", HeaderText = "Fecha Registro" };
            colFecha.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm"; // Formato amigable
            dgvInventario.Columns.Add(colFecha);

            dgvInventario.Columns.Add("Tipo", "Observaciones");

            // Ocultar ID visualmente
            dgvInventario.Columns[0].Visible = false;
        }

        private void CargarInventario(string filtro = "")
        {
            try
            {
                var datos = new Datos.InventarioDAL().ObtenerHistorialInventario();
                dgvInventario.Rows.Clear();

                foreach (var item in datos)
                {
                    // Filtro de búsqueda
                    bool coincide = string.IsNullOrEmpty(filtro) ||
                                    filtro == "Buscar producto..." ||
                                    item.NombreProducto.ToLower().Contains(filtro.ToLower());

                    if (coincide)
                    {
                        dgvInventario.Rows.Add(
                            item.IdInventario,
                            item.NombreProducto,
                            item.Cantidad,
                            item.FechaRegistro,
                            item.Observaciones
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                // Evitamos crash si no hay datos
                MessageBox.Show("Error cargando inventario: " + ex.Message, "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAgregarStock_Click(object sender, EventArgs e)
        {
            // Lógica rápida usando InputBox para no crear otro Form
            // REQUISITO: Agregar Referencia a Microsoft.VisualBasic
            try
            {
                string idStr = Interaction.InputBox("Ingresa el ID del Producto al que agregarás inventario:", "Agregar Stock Rápido");
                if (string.IsNullOrEmpty(idStr)) return; // Cancelado

                if (!int.TryParse(idStr, out int idProd))
                {
                    MessageBox.Show("ID inválido.");
                    return;
                }

                string cantStr = Interaction.InputBox("Cantidad a ingresar (Positivo para entradas):", "Agregar Stock Rápido");
                if (string.IsNullOrEmpty(cantStr)) return;

                if (!int.TryParse(cantStr, out int cantidad) || cantidad <= 0)
                {
                    MessageBox.Show("Cantidad inválida.");
                    return;
                }

                string obs = Interaction.InputBox("Observaciones (Opcional):", "Agregar Stock", "Entrada Manual");

                var movimiento = new Inventario
                {
                    IdProducto = idProd,
                    Cantidad = cantidad,
                    FechaRegistro = DateTime.Now,
                    Observaciones = obs,
                    Proveedor = "Interno"
                };

                bool exito = new Datos.InventarioDAL().RegistrarEntrada(movimiento);

                if (exito)
                {
                    MessageBox.Show("Stock actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarInventario(); // Refrescar la tabla
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message);
            }
        }
    }
}