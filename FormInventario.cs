using System;
using System.Drawing;
using System.Windows.Forms;
using KIOSKO_Proyecto.BLL;
using KIOSKO_Proyecto.Modelos;

namespace KIOSKO_Proyecto
{
    public partial class FormInventario : Form
    {
        private ProductoBLL productoBLL = new ProductoBLL();
        private Empleado _empleado;

        // Definición de controles (Si te marca error diciendo que "ya existen", borra estas 3 líneas)
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

            // 3. Intentamos inicializar controles si el diseñador estaba vacío
            InicializarControlesManuales();

            ConfigurarGrid();
            CargarInventario();
        }

        // HE RENOMBRADO ESTE MÉTODO PARA QUITAR EL ERROR
        private void InicializarControlesManuales()
        {
            // Si el Grid ya fue creado por el InitializeComponent automático, no hacemos nada
            if (this.dgvInventario != null) return;

            // Si no existe, lo creamos manualmente
            this.dgvInventario = new DataGridView();
            this.btnAgregarStock = new Button();
            this.txtBuscar = new TextBox();

            // Panel Superior
            Panel panelTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(10) };

            txtBuscar.Text = "Buscar producto..."; // Placeholder simple
            txtBuscar.Location = new Point(20, 20);
            txtBuscar.Width = 200;
            txtBuscar.TextChanged += (s, e) => CargarInventario(txtBuscar.Text);
            txtBuscar.Enter += (s, e) => { if (txtBuscar.Text == "Buscar producto...") txtBuscar.Text = ""; };

            btnAgregarStock.Text = "+ Agregar Stock";
            btnAgregarStock.Location = new Point(250, 18);
            btnAgregarStock.Width = 150;
            btnAgregarStock.BackColor = Color.SeaGreen;
            btnAgregarStock.ForeColor = Color.White;
            btnAgregarStock.Click += BtnAgregarStock_Click;

            panelTop.Controls.Add(txtBuscar);
            panelTop.Controls.Add(btnAgregarStock);

            // Grid
            dgvInventario.Dock = DockStyle.Fill;
            dgvInventario.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvInventario.BackgroundColor = Color.White;
            dgvInventario.ReadOnly = true;

            this.Controls.Add(dgvInventario);
            this.Controls.Add(panelTop);
        }

        private void ConfigurarGrid()
        {
            // Aseguramos que el grid tenga columnas
            if (dgvInventario.Columns.Count == 0)
            {
                dgvInventario.Columns.Add("Id", "ID");
                dgvInventario.Columns.Add("Producto", "Producto");
                dgvInventario.Columns.Add("Cantidad", "Cantidad Movimiento");
                dgvInventario.Columns.Add("Fecha", "Fecha");
                dgvInventario.Columns.Add("Tipo", "Observaciones");
            }
        }

        private void CargarInventario(string filtro = "")
        {
            try
            {
                var datos = new Datos.InventarioDAL().ObtenerHistorialInventario();
                dgvInventario.Rows.Clear();
                foreach (var item in datos)
                {
                    if (string.IsNullOrEmpty(filtro) || filtro == "Buscar producto..." || item.NombreProducto.ToLower().Contains(filtro.ToLower()))
                    {
                        dgvInventario.Rows.Add(item.IdInventario, item.NombreProducto, item.Cantidad, item.FechaRegistro, item.Observaciones);
                    }
                }
            }
            catch (Exception ex)
            {
                // Evitamos que el form falle si la DB no responde, solo mostramos el error
                MessageBox.Show("Error cargando inventario: " + ex.Message);
            }
        }

        private void BtnAgregarStock_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Para agregar stock, utiliza el módulo de Compras o Ajustes (Pendiente de implementación).", "Info");
        }
    }
}