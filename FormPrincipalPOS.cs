using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using KIOSKO_Proyecto.Modelos;
using KIOSKO_Proyecto.BLL;

namespace KIOSKO_Proyecto
{
    public partial class FormPrincipalPOS : Form
    {
        private ProductoBLL productoBLL = new ProductoBLL();
        private VentaBLL ventaBLL = new VentaBLL();
        private EmpleadoBLL empleadoBLL = new EmpleadoBLL();

        private List<DetalleVenta> carrito = new List<DetalleVenta>();
        private Modelos.Empleado _empleadoAutenticado;

        // --- Controles principales ---
        private DataGridView dgvProductos;
        private DataGridView dgvCarrito;
        private TextBox txtBuscar;
        private ComboBox cmbCategoria;
        private Label lblTotal;
        private Label lblEmpleado;
        private Button btnProductos;
        private Button btnGestionInventario;
        private Button btnVerReportes;
        private ContextMenuStrip menuUsuario;
        private Label lblItemsCarrito;
        private Button btnEliminar;
        private Button btnLimpiar;

        // --- Nuevos Controles de Pago ---
        private TextBox txtMontoEfectivo;
        private TextBox txtMontoTarjeta;
        private Label lblCambio;
        private Button btnCobrar;
        private Button btnTicketTemporal;


        public FormPrincipalPOS(Modelos.Empleado empleado)
        {
            _empleadoAutenticado = empleado;
            this.DoubleBuffered = true;
            InicializarComponentes();
            ConfigurarFormulario();
            InitializeUserInterface();
            CargarProductos();
        }

        private void InicializarComponentes()
        {
            this.SuspendLayout();
            this.Text = "Kioskito ITH - Sistema POS";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(248, 249, 250);
            this.FormBorderStyle = FormBorderStyle.None;

            var panelSuperior = CrearPanelSuperior();
            var panelCheckout = CrearPanelCheckout();
            panelCheckout.Dock = DockStyle.Fill;
            var panelProductos = CrearPanelProductos();
            panelProductos.Dock = DockStyle.Left;
            panelProductos.Width = (int)(this.Width * 0.65);

            this.Controls.Add(panelCheckout);
            this.Controls.Add(panelProductos);
            this.Controls.Add(panelSuperior);

            CrearMenuUsuario();
            ConfigurarEventos();
            this.ResumeLayout(false);
        }

        private void CrearMenuUsuario()
        {
            menuUsuario = new ContextMenuStrip();
            var itemCerrarSesion = menuUsuario.Items.Add("Cerrar sesión");
            itemCerrarSesion.Click += BtnCerrarSesion_Click;
        }

