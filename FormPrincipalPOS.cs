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

        // Variable para evitar bucles infinitos al editar celdas
        private bool _isUpdatingGrid = false; 

        // --- Controles principales ---
        private DataGridView dgvProductos;
        private DataGridView dgvCarrito;
        private TextBox txtBuscar;
        private ComboBox cmbCategoria;
        private Label lblTotal;
        private Label lblEmpleado;
        
        // Botones del menú superior
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
            InitializeComponent();
            ConfigurarFormulario();
            InitializeUserInterface();
            CargarProductos();
        }

        private void InitializeComponent()
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
            panelProductos.Width = (int)(this.Width * 0.60);

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

        // --- DISEÑO DEL ENCABEZADO ---
        private Panel CrearPanelSuperior()
        {
            var panel = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White, Padding = new Padding(10) };
            panel.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 220, 220), 1), 0, panel.Height - 1, panel.Width, panel.Height - 1);

            var lblTitulo = new Label 
            { 
                Text = "🛒 Kioskito ITH", 
                Font = new Font("Segoe UI", 16, FontStyle.Bold), 
                ForeColor = Color.FromArgb(45, 140, 200), 
                AutoSize = true, 
                Location = new Point(15, 25)
            };
            panel.Controls.Add(lblTitulo);

            var panelCentral = new FlowLayoutPanel 
            { 
                AutoSize = true, 
                FlowDirection = FlowDirection.LeftToRight, 
                Location = new Point(250, 20),
                WrapContents = false
            };

            cmbCategoria = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11), FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 5, 10, 0) };
            
            var panelWrapperBusqueda = new Panel { Size = new Size(250, 40), BackColor = this.BackColor, Margin = new Padding(0, 0, 0, 0) };
            panelWrapperBusqueda.Paint += (s, e) => PintarBordeRedondeado(e.Graphics, panelWrapperBusqueda, Color.FromArgb(200, 200, 200), 8);
            
            var iconoBuscar = new Label { Text = "🔍", Font = new Font("Segoe UI", 12), Location = new Point(8, 8), Size = new Size(25, 25), BackColor = Color.Transparent };
            txtBuscar = new TextBox { Location = new Point(35, 9), Width = 200, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 11), BackColor = this.BackColor, ForeColor = Color.Gray };
            txtBuscar.Text = "Buscar productos...";
            
            txtBuscar.Enter += (s, e) => { if (txtBuscar.Text == "Buscar productos...") { txtBuscar.Text = ""; txtBuscar.ForeColor = Color.Black; } };
            txtBuscar.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtBuscar.Text)) { txtBuscar.Text = "Buscar productos..."; txtBuscar.ForeColor = Color.Gray; } };

            panelWrapperBusqueda.Controls.Add(iconoBuscar);
            panelWrapperBusqueda.Controls.Add(txtBuscar);

            panelCentral.Controls.Add(cmbCategoria);
            panelCentral.Controls.Add(panelWrapperBusqueda);
            panel.Controls.Add(panelCentral);

            var panelDerecho = new FlowLayoutPanel 
            { 
                Dock = DockStyle.Right, 
                FlowDirection = FlowDirection.LeftToRight, 
                AutoSize = true, 
                Padding = new Padding(0, 10, 0, 0)
            };

            btnProductos = CrearBotonModulo("📦 Productos", Color.FromArgb(91, 192, 222));
            btnGestionInventario = CrearBotonModulo("📥 Inventario", Color.FromArgb(240, 173, 78));
            btnVerReportes = CrearBotonModulo("📈 Reportes", Color.FromArgb(45, 140, 200));

            var panelUsuario = new Panel { AutoSize = true, Margin = new Padding(20, 5, 0, 0), Cursor = Cursors.Hand };
            lblEmpleado = new Label { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(45, 140, 200), AutoSize = true, Text = "Usuario" };
            var lblRol = new Label { Font = new Font("Segoe UI", 8), ForeColor = Color.Gray, AutoSize = true, Location = new Point(0, 20), Text = "Rol" };
            
            panelUsuario.Controls.Add(lblEmpleado);
            panelUsuario.Controls.Add(lblRol);
            panelUsuario.Click += (s, e) => menuUsuario.Show(panelUsuario, 0, panelUsuario.Height);
            lblEmpleado.Click += (s, e) => menuUsuario.Show(panelUsuario, 0, panelUsuario.Height);
            lblEmpleado.Tag = lblRol; 

            panelDerecho.Controls.Add(btnProductos);
            panelDerecho.Controls.Add(btnGestionInventario);
            panelDerecho.Controls.Add(btnVerReportes);
            panelDerecho.Controls.Add(panelUsuario);

            panel.Controls.Add(panelDerecho);

            return panel;
        }

        private Button CrearBotonModulo(string texto, Color colorBase)
        {
            var btn = new Button
            {
                Text = texto,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = colorBase,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Padding = new Padding(10, 5, 10, 5),
                Margin = new Padding(5, 5, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private Panel CrearPanelProductos()
        {
            var panel = new Panel { Padding = new Padding(20, 20, 10, 20), BackColor = this.BackColor };
            var lblTitulo = new Label { Text = "Productos Disponibles", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(50, 50, 50), Dock = DockStyle.Top, Height = 35 };

            dgvProductos = new DataGridView 
            { 
                Dock = DockStyle.Fill, 
                AllowUserToAddRows = false, 
                AllowUserToDeleteRows = false, 
                ReadOnly = true, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, 
                RowHeadersVisible = false, 
                BackgroundColor = Color.White, 
                BorderStyle = BorderStyle.None, 
                Font = new Font("Segoe UI", 10), 
                RowTemplate = { Height = 45 }, 
                EnableHeadersVisualStyles = false, 
                AllowUserToResizeRows = false 
            };
            
            dgvProductos.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(45, 140, 200), ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Alignment = DataGridViewContentAlignment.MiddleLeft, Padding = new Padding(5) };
            dgvProductos.ColumnHeadersHeight = 40;
            dgvProductos.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvProductos.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
            dgvProductos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(91, 192, 222);
            dgvProductos.DefaultCellStyle.SelectionForeColor = Color.White;

            panel.Controls.Add(dgvProductos);
            panel.Controls.Add(lblTitulo);
            return panel;
        }

        private Panel CrearPanelCheckout()
        {
            var panel = new Panel { BackColor = Color.White, Padding = new Padding(10, 20, 20, 20) };
            panel.Paint += (s, e) => { using (var pen = new Pen(Color.FromArgb(20, 0, 0, 0), 1)) { e.Graphics.DrawLine(pen, 0, 0, 0, panel.Height); } };

            var panelPago = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 200, Padding = new Padding(5), ColumnCount = 2, RowCount = 4 };
            panelPago.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            panelPago.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));

            panelPago.Controls.Add(new Label { Text = "💵 Efectivo:", Font = new Font("Segoe UI", 10), Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
            txtMontoEfectivo = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12) };
            panelPago.Controls.Add(txtMontoEfectivo, 1, 0);

            panelPago.Controls.Add(new Label { Text = "💳 Tarjeta:", Font = new Font("Segoe UI", 10), Anchor = AnchorStyles.Left, AutoSize = true }, 0, 1);
            txtMontoTarjeta = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12) };
            panelPago.Controls.Add(txtMontoTarjeta, 1, 1);

            lblCambio = new Label { Text = "Cambio: $0.00", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.Blue, Anchor = AnchorStyles.Left, AutoSize = true };
            panelPago.SetColumnSpan(lblCambio, 2);
            panelPago.Controls.Add(lblCambio, 0, 2);

            btnCobrar = new Button { Text = "COBRAR", Height = 50, BackColor = Color.FromArgb(92, 184, 92), ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Dock = DockStyle.Fill };
            btnTicketTemporal = new Button { Text = "TICKET", Height = 50, BackColor = Color.FromArgb(240, 173, 78), ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Dock = DockStyle.Fill };
            
            panelPago.Controls.Add(btnCobrar, 0, 3);
            panelPago.Controls.Add(btnTicketTemporal, 1, 3);

            panel.Controls.Add(panelPago);

            lblTotal = new Label { Dock = DockStyle.Bottom, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.DarkGreen, BackColor = Color.FromArgb(230, 230, 230), TextAlign = ContentAlignment.MiddleCenter, Height = 50, Margin = new Padding(0, 10, 0, 10) };
            lblTotal.Text = "$0.00";
            panel.Controls.Add(lblTotal);

            var panelAcciones = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Padding = new Padding(0, 10, 0, 10) };
            btnLimpiar = new Button { Text = "Limpiar", BackColor = Color.LightCoral, Height = 35, Width = 80, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnEliminar = new Button { Text = "Quitar", BackColor = Color.IndianRed, Height = 35, Width = 80, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold), Margin = new Padding(0, 0, 5, 0) };
            
            panelAcciones.Controls.Add(btnLimpiar);
            panelAcciones.Controls.Add(btnEliminar);
            panel.Controls.Add(panelAcciones);

            var panelAreaCarrito = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };
            var lblTituloCarrito = new Label { Text = "🛒 Tu Carrito", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(50, 50, 50), Dock = DockStyle.Top, Height = 30 };
            lblItemsCarrito = new Label { Text = "0 items", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, Dock = DockStyle.Top, Height = 20 };
            
            dgvCarrito = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10), RowTemplate = { Height = 40 }, EnableHeadersVisualStyles = false, GridColor = Color.FromArgb(240, 240, 240) };
            dgvCarrito.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(248, 249, 250), ForeColor = Color.Black, Font = new Font("Segoe UI", 9, FontStyle.Bold), Alignment = DataGridViewContentAlignment.MiddleLeft };
            dgvCarrito.ColumnHeadersHeight = 35;
            dgvCarrito.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            panelAreaCarrito.Controls.Add(dgvCarrito);
            panelAreaCarrito.Controls.Add(lblItemsCarrito);
            panelAreaCarrito.Controls.Add(lblTituloCarrito);
            panel.Controls.Add(panelAreaCarrito);

            return panel;
        }

        private void PintarBordeRedondeado(Graphics g, Control control, Color borderColor, int radius)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = RoundedRect(new Rectangle(0, 0, control.Width - 1, control.Height - 1), radius)) 
            { 
                control.Region = new Region(path); 
                using (var pen = new Pen(borderColor, 1)) { g.DrawPath(pen, path); } 
            }
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
            dgvProductos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nombre", HeaderText = "Producto", FillWeight = 45 });
            dgvProductos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Categoria", HeaderText = "Cat.", FillWeight = 25 });
            dgvProductos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Precio", HeaderText = "Precio", FillWeight = 15, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvProductos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Stock", HeaderText = "Stock", FillWeight = 15, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvProductos.Columns["ID"].Visible = false;

            dgvCarrito.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCarrito.Columns.Add("ID", "ID");
            dgvCarrito.Columns.Add(new DataGridViewTextBoxColumn { Name = "Producto", HeaderText = "Producto", ReadOnly = true, FillWeight = 40 });
            dgvCarrito.Columns.Add(new DataGridViewTextBoxColumn { Name = "Precio", HeaderText = "$ Unit", ReadOnly = true, FillWeight = 20, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight } });
            
            // AQUI ES EL CAMBIO IMPORTANTE: La columna cantidad NO es ReadOnly para poder editarla
            dgvCarrito.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cantidad", HeaderText = "Cant.", FillWeight = 15, ReadOnly = false, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter, BackColor = Color.LightYellow } });
            
            dgvCarrito.Columns.Add(new DataGridViewTextBoxColumn { Name = "Subtotal", HeaderText = "Total", ReadOnly = true, FillWeight = 25, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10, FontStyle.Bold) } });
            dgvCarrito.Columns["ID"].Visible = false;
        }

        private void ConfigurarEventos()
        {
            txtBuscar.TextChanged += TxtBuscar_TextChanged;
            cmbCategoria.SelectedIndexChanged += CmbCategoria_SelectedIndexChanged;
            dgvProductos.CellDoubleClick += DgvProductos_CellDoubleClick;
            dgvProductos.CellFormatting += DgvProductos_CellFormatting;
            
            // Evento de edición de celda en carrito
            dgvCarrito.CellValueChanged += DgvCarrito_CellValueChanged;
            
            btnEliminar.Click += BtnEliminar_Click;
            btnLimpiar.Click += BtnLimpiar_Click;
            
            btnProductos.Click += BtnProductos_Click;
            btnGestionInventario.Click += BtnGestionInventario_Click;
            btnVerReportes.Click += BtnVerReportes_Click;
            
            txtMontoEfectivo.TextChanged += TxtMontoEfectivo_TextChanged;
            btnCobrar.Click += BtnCobrar_Click;
            btnTicketTemporal.Click += BtnTicketTemporal_Click;
        }

        private void InitializeUserInterface()
        {
            lblEmpleado.Text = _empleadoAutenticado.NombreEmp;
            if (lblEmpleado.Tag is Label lblRol)
            {
                lblRol.Text = _empleadoAutenticado.Puesto;
            }

            string puesto = _empleadoAutenticado.Puesto ?? "";
            bool esAdmin = puesto.Equals("Administrador", StringComparison.OrdinalIgnoreCase);
            bool esGerente = puesto.Equals("Gerente", StringComparison.OrdinalIgnoreCase);
            bool esSupervisor = puesto.Equals("Supervisor", StringComparison.OrdinalIgnoreCase) || puesto.Equals("Supervisora", StringComparison.OrdinalIgnoreCase);
            
            btnProductos.Visible = esAdmin || esGerente;
            btnGestionInventario.Visible = esAdmin || esGerente || esSupervisor;
            btnVerReportes.Visible = esAdmin || esGerente;
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
            _isUpdatingGrid = true; // Bloqueamos eventos mientras redibujamos todo
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
            _isUpdatingGrid = false; // Desbloqueamos eventos
        }

        // --- LÓGICA MEJORADA PARA EDICIÓN EN CARRITO ---
        private void DgvCarrito_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Si estamos actualizando masivamente o no es la columna cantidad, salimos
            if (_isUpdatingGrid || e.RowIndex < 0 || e.ColumnIndex != dgvCarrito.Columns["Cantidad"].Index) return;
            
            _isUpdatingGrid = true; // Bloquear reentrada
            try
            {
                var cell = dgvCarrito.Rows[e.RowIndex].Cells["Cantidad"];
                var item = carrito[e.RowIndex]; // Item en memoria
                var producto = productoBLL.ObtenerProductoPorId(item.ProductoID);

                // Intentamos parsear el valor que el usuario escribió
                string inputValor = cell.Value?.ToString();
                
                if (int.TryParse(inputValor, out int nuevaCantidad))
                {
                    // 1. Validar números positivos
                    if (nuevaCantidad <= 0)
                    {
                        MessageBox.Show("La cantidad debe ser mayor a 0.", "Cantidad Inválida");
                        cell.Value = item.Cantidad; // Restauramos valor anterior
                    }
                    // 2. Validar Stock disponible
                    else if (nuevaCantidad > producto.CantidadDisponible)
                    {
                        MessageBox.Show($"Stock insuficiente. Solo hay {producto.CantidadDisponible} unidades disponibles.", "Stock Límite");
                        item.Cantidad = producto.CantidadDisponible;
                        cell.Value = producto.CantidadDisponible;
                    }
                    else
                    {
                        // Valor válido: actualizamos modelo y totales
                        item.Cantidad = nuevaCantidad;
                        item.Subtotal = item.Cantidad * item.PrecioUnitario;
                        
                        // Actualizamos visualmente solo la celda de subtotal y el total general
                        dgvCarrito.Rows[e.RowIndex].Cells["Subtotal"].Value = item.Subtotal;
                        lblItemsCarrito.Text = $"{carrito.Sum(c => c.Cantidad)} items";
                        ActualizarTotal();
                    }
                }
                else
                {
                    // Si escribió letras o símbolos, restauramos el valor anterior silenciosamente o con aviso
                    MessageBox.Show("Por favor, ingresa un número válido.", "Error de Formato");
                    cell.Value = item.Cantidad; // Restauramos valor anterior
                }
            }
            catch (Exception ex)
            {
                // Red de seguridad final
                MessageBox.Show("Error al actualizar cantidad: " + ex.Message);
            }
            finally
            {
                _isUpdatingGrid = false; // Liberar bloqueo
            }
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
            lblTotal.Text = $"{total:C2}"; 
            TxtMontoEfectivo_TextChanged(null, null);
        }

        private void LimpiarVenta()
        {
            carrito.Clear();
            txtMontoEfectivo.Clear();
            txtMontoTarjeta.Clear();
            ActualizarCarrito();
        }

        // --- LÓGICA DE PAGO ---
        private bool _isUpdatingPayments = false;

        private void TxtMontoEfectivo_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingPayments) return;

            _isUpdatingPayments = true;
            decimal totalVenta = carrito.Sum(c => c.Subtotal);
            decimal.TryParse(txtMontoEfectivo.Text, out decimal montoEfectivo);

            if (montoEfectivo >= totalVenta && totalVenta > 0)
            {
                decimal cambio = montoEfectivo - totalVenta;
                lblCambio.Text = $"Cambio: {cambio:C2}";
                lblCambio.ForeColor = Color.Green;
                txtMontoTarjeta.Text = "0";
            }
            else
            {
                decimal restante = totalVenta - montoEfectivo;
                if (restante < 0) restante = 0;
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

            if ((montoEfectivo + montoTarjeta) < (totalVenta - 0.01m))
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
                                MessageBox.Show($"Ticket guardado en:\n{saveFileDialog.FileName}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error al guardar ticket: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                LimpiarVenta();
                CargarProductos();
            }
            else
            {
                MessageBox.Show("Ocurrió un error al registrar la venta en base de datos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnTicketTemporal_Click(object sender, EventArgs e)
        {
            if (!carrito.Any())
            {
                MessageBox.Show("El carrito está vacío.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--- TICKET PRELIMINAR ---");
            sb.AppendLine($"Fecha: {DateTime.Now:g}");
            sb.AppendLine($"Cajero: {_empleadoAutenticado.NombreEmp}");
            sb.AppendLine("------------------------------------");
            foreach(var item in carrito)
            {
                sb.AppendLine($"{item.Cantidad} x {item.PrecioUnitario:C2} - {item.ProductoID}");
            }
            sb.AppendLine("------------------------------------");
            sb.AppendLine($"TOTAL: {carrito.Sum(x=>x.Subtotal):C2}");
            
            MessageBox.Show(sb.ToString(), "Vista Previa Ticket", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // --- NAVEGACIÓN Y SISTEMA ---

        private void BtnCerrarSesion_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Estás seguro de cerrar sesión?", "Cerrar Sesión", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Close(); 
            }
        }

        private void BtnProductos_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Módulo de Gestión de Productos (En desarrollo)", "Info");
        }

        private void BtnGestionInventario_Click(object sender, EventArgs e)
        {
            try 
            {
                FormInventario formInv = new FormInventario(_empleadoAutenticado);
                formInv.ShowDialog();
                CargarProductos(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir inventario: " + ex.Message);
            }
        }

        private void BtnVerReportes_Click(object sender, EventArgs e)
        {
            try 
            {
                FormVerReportes formRep = new FormVerReportes(_empleadoAutenticado);
                formRep.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir reportes: " + ex.Message);
            }
        }

        private void LblEmpleado_Click(object sender, EventArgs e)
        {
            menuUsuario.Show(Cursor.Position);
        }
    }
}