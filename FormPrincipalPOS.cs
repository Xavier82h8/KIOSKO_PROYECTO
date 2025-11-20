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
        // --- CAPAS DE NEGOCIO ---
        private ProductoBLL productoBLL = new ProductoBLL();
        private VentaBLL ventaBLL = new VentaBLL();

        // --- DATOS ---
        private List<DetalleVenta> carrito = new List<DetalleVenta>();
        private Modelos.Empleado _empleadoAutenticado;
        private bool _isUpdatingGrid = false; // Evita bucles al editar celdas

        // --- CONTROLES ---
        private DataGridView dgvProductos;
        private DataGridView dgvCarrito;
        private TextBox txtBuscar;
        private ComboBox cmbCategoria;

        private Label lblTotal;
        private Label lblEmpleado;
        private Label lblRol;
        private ContextMenuStrip menuUsuario;

        // Botones Menú
        private Button btnProductos;
        private Button btnGestionInventario;
        private Button btnVerReportes;

        // Controles Carrito
        private Label lblItemsCarrito;
        private Button btnEliminar;
        private Button btnLimpiar;

        // Controles Pago
        private TextBox txtMontoEfectivo;
        private TextBox txtMontoTarjeta;
        private Label lblCambio;
        private Button btnCobrar;
        private Button btnTicketTemporal;

        public FormPrincipalPOS(Modelos.Empleado empleado)
        {
            _empleadoAutenticado = empleado;
            this.DoubleBuffered = true; // Reduce parpadeo

            // Inicializar controles de usuario antes de InitializeComponent
            lblEmpleado = new Label();
            lblRol = new Label();

            InitializeComponent();
            ConfigurarFormulario();
            InitializeUserInterface();
            CargarProductos();
        }

        // Método para volver a mostrar el formulario después de login exitoso
        public void MostrarFormulario()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = "Kioskito ITH - Sistema POS";
            this.Size = new Size(1400, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(248, 249, 250);

            // 1. Crear los Paneles
            var panelSuperior = CrearPanelSuperior();
            var panelCheckout = CrearPanelCheckout(); // Panel derecho/central
            var panelProductos = CrearPanelProductos(); // Panel izquierdo

            // 2. Configurar Docking (Anclaje)
            panelSuperior.Dock = DockStyle.Top;
            panelProductos.Dock = DockStyle.Left;
            panelProductos.Width = (int)(this.Width * 0.60); // 60% del ancho
            panelCheckout.Dock = DockStyle.Fill; // Ocupa el resto

            // 3. Agregar al Formulario 
            this.Controls.Add(panelCheckout);   // Fondo
            this.Controls.Add(panelProductos);  // Izquierda
            this.Controls.Add(panelSuperior);   // Arriba

            CrearMenuUsuario();
            ConfigurarEventos();

            this.ResumeLayout(false);
        }

        // --- DISEÑO DEL ENCABEZADO CORREGIDO ---
        private Panel CrearPanelSuperior()
        {
            var panel = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Color.White };

            // Línea gris decorativa abajo
            panel.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.LightGray, 1), 0, panel.Height - 1, panel.Width, panel.Height - 1);

            // Usamos un TableLayoutPanel para distribución precisa
            var tableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };

            // Configurar columnas: 30% Logo, 40% Buscador, 30% Módulos + Usuario
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

            // --- 1. SECCIÓN IZQUIERDA: LOGO ---
            var panelLogo = new Panel { Dock = DockStyle.Fill };
            var lblLogo = new Label
            {
                Text = "🛒 Kioskito ITH",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 140, 200),
                AutoSize = true,
                Location = new Point(20, 25)
            };
            panelLogo.Controls.Add(lblLogo);
            tableLayoutPanel.Controls.Add(panelLogo, 0, 0);

            // --- 2. SECCIÓN CENTRAL: BUSCADOR Y FILTROS ---
            var panelBusqueda = new Panel { Dock = DockStyle.Fill };
            var containerBusqueda = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Location = new Point(10, 25)
            };

            cmbCategoria = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 10, 0)
            };

            // Panel wrapper para darle borde redondeado al textbox
            var panelTxtWrapper = new Panel
            {
                Size = new Size(250, 40),
                BackColor = Color.WhiteSmoke
            };
            panelTxtWrapper.Paint += (s, e) => PintarBordeRedondeado(e.Graphics, panelTxtWrapper, Color.Silver, 8);

            var icoSearch = new Label
            {
                Text = "🔍",
                Font = new Font("Segoe UI", 12),
                Location = new Point(8, 10),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            txtBuscar = new TextBox
            {
                Location = new Point(35, 8),
                Width = 200,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.WhiteSmoke,
                ForeColor = Color.Gray,
                Text = "Buscar productos..."
            };

            // Placeholders del buscador
            txtBuscar.Enter += (s, e) => { if (txtBuscar.Text == "Buscar productos...") { txtBuscar.Text = ""; txtBuscar.ForeColor = Color.Black; } };
            txtBuscar.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtBuscar.Text)) { txtBuscar.Text = "Buscar productos..."; txtBuscar.ForeColor = Color.Gray; } };

            panelTxtWrapper.Controls.Add(icoSearch);
            panelTxtWrapper.Controls.Add(txtBuscar);
            containerBusqueda.Controls.Add(cmbCategoria);
            containerBusqueda.Controls.Add(panelTxtWrapper);
            panelBusqueda.Controls.Add(containerBusqueda);
            tableLayoutPanel.Controls.Add(panelBusqueda, 1, 0);

            // --- 3. SECCIÓN DERECHA: MÓDULOS + USUARIO ---
            var panelDerecha = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Location = new Point(0, 20)
            };

            // Botones Módulos
            btnProductos = CrearBotonModulo("📦 Productos");
            btnGestionInventario = CrearBotonModulo("📥 Inventario");
            btnVerReportes = CrearBotonModulo("📈 Reportes");

            // Panel del Usuario (siempre a la derecha)
            var panelUsuario = new Panel
            {
                Size = new Size(140, 50),
                Margin = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand
            };

            string nombre = _empleadoAutenticado != null ? _empleadoAutenticado.NombreEmp : "Usuario";
            string rol = _empleadoAutenticado != null ? _empleadoAutenticado.Puesto : "Rol";

            lblEmpleado = new Label
            {
                Text = nombre,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64),
                AutoSize = true,
                Location = new Point(5, 5)
            };
            lblRol = new Label
            {
                Text = rol,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(5, 28)
            };

            // Evento para menú al hacer clic
            void ClickMenu(object s, EventArgs e) => menuUsuario?.Show(Cursor.Position);
            panelUsuario.Click += ClickMenu;
            lblEmpleado.Click += ClickMenu;
            lblRol.Click += ClickMenu;

            panelUsuario.Controls.Add(lblEmpleado);
            panelUsuario.Controls.Add(lblRol);

            // Agregar controles al panel derecho
            panelDerecha.Controls.Add(btnProductos);
            panelDerecha.Controls.Add(btnGestionInventario);
            panelDerecha.Controls.Add(btnVerReportes);
            panelDerecha.Controls.Add(panelUsuario);

            tableLayoutPanel.Controls.Add(panelDerecha, 2, 0);

            panel.Controls.Add(tableLayoutPanel);
            return panel;
        }

        private Button CrearBotonModulo(string texto)
        {
            Color colorBase;
            if (texto.Contains("Productos")) colorBase = Color.FromArgb(91, 192, 222);
            else if (texto.Contains("Inventario")) colorBase = Color.FromArgb(240, 173, 78);
            else colorBase = Color.FromArgb(45, 140, 200);

            var btn = new Button
            {
                Text = texto,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = colorBase,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(110, 35),
                Margin = new Padding(5, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                (int)(colorBase.R * 0.9),
                (int)(colorBase.G * 0.9),
                (int)(colorBase.B * 0.9)
            );
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

        // --- UTILIDADES GRÁFICAS ---
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

        // --- CONFIGURACIÓN ---
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

            // Columna Cantidad Editable
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

        private void CrearMenuUsuario()
        {
            menuUsuario = new ContextMenuStrip();
            var itemCerrarSesion = menuUsuario.Items.Add("Cerrar sesión");
            itemCerrarSesion.Click += BtnCerrarSesion_Click;
        }

        private void InitializeUserInterface()
        {
            if (_empleadoAutenticado != null)
            {
                lblEmpleado.Text = _empleadoAutenticado.NombreEmp;
                lblRol.Text = _empleadoAutenticado.Puesto;

                string puesto = _empleadoAutenticado.Puesto ?? "";
                bool esAdmin = puesto.IndexOf("Admin", StringComparison.OrdinalIgnoreCase) >= 0;
                bool esGerente = puesto.IndexOf("Gerente", StringComparison.OrdinalIgnoreCase) >= 0;
                bool esSupervisor = puesto.IndexOf("Supervisor", StringComparison.OrdinalIgnoreCase) >= 0;

                // CORRECCIÓN: EL BOTÓN DE PRODUCTOS AHORA ES SIEMPRE VISIBLE
                btnProductos.Visible = true; // Siempre visible
                btnGestionInventario.Visible = esAdmin || esGerente || esSupervisor;
                btnVerReportes.Visible = esAdmin || esGerente || esSupervisor;
            }
        }

        // --- MÉTODOS DE NEGOCIO ---

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
            _isUpdatingGrid = true;
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
            _isUpdatingGrid = false;
        }

        private void DgvCarrito_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_isUpdatingGrid || e.RowIndex < 0 || e.ColumnIndex != dgvCarrito.Columns["Cantidad"].Index) return;

            _isUpdatingGrid = true;
            try
            {
                var cell = dgvCarrito.Rows[e.RowIndex].Cells["Cantidad"];
                var item = carrito[e.RowIndex];
                var producto = productoBLL.ObtenerProductoPorId(item.ProductoID);

                string inputValor = cell.Value?.ToString();

                if (int.TryParse(inputValor, out int nuevaCantidad))
                {
                    if (nuevaCantidad <= 0)
                    {
                        MessageBox.Show("La cantidad debe ser mayor a 0.", "Cantidad Inválida");
                        cell.Value = item.Cantidad;
                    }
                    else if (nuevaCantidad > producto.CantidadDisponible)
                    {
                        MessageBox.Show($"Stock insuficiente. Solo hay {producto.CantidadDisponible} unidades disponibles.", "Stock Límite");
                        item.Cantidad = producto.CantidadDisponible;
                        cell.Value = producto.CantidadDisponible;
                    }
                    else
                    {
                        item.Cantidad = nuevaCantidad;
                        item.Subtotal = item.Cantidad * item.PrecioUnitario;
                        dgvCarrito.Rows[e.RowIndex].Cells["Subtotal"].Value = item.Subtotal;
                        lblItemsCarrito.Text = $"{carrito.Sum(c => c.Cantidad)} items";
                        ActualizarTotal();
                    }
                }
                else
                {
                    MessageBox.Show("Por favor, ingresa un número válido.", "Error de Formato");
                    cell.Value = item.Cantidad;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar cantidad: " + ex.Message);
            }
            finally
            {
                _isUpdatingGrid = false;
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
            foreach (var item in carrito)
            {
                var producto = productoBLL.ObtenerProductoPorId(item.ProductoID);
                sb.AppendLine($"{item.Cantidad} x {producto.Nombre.PadRight(20).Substring(0, Math.Min(20, producto.Nombre.Length))} - {item.Subtotal:C2}");
            }
            sb.AppendLine("------------------------------------");
            sb.AppendLine($"TOTAL: {carrito.Sum(x => x.Subtotal):C2}");

            MessageBox.Show(sb.ToString(), "Vista Previa Ticket", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // --- NAVEGACIÓN ---

        private void BtnGestionInventario_Click(object sender, EventArgs e)
        {
            try
            {
                FormInventario formInv = new FormInventario(_empleadoAutenticado);
                formInv.ShowDialog();
                CargarProductos();
            }
            catch (Exception ex) { MessageBox.Show("Error al abrir inventario: " + ex.Message); }
        }

        // CORRECCIÓN: Botón de productos ahora abre el formulario correcto
        private void BtnProductos_Click(object sender, EventArgs e)
        {
            try
            {
                // Abrir el formulario de gestión de productos con el usuario autenticado
                FormularioProducto formProductos = new FormularioProducto();
                formProductos.EmpleadoAutenticado = _empleadoAutenticado;
                formProductos.ShowDialog();

                // Recargar productos después de cerrar el formulario
                CargarProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir el módulo de productos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnVerReportes_Click(object sender, EventArgs e)
        {
            try
            {
                FormVerReportes formRep = new FormVerReportes(_empleadoAutenticado);
                formRep.ShowDialog();
            }
            catch (Exception ex) { MessageBox.Show("Error al abrir reportes: " + ex.Message); }
        }

        // CORRECCIÓN: Flujo de cierre de sesión mejorado
        private void BtnCerrarSesion_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Estás seguro de cerrar sesión?", "Cerrar Sesión", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Ocultar el formulario actual
                this.Hide();

                // Crear y mostrar el formulario de login
                FormLogin loginForm = new FormLogin();

                // Mostrar el login de forma modal
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    // Si el login fue exitoso, actualizar el empleado autenticado
                    _empleadoAutenticado = loginForm.EmpleadoAutenticado;

                    // Actualizar la interfaz de usuario
                    InitializeUserInterface();

                    // Mostrar el formulario principal nuevamente
                    this.Show();
                }
                else
                {
                    // Si el usuario canceló el login, cerrar la aplicación
                    Application.Exit();
                }
            }
        }
    }
}