        private Panel CrearPanelSuperior()
        {
            var panel = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Color.White, Padding = new Padding(10) };
            panel.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 220, 220), 1), 0, panel.Height - 1, panel.Width, panel.Height - 1);

            // --- Panel Izquierdo (Título y Módulos) ---
            var panelIzquierdo = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Padding = new Padding(10, 0, 10, 0),
                Margin = new Padding(0),
                WrapContents = false
            };
            panel.Controls.Add(panelIzquierdo);

            var lblTitulo = new Label { Text = "🛒 Kioskito ITH", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(45, 140, 200), AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 15, 20, 0) };
            panelIzquierdo.Controls.Add(lblTitulo);

            btnProductos = CrearBotonModulo("📦 Productos", Color.FromArgb(91, 192, 222));
            btnGestionInventario = CrearBotonModulo("📥 Inventario", Color.FromArgb(240, 173, 78));
            btnVerReportes = CrearBotonModulo("📈 Reportes", Color.FromArgb(45, 140, 200));
            panelIzquierdo.Controls.AddRange(new Control[] { btnProductos, btnGestionInventario, btnVerReportes });


            // --- Panel Derecho (Búsqueda y Usuario) ---
            var panelDerecho = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Padding = new Padding(10, 0, 10, 0),
                Margin = new Padding(0),
                WrapContents = false
            };
            panel.Controls.Add(panelDerecho);

            // Contenedor para Usuario
            var panelUsuario = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Margin = new Padding(10, 10, 0, 0) };
            lblEmpleado = new Label { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(45, 140, 200), TextAlign = ContentAlignment.MiddleLeft, AutoSize = true, Margin = new Padding(0, 5, 10, 0), Cursor = Cursors.Hand };
            panelUsuario.Controls.Add(lblEmpleado);
            panelDerecho.Controls.Add(panelUsuario);

            // Contenedor para Búsqueda
            cmbCategoria = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11), FlatStyle = FlatStyle.Flat, Margin = new Padding(10, 12, 0, 0) };

            var panelWrapperBusqueda = new Panel { Size = new Size(220, 45), BackColor = this.BackColor, Margin = new Padding(10, 8, 0, 0) };
            panelWrapperBusqueda.Paint += (s, e) => PintarBordeRedondeado(e.Graphics, panelWrapperBusqueda, Color.FromArgb(200, 200, 200), 8);
            var iconoBuscar = new Label { Text = "🔍", Font = new Font("Segoe UI", 14), Location = new Point(10, 10), Size = new Size(30, 25), BackColor = Color.Transparent };
            txtBuscar = new TextBox { Location = new Point(45, 12), Width = 160, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 12), BackColor = this.BackColor, ForeColor = Color.FromArgb(50, 50, 50) };
            txtBuscar.Text = "Buscar productos...";
            txtBuscar.ForeColor = Color.Gray;
            txtBuscar.Enter += (s, e) => { if (txtBuscar.Text == "Buscar productos...") { txtBuscar.Text = ""; txtBuscar.ForeColor = Color.Black; } };
            txtBuscar.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtBuscar.Text)) { txtBuscar.Text = "Buscar productos..."; txtBuscar.ForeColor = Color.Gray; } };
            panelWrapperBusqueda.Controls.AddRange(new Control[] { iconoBuscar, txtBuscar });

            panelDerecho.Controls.Add(cmbCategoria);
            panelDerecho.Controls.Add(panelWrapperBusqueda);

            return panel;
        }

        private Panel CrearPanelProductos()
        {
            var panel = new Panel { Padding = new Padding(20, 20, 10, 20), BackColor = this.BackColor };
            var lblTitulo = new Label { Text = "Productos Disponibles", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(50, 50, 50), Dock = DockStyle.Top, Height = 40 };

            dgvProductos = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10), RowTemplate = { Height = 50 }, EnableHeadersVisualStyles = false, AllowUserToResizeRows = false };
            dgvProductos.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(45, 140, 200), ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), Alignment = DataGridViewContentAlignment.MiddleLeft, Padding = new Padding(10) };
            dgvProductos.ColumnHeadersHeight = 45;
            dgvProductos.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvProductos.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
            dgvProductos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(91, 192, 222);
            dgvProductos.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvProductos.DefaultCellStyle.Padding = new Padding(8);

            panel.Controls.Add(dgvProductos);
            panel.Controls.Add(lblTitulo);
            return panel;
        }

        private Panel CrearPanelCheckout()
        {
            var panel = new Panel { BackColor = Color.White, Padding = new Padding(10, 20, 20, 20) };
            panel.Paint += (s, e) => { using (var pen = new Pen(Color.FromArgb(20, 0, 0, 0), 1)) { e.Graphics.DrawLine(pen, 0, 0, 0, panel.Height); } };

            // --- Panel de Pago (Inferior) ---
            var panelPago = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 220, Padding = new Padding(10), ColumnCount = 2, RowCount = 4 };
            panelPago.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            panelPago.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            // Monto Efectivo
            panelPago.Controls.Add(new Label { Text = "💵 Monto Efectivo:", Font = new Font("Segoe UI", 10), Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
            txtMontoEfectivo = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12) };
            panelPago.Controls.Add(txtMontoEfectivo, 1, 0);

            // Monto Tarjeta
            panelPago.Controls.Add(new Label { Text = "💳 Monto Tarjeta:", Font = new Font("Segoe UI", 10), Anchor = AnchorStyles.Left, AutoSize = true }, 0, 1);
            txtMontoTarjeta = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12) };
            panelPago.Controls.Add(txtMontoTarjeta, 1, 1);

            // Cambio
            lblCambio = new Label { Text = "Cambio: $0.00", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.Blue, Anchor = AnchorStyles.Left, AutoSize = true };
            panelPago.SetColumnSpan(lblCambio, 2);
            panelPago.Controls.Add(lblCambio, 0, 2);

            // Botones
            btnCobrar = new Button { Text = "✓ Cobrar", Height = 50, BackColor = Color.FromArgb(92, 184, 92), ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Dock = DockStyle.Fill };
            btnTicketTemporal = new Button { Text = "📄 Ticket", Height = 50, BackColor = Color.FromArgb(240, 173, 78), ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Dock = DockStyle.Fill };
            panelPago.Controls.Add(btnCobrar, 0, 3);
            panelPago.Controls.Add(btnTicketTemporal, 1, 3);

            panel.Controls.Add(panelPago);

            // --- Panel de Total ---
            lblTotal = new Label { Dock = DockStyle.Bottom, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.DarkGreen, BackColor = Color.LightGray, TextAlign = ContentAlignment.MiddleCenter, Height = 60, Padding = new Padding(10), Margin = new Padding(10) };
            panel.Controls.Add(lblTotal);

            // --- Panel de Acciones del Carrito ---
            var panelAcciones = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Padding = new Padding(10) };
            btnEliminar = new Button { Text = "Eliminar", BackColor = Color.LightCoral, Height = 40, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnLimpiar = new Button { Text = "Limpiar Todo", BackColor = Color.LightCoral, Height = 40, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            panelAcciones.Controls.Add(btnEliminar);
            panelAcciones.Controls.Add(btnLimpiar);
            panel.Controls.Add(panelAcciones);

            // --- Carrito (Centro) ---
            var panelAreaCarrito = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            var lblTituloCarrito = new Label { Text = "🛒 Carrito de Compras", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(50, 50, 50), Dock = DockStyle.Top, Height = 40 };
            lblItemsCarrito = new Label { Text = "0 items", Font = new Font("Segoe UI", 11), ForeColor = Color.Gray, Dock = DockStyle.Top, Height = 30 };
            dgvCarrito = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10), RowTemplate = { Height = 55 }, EnableHeadersVisualStyles = false, GridColor = Color.FromArgb(230, 230, 230) };
            dgvCarrito.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(248, 249, 250), ForeColor = Color.FromArgb(80, 80, 80), Font = new Font("Segoe UI", 10, FontStyle.Bold), Alignment = DataGridViewContentAlignment.MiddleLeft, Padding = new Padding(8) };
            dgvCarrito.ColumnHeadersHeight = 40;
            dgvCarrito.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            panelAreaCarrito.Controls.Add(dgvCarrito);
            panelAreaCarrito.Controls.Add(lblItemsCarrito);
            panelAreaCarrito.Controls.Add(lblTituloCarrito);
            panel.Controls.Add(panelAreaCarrito);

            return panel;
        }

        private void PintarBordeRedondeado(Graphics g, Control control, Color borderColor, int radius)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = RoundedRect(new Rectangle(0, 0, control.Width - 1, control.Height - 1), radius)) { control.Region = new Region(path); using (var pen = new Pen(borderColor, 1)) { g.DrawPath(pen, path); } }
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            if (diameter > bounds.Width) diameter = bounds.Width;
            if (diameter > bounds.Height) diameter = bounds.Height;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void ConfigurarFormulario()
        {
            dgvProductos.Columns.Clear();
            dgvCarrito.Columns.Clear();
            dgvProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProductos.Columns.Add("ID", "ID");
            dgvProductos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nombre", HeaderText = "Producto", FillWeight = 40 });
            dgvProductos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Categoria", HeaderText = "Categoría", FillWeight = 25 });
            dgvProductos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Precio", HeaderText = "Precio", FillWeight = 20, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvProductos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Stock", HeaderText = "Stock", FillWeight = 15, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvProductos.Columns["ID"].Visible = false;
            dgvCarrito.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCarrito.Columns.Add("ID", "ID");
            dgvCarrito.Columns.Add(new DataGridViewTextBoxColumn { Name = "Producto", HeaderText = "Producto", ReadOnly = true, FillWeight = 40 });
            dgvCarrito.Columns.Add(new DataGridViewTextBoxColumn { Name = "Precio", HeaderText = "Precio", ReadOnly = true, FillWeight = 20, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvCarrito.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cantidad", HeaderText = "Cant.", FillWeight = 15, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvCarrito.Columns.Add(new DataGridViewTextBoxColumn { Name = "Subtotal", HeaderText = "Subtotal", ReadOnly = true, FillWeight = 25, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 11, FontStyle.Bold) } });
            dgvCarrito.Columns["ID"].Visible = false;
        }

        private void ConfigurarEventos()
        {
            txtBuscar.TextChanged += TxtBuscar_TextChanged;
            cmbCategoria.SelectedIndexChanged += CmbCategoria_SelectedIndexChanged;
            dgvProductos.CellDoubleClick += DgvProductos_CellDoubleClick;
            dgvProductos.CellFormatting += DgvProductos_CellFormatting;
            dgvCarrito.CellValueChanged += DgvCarrito_CellValueChanged;
            btnEliminar.Click += BtnEliminar_Click;
            btnLimpiar.Click += BtnLimpiar_Click;
            btnProductos.Click += BtnProductos_Click;
            btnGestionInventario.Click += BtnGestionInventario_Click;
            btnVerReportes.Click += BtnVerReportes_Click;
            lblEmpleado.Click += LblEmpleado_Click;

            // Nuevos eventos
            txtMontoEfectivo.TextChanged += TxtMontoEfectivo_TextChanged;
            btnCobrar.Click += BtnCobrar_Click;
            btnTicketTemporal.Click += BtnTicketTemporal_Click;
        }

        private void InitializeUserInterface()
        {
            lblEmpleado.Text = $"👤 {_empleadoAutenticado.NombreEmp} ({_empleadoAutenticado.Puesto})";
            string puesto = _empleadoAutenticado.Puesto ?? "";
            bool esAdmin = puesto.Equals("Administrador", StringComparison.OrdinalIgnoreCase);
            bool esGerente = puesto.Equals("Gerente", StringComparison.OrdinalIgnoreCase);
            bool esSupervisor = puesto.Equals("Supervisor", StringComparison.OrdinalIgnoreCase) || puesto.Equals("Supervisora", StringComparison.OrdinalIgnoreCase);
            btnProductos.Visible = esAdmin || esGerente || esSupervisor;
            btnGestionInventario.Visible = esAdmin || esGerente || esSupervisor;
            btnVerReportes.Visible = esAdmin || esGerente || esSupervisor;
        }

        private void CargarProductos()
        {
            dgvProductos.Rows.Clear();
            var productos = productoBLL.ObtenerProductosDisponibles();
            foreach (var p in productos) { dgvProductos.Rows.Add(p.IdProducto, p.Nombre, p.Categoria, p.Precio, p.CantidadDisponible); }
            cmbCategoria.Items.Clear();
            cmbCategoria.Items.Add("📋 Todas las categorías");
            var categorias = productoBLL.ObtenerCategorias();
            foreach (var cat in categorias) { cmbCategoria.Items.Add(cat); }
            cmbCategoria.SelectedIndex = 0;
        }

        private void FiltrarProductos()
        {
            var categoriaSeleccionada = cmbCategoria.SelectedIndex > 0 ? cmbCategoria.SelectedItem.ToString().Replace("📋 ", "") : null;
            var textoBusqueda = txtBuscar.Text == "Buscar productos..." ? "" : txtBuscar.Text;
            var productos = productoBLL.FiltrarProductos(textoBusqueda, categoriaSeleccionada);
            dgvProductos.Rows.Clear();
            foreach (var p in productos) { dgvProductos.Rows.Add(p.IdProducto, p.Nombre, p.Categoria, p.Precio, p.CantidadDisponible); }
        }

        private void DgvProductos_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvProductos.Columns.Contains("Stock") && e.ColumnIndex == dgvProductos.Columns["Stock"].Index && e.Value != null)
            {
                if (int.TryParse(e.Value.ToString(), out int stock))
                {
                    if (stock <= 5) { e.CellStyle.BackColor = Color.FromArgb(255, 230, 230); e.CellStyle.ForeColor = Color.DarkRed; e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold); }
                    else if (stock <= 15) { e.CellStyle.BackColor = Color.FromArgb(255, 250, 230); e.CellStyle.ForeColor = Color.DarkOrange; }
                    else { e.CellStyle.BackColor = Color.White; e.CellStyle.ForeColor = Color.Black; }
                }
            }
        }

        private void TxtBuscar_TextChanged(object sender, EventArgs e) => FiltrarProductos();
        private void CmbCategoria_SelectedIndexChanged(object sender, EventArgs e) => FiltrarProductos();

        private void DgvProductos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int idProducto = Convert.ToInt32(dgvProductos.Rows[e.RowIndex].Cells["ID"].Value);
            var producto = productoBLL.ObtenerProductoPorId(idProducto);
            if (producto != null) { AgregarAlCarrito(producto); }
        }

        private void AgregarAlCarrito(Producto producto)
        {
            var itemExistente = carrito.FirstOrDefault(c => c.ProductoID == producto.IdProducto);
            if (itemExistente != null)
            {
                var productoEnGrid = productoBLL.ObtenerProductoPorId(producto.IdProducto);
                if (itemExistente.Cantidad < productoEnGrid.CantidadDisponible) { itemExistente.Cantidad++; }
                else { MessageBox.Show("No hay más stock disponible", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            }
            else
            {
                if (producto.CantidadDisponible > 0) { carrito.Add(new DetalleVenta { ProductoID = producto.IdProducto, Cantidad = 1 }); }
                else { MessageBox.Show("Producto sin stock", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            }
            ActualizarCarrito();
        }

        private void ActualizarCarrito()
        {
            dgvCarrito.Rows.Clear();
            foreach (var item in carrito)
            {
                var producto = productoBLL.ObtenerProductoPorId(item.ProductoID);
                item.PrecioUnitario = producto.Precio;
                item.Subtotal = item.Cantidad * item.PrecioUnitario;
                dgvCarrito.Rows.Add(item.ProductoID, producto.Nombre, item.PrecioUnitario, item.Cantidad, item.Subtotal);
            }
            lblItemsCarrito.Text = $"{carrito.Sum(c => c.Cantidad)} items";
            ActualizarTotal();
        }

        private void DgvCarrito_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != dgvCarrito.Columns["Cantidad"].Index) return;
            try
            {
                int nuevaCantidad = Convert.ToInt32(dgvCarrito.Rows[e.RowIndex].Cells["Cantidad"].Value);
                var item = carrito[e.RowIndex];
                var producto = productoBLL.ObtenerProductoPorId(item.ProductoID);

                if (nuevaCantidad <= 0) { nuevaCantidad = 1; }
                if (nuevaCantidad > producto.CantidadDisponible) { MessageBox.Show($"Stock insuficiente. Disponible: {producto.CantidadDisponible}", "Error"); nuevaCantidad = producto.CantidadDisponible; }
                
                item.Cantidad = nuevaCantidad;
                dgvCarrito.Rows[e.RowIndex].Cells["Cantidad"].Value = item.Cantidad;
                ActualizarCarrito();
            }
            catch { ActualizarCarrito(); }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvCarrito.SelectedRows.Count > 0)
            {
                var nombreProducto = dgvCarrito.SelectedRows[0].Cells["Producto"].Value.ToString();
                if (MessageBox.Show($"¿Eliminar '{nombreProducto}' del carrito?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    carrito.RemoveAt(dgvCarrito.SelectedRows[0].Index);
                    ActualizarCarrito();
                }
            }
            else { MessageBox.Show("Selecciona un producto para eliminar", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            if (carrito.Any() && MessageBox.Show("¿Desea limpiar todo el carrito?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                LimpiarVenta();
            }
        }

        private void ActualizarTotal()
        {
            var total = carrito.Sum(c => c.Subtotal);
            lblTotal.Text = $"Total a pagar: {total:C2}";
            TxtMontoEfectivo_TextChanged(null, null); // Recalcular cambio
        }

        private void LimpiarVenta()
        {
            carrito.Clear();
            txtMontoEfectivo.Clear();
            txtMontoTarjeta.Clear();
            ActualizarCarrito();
        }

        // --- NUEVA LÓGICA DE PAGO ---

        private bool _isUpdatingPayments = false;

        private void TxtMontoEfectivo_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingPayments) return;

            _isUpdatingPayments = true;
            decimal totalVenta = carrito.Sum(c => c.Subtotal);
            decimal.TryParse(txtMontoEfectivo.Text, out decimal montoEfectivo);

            if (montoEfectivo >= totalVenta)
            {
                decimal cambio = montoEfectivo - totalVenta;
                lblCambio.Text = $"Cambio: {cambio:C2}";
                lblCambio.ForeColor = Color.Green;
                txtMontoTarjeta.Text = "0";
            }
            else
            {
                decimal restante = totalVenta - montoEfectivo;
                txtMontoTarjeta.Text = restante.ToString("F2");
                lblCambio.Text = "Cambio: $0.00";
                lblCambio.ForeColor = Color.Blue;
            }
            _isUpdatingPayments = false;
        }

        private void BtnCobrar_Click(object sender, EventArgs e)
        {
            if (!carrito.Any())
            {
                MessageBox.Show("El carrito está vacío.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal totalVenta = carrito.Sum(c => c.Subtotal);
            decimal.TryParse(txtMontoEfectivo.Text, out decimal montoEfectivo);
            decimal.TryParse(txtMontoTarjeta.Text, out decimal montoTarjeta);

            if (montoEfectivo + montoTarjeta < totalVenta)
            {
                MessageBox.Show("El monto pagado es insuficiente.", "Error de Pago", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            decimal cambio = (montoEfectivo + montoTarjeta) - totalVenta;

            var venta = new Venta
            {
                EmpleadoID = _empleadoAutenticado.IdEmpleado,
                FechaVenta = DateTime.Now,
                TotalVenta = totalVenta,
                MontoEfectivo = montoEfectivo > 0 ? montoEfectivo : (decimal?)null,
                MontoTarjeta = montoTarjeta > 0 ? montoTarjeta : (decimal?)null,
                Cambio = cambio > 0 ? cambio : (decimal?)null,
                Detalles = this.carrito
            };

            Venta ventaRegistrada = ventaBLL.RegistrarVenta(venta);

            if (ventaRegistrada != null)
            {
                MessageBox.Show($"Venta registrada con éxito.\nTotal: {ventaRegistrada.TotalVenta:C2}\nCambio: {ventaRegistrada.Cambio:C2}", "Venta Completada", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Preguntar si se desea generar el ticket
                if (MessageBox.Show("¿Desea generar el ticket de venta?", "Generar Ticket", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (var saveFileDialog = new SaveFileDialog())
                    {
                        saveFileDialog.Filter = "Archivo PDF (*.pdf)|*.pdf";
                        saveFileDialog.Title = "Guardar Ticket de Venta";
                        saveFileDialog.FileName = $"Ticket_Venta_{ventaRegistrada.VentaID}.pdf";

                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            try
                            {
                                ventaBLL.ExportarTicketPDF(ventaRegistrada, saveFileDialog.FileName);
                                MessageBox.Show($"Ticket guardado exitosamente en:\n{saveFileDialog.FileName}", "Ticket Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ocurrió un error al guardar el ticket: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }

                LimpiarVenta();
                CargarProductos(); // Recargar productos para actualizar stock
            }
            else
            {
                MessageBox.Show("Ocurrió un error al registrar la venta.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnTicketTemporal_Click(object sender, EventArgs e)
        {
            if (!carrito.Any())
            {
                MessageBox.Show("El carrito está vacío.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal totalVenta = carrito.Sum(c => c.Subtotal);
            decimal.TryParse(txtMontoEfectivo.Text, out decimal montoEfectivo);
            decimal.TryParse(txtMontoTarjeta.Text, out decimal montoTarjeta);
            decimal cambio = (montoEfectivo + montoTarjeta) - totalVenta;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--- TICKET DE VENTA (TEMPORAL) ---");
            sb.AppendLine($"Fecha: {DateTime.Now:g}");
            sb.AppendLine($"Atendido por: {_empleadoAutenticado.NombreEmp}");
            sb.AppendLine("------------------------------------");
            sb.AppendLine();
            sb.AppendLine("Producto".PadRight(20) + "Cant.".PadRight(8) + "Subtotal");
            sb.AppendLine("------------------------------------");

            foreach (var item in carrito)
            {
                var producto = productoBLL.ObtenerProductoPorId(item.ProductoID);
                string nombre = producto.Nombre.Length > 18 ? producto.Nombre.Substring(0, 18) + ".." : producto.Nombre;
                sb.AppendLine($"{nombre.PadRight(20)}{item.Cantidad.ToString().PadRight(8)}{item.Subtotal:C2}");
            }

            sb.AppendLine("------------------------------------");
            sb.AppendLine($"TOTAL: {totalVenta:C2}");
            sb.AppendLine();
            if (montoEfectivo > 0) sb.AppendLine($"Pagado (Efectivo): {montoEfectivo:C2}");
            if (montoTarjeta > 0) sb.AppendLine($"Pagado (Tarjeta): {montoTarjeta:C2}");
            if (cambio > 0) sb.AppendLine($"Cambio: {cambio:C2}");
            sb.AppendLine();
            sb.AppendLine("--- ¡Gracias por su compra! ---");

            // Mostrar en un MessageBox con fuente monoespaciada
            using (Form ticketForm = new Form())
            {
                ticketForm.Text = "Ticket Temporal";
                ticketForm.Size = new Size(400, 500);
                ticketForm.StartPosition = FormStartPosition.CenterParent;
                RichTextBox rtb = new RichTextBox
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Consolas", 10),
                    Text = sb.ToString(),
                    ReadOnly = true,
                    BackColor = Color.White
                };
                ticketForm.Controls.Add(rtb);
                ticketForm.ShowDialog();
            }
        }

        // --- FIN NUEVA LÓGICA DE PAGO ---

        private void BtnProductos_Click(object sender, EventArgs e) 
        { 
            using (var formInventario = new FormInventario()) 
            { 
                formInventario.ShowDialog(this); 
            } 
            CargarProductos(); 
        }

        private void BtnGestionInventario_Click(object sender, EventArgs e) 
        { 
            using (var formGestionInventario = new FormGestionInventario()) 
            { 
                formGestionInventario.ShowDialog(this); 
            } 
            CargarProductos(); 
        }

        private void BtnVerReportes_Click(object sender, EventArgs e) 
        { 
            using (var formVerReportes = new FormVerReportes(_empleadoAutenticado)) 
            { 
                formVerReportes.ShowDialog(this); 
            } 
        }

        private void LblEmpleado_Click(object sender, EventArgs e)
        {
            menuUsuario.Show(lblEmpleado, new Point(0, lblEmpleado.Height));
        }

        private void BtnCerrarSesion_Click(object sender, EventArgs e)
        {
            if (carrito.Any()) 
            { 
                if (MessageBox.Show("Hay productos en el carrito. ¿Seguro que deseas cerrar sesión?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No) 
                { 
                    return; 
                } 
            }
            if (MessageBox.Show("¿Deseas cerrar sesión?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) 
            { 
                this.DialogResult = DialogResult.Retry; 
                this.Close(); 
            }
        }

        private Button CrearBotonModulo(string texto, Color color)
        {
            return new Button
            {
                Text = texto,
                Width = 140,
                Height = 45,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(5, 0, 5, 0)
            };
        }
    }
}