using System;
using System.Drawing;
using System.Windows.Forms;
using KIOSKO_Proyecto.BLL;
using KIOSKO_Proyecto.Modelos;
// Ya no necesitamos Microsoft.VisualBasic aquí porque usamos el formulario robusto

namespace KIOSKO_Proyecto
{
    public partial class FormInventario : Form
    {
        private ProductoBLL productoBLL = new ProductoBLL();
        private Empleado _empleado;

        // Definición de controles manuales (Protección contra fallos del diseñador)
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
            this.Size = new Size(1000, 600);

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
            dgvInventario.Columns.Add("Prov", "Proveedor"); // Agregamos proveedor al grid

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
                            item.Observaciones,
                            item.Proveedor
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
            // Usamos el nuevo formulario ROBUSTO que incluye costos y proveedor
            try
            {
                using (var form = new FormAgregarStock())
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        // Si guardó bien, recargamos la tabla para ver el cambio
                        CargarInventario();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir formulario de stock: " + ex.Message);
            }
        }
    }
}