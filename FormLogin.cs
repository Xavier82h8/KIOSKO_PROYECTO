using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using KIOSKO_Proyecto.BLL;
using KIOSKO_Proyecto.Modelos; // Asegúrate de tener este namespace para 'Empleado'

namespace KIOSKO_Proyecto
{
    public class FormLogin : Form
    {
        private TextBox txtUsuario;
        private TextBox txtContrasena;
        private Button btnIngresar;
        private Button btnMostrarContrasena;
        private Label lblRecordarContrasena;
        private Panel panelLateral;
        private Panel panelPrincipal;
        private PictureBox pbLogo;
        private bool mostrarPassword = false;

        // CORRECCIÓN 1: Declarar la variable sin instanciarla aquí para evitar crashes
        private EmpleadoBLL _empleadoBLL;

        public Empleado EmpleadoAutenticado { get; private set; }

        public FormLogin()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            // CORRECCIÓN 2: Instanciar la BLL dentro de un try-catch
            // Esto evita que la app se cierre de golpe si no hay conexión
            try
            {
                _empleadoBLL = new EmpleadoBLL();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo establecer conexión con la base de datos.\n" +
                                "Detalle: " + ex.Message,
                                "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Opcional: Deshabilitar el botón de ingresar si no hay conexión
                if (btnIngresar != null) btnIngresar.Enabled = false;
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Kioskito ITH - Login";
            this.Size = new Size(900, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;

            // Panel lateral izquierdo con degradado
            panelLateral = new Panel
            {
                Dock = DockStyle.Left,
                Width = 400,
                BackColor = Color.FromArgb(45, 206, 220)
            };
            panelLateral.Paint += PanelLateral_Paint;

            // Logo
            pbLogo = new PictureBox
            {
                Size = new Size(280, 160),
                Location = new Point(60, 150),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };

            // Título en panel lateral
            var lblTitulo = new Label
            {
                Text = "Kioskito\nITH",
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 50, 90),
                Location = new Point(60, 40),
                AutoSize = true
            };

            // Subtítulo en panel lateral
            var lblSubtitulo = new Label
            {
                Text = "Sistema de Gestión\nPunto de Venta",
                Font = new Font("Segoe UI", 14, FontStyle.Regular),
                ForeColor = Color.FromArgb(50, 70, 110),
                Location = new Point(60, 340),
                AutoSize = true
            };

            panelLateral.Controls.AddRange(new Control[] { pbLogo, lblTitulo, lblSubtitulo });

            // Panel principal derecho
            panelPrincipal = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 245, 250)
            };

            // Botón cerrar
            var btnCerrar = new Button
            {
                Text = "✕",
                Size = new Size(40, 40),
                Location = new Point(450, 10),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 16, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 100, 120),
                Cursor = Cursors.Hand
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 100, 100);
            btnCerrar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            btnCerrar.MouseEnter += (s, e) => btnCerrar.ForeColor = Color.White;
            btnCerrar.MouseLeave += (s, e) => btnCerrar.ForeColor = Color.FromArgb(100, 100, 120);

            // Título bienvenida
            var lblBienvenida = new Label
            {
                Text = "¡Bienvenido de nuevo!",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 50, 90),
                Location = new Point(60, 80),
                AutoSize = true
            };

            var lblSubBienvenida = new Label
            {
                Text = "Ingresa tus credenciales para continuar",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(120, 120, 140),
                Location = new Point(60, 125),
                AutoSize = true
            };

            // Label Email/Usuario
            var lblEmail = new Label
            {
                Text = "Nombre de usuario",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(80, 80, 100),
                Location = new Point(60, 180),
                AutoSize = true
            };

            // TextBox Usuario con estilo moderno
            txtUsuario = new TextBox
            {
                Location = new Point(15, 12),
                Width = 350,
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(50, 50, 70)
            };

            var panelUsuario = new Panel
            {
                Location = new Point(60, 205),
                Size = new Size(380, 45),
                BackColor = Color.White,
            };
            panelUsuario.Paint += (s, e) => PintarBordeRedondeado(e.Graphics, panelUsuario, Color.FromArgb(220, 220, 230));
            panelUsuario.Controls.Add(txtUsuario);

            // Label Contraseña
            var lblPassword = new Label
            {
                Text = "Contraseña",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(80, 80, 100),
                Location = new Point(60, 270),
                AutoSize = true
            };

            // TextBox Contraseña con botón de mostrar
            txtContrasena = new TextBox
            {
                Location = new Point(15, 12),
                Width = 310,
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.None,
                UseSystemPasswordChar = true,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(50, 50, 70)
            };
            // Agregar evento KeyDown para aceptar con ENTER
            txtContrasena.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter)
                {
                    BtnIngresar_Click(this, new EventArgs());
                    e.SuppressKeyPress = true; // Evitar sonido 'ding'
                }
            };

            btnMostrarContrasena = new Button
            {
                Text = "👁",
                Size = new Size(40, 30),
                Location = new Point(330, 8),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnMostrarContrasena.FlatAppearance.BorderSize = 0;
            btnMostrarContrasena.Click += BtnMostrarContrasena_Click;

            var panelContrasena = new Panel
            {
                Location = new Point(60, 295),
                Size = new Size(380, 45),
                BackColor = Color.White,
            };
            panelContrasena.Paint += (s, e) => PintarBordeRedondeado(e.Graphics, panelContrasena, Color.FromArgb(220, 220, 230));
            panelContrasena.Controls.AddRange(new Control[] { txtContrasena, btnMostrarContrasena });

            // Recordar contraseña
            lblRecordarContrasena = new Label
            {
                Text = "Recordar contraseña",
                Font = new Font("Segoe UI", 9, FontStyle.Underline),
                ForeColor = Color.FromArgb(45, 140, 200),
                Location = new Point(60, 355),
                AutoSize = true,
                Cursor = Cursors.Hand
            };

            // Botón Ingresar moderno
            btnIngresar = new Button
            {
                Text = "Iniciar sesión",
                Size = new Size(380, 50),
                Location = new Point(60, 400),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 140, 200),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnIngresar.FlatAppearance.BorderSize = 0;
            btnIngresar.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundedRect(btnIngresar.ClientRectangle, 10))
                {
                    btnIngresar.Region = new Region(path);
                }
            };
            btnIngresar.Click += BtnIngresar_Click;
            btnIngresar.MouseEnter += (s, e) => btnIngresar.BackColor = Color.FromArgb(35, 120, 180);
            btnIngresar.MouseLeave += (s, e) => btnIngresar.BackColor = Color.FromArgb(45, 140, 200);

            panelPrincipal.Controls.AddRange(new Control[] {
                btnCerrar, lblBienvenida, lblSubBienvenida,
                lblEmail, panelUsuario, lblPassword, panelContrasena,
                lblRecordarContrasena, btnIngresar
            });

            this.Controls.Add(panelPrincipal);
            this.Controls.Add(panelLateral);

            // Asignar botón predeterminado
            this.AcceptButton = btnIngresar;
        }

        private void PanelLateral_Paint(object sender, PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
                panelLateral.ClientRectangle,
                Color.FromArgb(45, 206, 220),
                Color.FromArgb(30, 144, 180),
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, panelLateral.ClientRectangle);
            }

            // Círculos decorativos
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(30, 255, 255, 255)))
            {
                e.Graphics.FillEllipse(brush, new Rectangle(-50, 400, 200, 200));
                e.Graphics.FillEllipse(brush, new Rectangle(300, -80, 180, 180));
            }
        }

        private void PintarBordeRedondeado(Graphics g, Panel panel, Color borderColor)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = RoundedRect(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 8))
            {
                panel.Region = new Region(path);
                using (Pen pen = new Pen(borderColor, 1))
                {
                    g.DrawPath(pen, path);
                }
            }
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void BtnMostrarContrasena_Click(object sender, EventArgs e)
        {
            mostrarPassword = !mostrarPassword;
            txtContrasena.UseSystemPasswordChar = !mostrarPassword;
            btnMostrarContrasena.Text = mostrarPassword ? "🙈" : "👁";
        }

        private void BtnIngresar_Click(object sender, EventArgs e)
        {
            if (_empleadoBLL == null)
            {
                MessageBox.Show("No hay conexión con la base de datos. Reinicia la aplicación.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var usuario = txtUsuario.Text.Trim();
            var pass = txtContrasena.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                MessageBox.Show("Por favor introduce tu usuario.", "Atención",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsuario.Focus();
                return;
            }

            // Validar credenciales usando Try/Catch por si la consulta falla
            try
            {
                var id = _empleadoBLL.ValidarCredenciales(usuario, pass);
                if (id.HasValue)
                {
                    EmpleadoAutenticado = _empleadoBLL.ObtenerEmpleadoPorId(id.Value);
                    if (EmpleadoAutenticado != null)
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Error al obtener los datos del empleado.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Usuario o contraseña incorrectos.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtContrasena.SelectAll();
                    txtContrasena.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al intentar validar el usuario: {ex.Message}", "Error Base de Datos",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}