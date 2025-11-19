using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using KIOSKO_Proyecto.BLL;
using KIOSKO_Proyecto.Modelos;

namespace KIOSKO_Proyecto
{
    public partial class FormVerReportes : Form
    {
        private Empleado _empleado;
        private TabControl tabControlPrincipal;
        private TabPage tabPageReportesGuardados;
        private TabPage tabPageGenerarReporte;

        // --- Controles para Reportes Guardados ---
        private DataGridView dgvReportesGuardados;
        private Button btnExportarGuardados;

        // --- Controles para Generar Reporte ---
        private DataGridView dgvVentasDetalladas;
        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private Button btnGenerarReporte;
        private Button btnGenerarCorteCaja;
        private Button btnExportarNuevo;
        private Button btnGuardarReporte;

        private ReporteBLL _reporteBLL = new ReporteBLL();
        private Empleado _empleadoActual;

        // --- Controles UI ---
        private TabControl tabControlPrincipal;
        private TabPage tabVentasDetalladas;
        private TabPage tabCorteCaja;

        // Controles para Ventas Detalladas
        private DateTimePicker dtpInicioVentas;
        private DateTimePicker dtpFinVentas;
        private Button btnGenerarReporteVentas;
        private Button btnExportarVentasCSV;
        private DataGridView dgvVentasDetalladas;

        // Controles para Corte de Caja
        private DateTimePicker dtpFechaCorte;
        private Button btnGenerarCorte;
        private Button btnExportarCortePDF;
        private DataGridView dgvCorteCaja;
        private Label lblTotalCorte;


        public FormVerReportes(Empleado empleado)
        {
            _empleado = empleado;
            this.Text = "Módulo de Reportes";
        public FormVerReportes(Empleado empleado)
        {
            _empleadoActual = empleado;
            InitializeComponent();
            this.Load += FormVerReportes_Load;
        }

        // --- Controles UI ---
        private TabControl tabControlPrincipal;
        private TabPage tabVentasDetalladas;
        private TabPage tabCorteCaja;

        // Controles para Ventas Detalladas
        private DateTimePicker dtpInicioVentas;
        private DateTimePicker dtpFinVentas;
        private Button btnGenerarReporteVentas;
        private Button btnExportarVentasCSV;
        private DataGridView dgvVentasDetalladas;

        // Controles para Corte de Caja
        private DateTimePicker dtpFechaCorte;
        private Button btnGenerarCorte;
        private Button btnExportarCortePDF;
        private DataGridView dgvCorteCaja;
        private Label lblTotalCorte;


        public FormVerReportes(Empleado empleado)
        {
            _empleado = empleado;
            this.Text = "Módulo de Reportes";
        private void InitializeComponent()
        {
            this.Text = "Gestión de Reportes";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(1000, 700);
            this.BackColor = Color.White;
            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            // TabControl principal
            tabControlPrincipal = new TabControl { Dock = DockStyle.Fill };
            tabVentasDetalladas = new TabPage("Reporte de Ventas Detalladas");
            tabCorteCaja = new TabPage("Corte de Caja Diario");

            tabControlPrincipal.TabPages.Add(tabVentasDetalladas);
            tabControlPrincipal.TabPages.Add(tabCorteCaja);

            this.Controls.Add(tabControlPrincipal);

            // Inicializar Pestañas
            InicializarTabVentas();
            InicializarTabCorteCaja();
        }

        private void InicializarTabVentas()
        {
            var panelSuperiorVentas = new FlowLayoutPanel { Dock = DockStyle.Top, Padding = new Padding(10), Height = 60, WrapContents = false };
            dtpInicioVentas = new DateTimePicker { Value = DateTime.Now.Date, Format = DateTimePickerFormat.Short, Width = 120 };
            dtpFinVentas = new DateTimePicker { Value = DateTime.Now.Date.AddDays(1).AddSeconds(-1), Format = DateTimePickerFormat.Short, Width = 120 };
            btnGenerarReporteVentas = new Button { Text = "Generar Reporte", Width = 150, Height = 30, BackColor = Color.DodgerBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnExportarVentasCSV = new Button { Text = "Exportar a CSV", Width = 150, Height = 30, BackColor = Color.SeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false };

            panelSuperiorVentas.Controls.AddRange(new Control[] {
                new Label { Text = "Desde:", AutoSize = true, Margin = new Padding(5, 5, 0, 0) }, dtpInicioVentas,
                new Label { Text = "Hasta:", AutoSize = true, Margin = new Padding(10, 5, 0, 0) }, dtpFinVentas,
                btnGenerarReporteVentas, btnExportarVentasCSV
            });

            dgvVentasDetalladas = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true, BackgroundColor = Color.WhiteSmoke };

            tabVentasDetalladas.Controls.Add(dgvVentasDetalladas);
            tabVentasDetalladas.Controls.Add(panelSuperiorVentas);

            // Eventos
            btnGenerarReporteVentas.Click += BtnGenerarReporteVentas_Click;
            btnExportarVentasCSV.Click += BtnExportarVentasCSV_Click;
        }

        private void InicializarTabCorteCaja()
        {
            var panelSuperiorCorte = new FlowLayoutPanel { Dock = DockStyle.Top, Padding = new Padding(10), Height = 60, WrapContents = false };
            dtpFechaCorte = new DateTimePicker { Value = DateTime.Now.Date, Format = DateTimePickerFormat.Short, Width = 120 };
            btnGenerarCorte = new Button { Text = "Generar Corte", Width = 150, Height = 30, BackColor = Color.DodgerBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnExportarCortePDF = new Button { Text = "Exportar a PDF", Width = 150, Height = 30, BackColor = Color.IndianRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false };

            panelSuperiorCorte.Controls.AddRange(new Control[] {
                new Label { Text = "Fecha:", AutoSize = true, Margin = new Padding(5, 5, 0, 0) }, dtpFechaCorte,
                btnGenerarCorte, btnExportarCortePDF
            });

            dgvCorteCaja = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true, BackgroundColor = Color.WhiteSmoke };

            lblTotalCorte = new Label { Dock = DockStyle.Bottom, Text = "Total del Día: $0.00", Font = new Font("Segoe UI", 14, FontStyle.Bold), Padding = new Padding(10), Height = 50, TextAlign = ContentAlignment.MiddleRight, BackColor = Color.LightGray };

            tabCorteCaja.Controls.Add(dgvCorteCaja);
            tabCorteCaja.Controls.Add(lblTotalCorte);
            tabCorteCaja.Controls.Add(panelSuperiorCorte);

            // Eventos
            btnGenerarCorte.Click += BtnGenerarCorte_Click;
            btnExportarCortePDF.Click += BtnExportarCortePDF_Click;
        }

        private void BtnGenerarReporteVentas_Click(object sender, EventArgs e)
        }

        private void InicializarTabVentas()
            // TabControl Principal
            tabControlPrincipal = new TabControl { Dock = DockStyle.Fill };
            tabPageReportesGuardados = new TabPage { Text = "Reportes Guardados" };
            tabPageGenerarReporte = new TabPage { Text = "Generar Reporte" };

            tabControlPrincipal.TabPages.Add(tabPageReportesGuardados);
            tabControlPrincipal.TabPages.Add(tabPageGenerarReporte);

            // --- Pestaña 1: Reportes Guardados ---
            btnExportarGuardados = new Button { Text = "Exportar a CSV", Location = new Point(10, 10), Width = 120, Height = 25, Enabled = false };
            btnExportarGuardados.Click += BtnExportarGuardados_Click;

            dgvReportesGuardados = new DataGridView {
                Location = new Point(10, 45),
                Size = new Size(860, 480),
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.Fixed3D
            };

            var panelGuardados = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            panelGuardados.Controls.Add(dgvReportesGuardados);
            panelGuardados.Controls.Add(btnExportarGuardados);
            dgvReportesGuardados.BringToFront(); // Asegurarse que el dgv está por encima
            tabPageReportesGuardados.Controls.Add(panelGuardados);

            // --- Pestaña 2: Generar Reporte ---
            var panelGenerarControles = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(10) };

            dtpDesde = new DateTimePicker { Location = new Point(60, 10), Format = DateTimePickerFormat.Short };
            dtpHasta = new DateTimePicker { Location = new Point(320, 10), Format = DateTimePickerFormat.Short };
            btnGenerarReporte = new Button { Text = "Generar Reporte de Ventas", Location = new Point(500, 10), Width = 180 };
            btnGenerarCorteCaja = new Button { Text = "Generar Corte de Caja (Hoy)", Location = new Point(690, 10), Width = 180 };

            btnExportarNuevo = new Button { Text = "Exportar a Excel", Location = new Point(10, 45), Width = 120, Enabled = false };
            btnGuardarReporte = new Button { Text = "Guardar Reporte", Location = new Point(140, 45), Width = 120, Enabled = false };

            panelGenerarControles.Controls.AddRange(new Control[] { new Label {Text="Desde:", Location = new Point(10, 13)}, dtpDesde, new Label {Text="Hasta:", Location = new Point(270, 13)}, dtpHasta, btnGenerarReporte, btnGenerarCorteCaja, btnExportarNuevo, btnGuardarReporte });

            dgvVentasDetalladas = new DataGridView {
                Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.Fixed3D
            };

            var panelGenerarGrid = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 0, 10, 10) };
            panelGenerarGrid.Controls.Add(dgvVentasDetalladas);

            tabPageGenerarReporte.Controls.Add(panelGenerarGrid);
            tabPageGenerarReporte.Controls.Add(panelGenerarControles);

            this.Controls.Add(tabControlPrincipal);

            ConfigurarDgvReportesGuardados();
            ConfigurarDgvVentasDetalladas();

            // Conectar eventos para la nueva funcionalidad
            btnGenerarReporte.Click += BtnGenerarReporte_Click;
            btnExportarNuevo.Click += BtnExportarNuevo_Click;
            btnGenerarCorteCaja.Click += BtnGenerarCorteCaja_Click;
            btnGuardarReporte.Click += BtnGuardarReporte_Click;
        }

        private void BtnGuardarReporte_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvVentasDetalladas.Rows.Count == 0)
                {
                    MessageBox.Show("No hay datos de reporte para guardar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                decimal totalVentas = 0;
                foreach (DataGridViewRow row in dgvVentasDetalladas.Rows)
                {
                    totalVentas += Convert.ToDecimal(row.Cells["Subtotal"].Value);
                }

                var nuevoReporte = new Reporte
                {
                    FechaGeneracion = DateTime.Now,
                    FechaInicio = dtpDesde.Value.Date,
                    FechaFin = dtpHasta.Value.Date,
                    TotalVentas = totalVentas,
                    GeneradoPorEmpleadoId = _empleadoActual.IdEmpleado
                };

                _reporteBLL.GuardarReporte(nuevoReporte);

                MessageBox.Show("Reporte guardado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Actualizar la otra pestaña
                CargarReportesGuardados();
                btnGuardarReporte.Enabled = false; // Deshabilitar después de guardar
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el reporte: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGenerarCorteCaja_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime hoy = DateTime.Today;
                var totales = _reporteBLL.ObtenerTotalesCorteCaja(hoy);

                if (totales.Item1 == 0 && totales.Item2 == 0)
                {
                    MessageBox.Show("No se encontraron ventas registradas hoy.", "Corte de Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "Archivo PDF (*.pdf)|*.pdf",
                    FileName = $"Corte_Caja_{hoy:yyyyMMdd}.pdf"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    _reporteBLL.GenerarCorteDeCajaPdf(totales, hoy, sfd.FileName);
                    MessageBox.Show("Corte de caja generado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el corte de caja: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportarNuevo_Click(object sender, EventArgs e)
        {
            if (dgvVentasDetalladas.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var reportData = _reporteBLL.GenerarReporteVentasDetallado(dtpInicioVentas.Value, dtpFinVentas.Value);
                dgvVentasDetalladas.DataSource = reportData;
                dgvVentasDetalladas.Tag = reportData; // Guardar datos para exportación
                btnExportarVentasCSV.Enabled = reportData.Count > 0;
                if (reportData.Count == 0)
                {
                    MessageBox.Show("No se encontraron ventas en el rango de fechas seleccionado.", "Sin Resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar el reporte de ventas: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportarVentasCSV_Click(object sender, EventArgs e)
        {
            var data = dgvVentasDetalladas.Tag as List<VentaDetalladaReporte>;
            if (data == null || data.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var sfd = new SaveFileDialog { Filter = "CSV file (*.csv)|*.csv", FileName = $"ReporteVentas_{DateTime.Now:yyyyMMdd}.csv" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _reporteBLL.ExportarVentasDetalladasCSV(data, sfd.FileName);
                        MessageBox.Show("Reporte exportado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al exportar a CSV: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnGenerarCorte_Click(object sender, EventArgs e)
        {
            try
            {
                var corteData = _reporteBLL.GenerarCorteCajaDiario(dtpFechaCorte.Value);
                dgvCorteCaja.DataSource = corteData.Ventas;
                dgvCorteCaja.Tag = corteData; // Guardar todo el objeto para el PDF
                lblTotalCorte.Text = $"Total del Día: {corteData.TotalDia:C2} | Efectivo: {corteData.TotalEfectivo:C2} | Tarjeta: {corteData.TotalTarjeta:C2}";
                btnExportarCortePDF.Enabled = corteData.Ventas.Count > 0;

                if (corteData.Ventas.Count == 0)
                {
                    MessageBox.Show("No se encontraron ventas para la fecha seleccionada.", "Sin Resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "Archivos CSV (*.csv)|*.csv",
                    FileName = $"Reporte_Ventas_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    // Lógica de exportación manual
                    using (var sw = new System.IO.StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                    {
                        // Encabezados
                        var headers = dgvVentasDetalladas.Columns.Cast<DataGridViewColumn>();
                        sw.WriteLine(string.Join(",", headers.Select(c => $"\"{c.HeaderText}\"")));

                        // Filas
                        foreach (DataGridViewRow row in dgvVentasDetalladas.Rows)
                        {
                            var cells = row.Cells.Cast<DataGridViewCell>();
                            sw.WriteLine(string.Join(",", cells.Select(c => $"\"{c.Value}\"")));
                        }
                    }
                    MessageBox.Show("Reporte exportado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar el reporte: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarDgvVentasDetalladas()
        {
            dgvVentasDetalladas.Columns.Clear();
            dgvVentasDetalladas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvVentasDetalladas.Columns.Add("VentaID", "ID Venta");
            dgvVentasDetalladas.Columns.Add("FechaVenta", "Fecha");
            dgvVentasDetalladas.Columns.Add("NombreProducto", "Producto");
            dgvVentasDetalladas.Columns.Add("Cantidad", "Cantidad");
            dgvVentasDetalladas.Columns.Add("PrecioUnitario", "Precio Unit.");
            dgvVentasDetalladas.Columns.Add("Subtotal", "Subtotal");
            dgvVentasDetalladas.Columns.Add("MetodoPago", "Método de Pago");
            dgvVentasDetalladas.Columns.Add("NombreEmpleado", "Atendido Por");

            dgvVentasDetalladas.Columns["PrecioUnitario"].DefaultCellStyle.Format = "C2";
            dgvVentasDetalladas.Columns["Subtotal"].DefaultCellStyle.Format = "C2";
        }

        private void ConfigurarDgvReportesGuardados()
        {
            var panelSuperiorVentas = new FlowLayoutPanel { Dock = DockStyle.Top, Padding = new Padding(10), Height = 60, WrapContents = false };
            dtpInicioVentas = new DateTimePicker { Value = DateTime.Now.Date, Format = DateTimePickerFormat.Short, Width = 120 };
            dtpFinVentas = new DateTimePicker { Value = DateTime.Now.Date.AddDays(1).AddSeconds(-1), Format = DateTimePickerFormat.Short, Width = 120 };
            btnGenerarReporteVentas = new Button { Text = "Generar Reporte", Width = 150, Height = 30, BackColor = Color.DodgerBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnExportarVentasCSV = new Button { Text = "Exportar a CSV", Width = 150, Height = 30, BackColor = Color.SeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false };

            panelSuperiorVentas.Controls.AddRange(new Control[] {
                new Label { Text = "Desde:", AutoSize = true, Margin = new Padding(5, 5, 0, 0) }, dtpInicioVentas,
                new Label { Text = "Hasta:", AutoSize = true, Margin = new Padding(10, 5, 0, 0) }, dtpFinVentas,
                btnGenerarReporteVentas, btnExportarVentasCSV
            });

            dgvVentasDetalladas = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true, BackgroundColor = Color.WhiteSmoke };

            tabVentasDetalladas.Controls.Add(dgvVentasDetalladas);
            tabVentasDetalladas.Controls.Add(panelSuperiorVentas);

            // Eventos
            btnGenerarReporteVentas.Click += BtnGenerarReporteVentas_Click;
            btnExportarVentasCSV.Click += BtnExportarVentasCSV_Click;
        }

        private void InicializarTabCorteCaja()
        {
            var panelSuperiorCorte = new FlowLayoutPanel { Dock = DockStyle.Top, Padding = new Padding(10), Height = 60, WrapContents = false };
            dtpFechaCorte = new DateTimePicker { Value = DateTime.Now.Date, Format = DateTimePickerFormat.Short, Width = 120 };
            btnGenerarCorte = new Button { Text = "Generar Corte", Width = 150, Height = 30, BackColor = Color.DodgerBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnExportarCortePDF = new Button { Text = "Exportar a PDF", Width = 150, Height = 30, BackColor = Color.IndianRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false };

            panelSuperiorCorte.Controls.AddRange(new Control[] {
                new Label { Text = "Fecha:", AutoSize = true, Margin = new Padding(5, 5, 0, 0) }, dtpFechaCorte,
                btnGenerarCorte, btnExportarCortePDF
            });

            dgvCorteCaja = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true, BackgroundColor = Color.WhiteSmoke };

            lblTotalCorte = new Label { Dock = DockStyle.Bottom, Text = "Total del Día: $0.00", Font = new Font("Segoe UI", 14, FontStyle.Bold), Padding = new Padding(10), Height = 50, TextAlign = ContentAlignment.MiddleRight, BackColor = Color.LightGray };

            tabCorteCaja.Controls.Add(dgvCorteCaja);
            tabCorteCaja.Controls.Add(lblTotalCorte);
            tabCorteCaja.Controls.Add(panelSuperiorCorte);

            // Eventos
            btnGenerarCorte.Click += BtnGenerarCorte_Click;
            btnExportarCortePDF.Click += BtnExportarCortePDF_Click;
        }

        private void BtnGenerarReporteVentas_Click(object sender, EventArgs e)
            CargarReportesGuardados();
        }

        private void CargarReportesGuardados()
        {
            try
            {
                var reportData = _reporteBLL.GenerarReporteVentasDetallado(dtpInicioVentas.Value, dtpFinVentas.Value);
                dgvVentasDetalladas.DataSource = reportData;
                dgvVentasDetalladas.Tag = reportData; // Guardar datos para exportación
                btnExportarVentasCSV.Enabled = reportData.Count > 0;
                if (reportData.Count == 0)
                {
                    MessageBox.Show("No se encontraron ventas en el rango de fechas seleccionado.", "Sin Resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar el corte de caja: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportarCortePDF_Click(object sender, EventArgs e)
                MessageBox.Show("Error al generar el reporte de ventas: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportarVentasCSV_Click(object sender, EventArgs e)
        {
            var data = dgvVentasDetalladas.Tag as List<VentaDetalladaReporte>;
            if (data == null || data.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var sfd = new SaveFileDialog { Filter = "CSV file (*.csv)|*.csv", FileName = $"ReporteVentas_{DateTime.Now:yyyyMMdd}.csv" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _reporteBLL.ExportarVentasDetalladasCSV(data, sfd.FileName);
                        MessageBox.Show("Reporte exportado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al exportar a CSV: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    btnExportarGuardados.Enabled = true;
                }
            }
        }

        private void BtnGenerarCorte_Click(object sender, EventArgs e)
        {
            var data = dgvCorteCaja.Tag as CorteCaja;
            if (data == null || data.Ventas.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
                var corteData = _reporteBLL.GenerarCorteCajaDiario(dtpFechaCorte.Value);
                dgvCorteCaja.DataSource = corteData.Ventas;
                dgvCorteCaja.Tag = corteData; // Guardar todo el objeto para el PDF
                lblTotalCorte.Text = $"Total del Día: {corteData.TotalDia:C2} | Efectivo: {corteData.TotalEfectivo:C2} | Tarjeta: {corteData.TotalTarjeta:C2}";
                btnExportarCortePDF.Enabled = corteData.Ventas.Count > 0;

                if (corteData.Ventas.Count == 0)
                {
                    MessageBox.Show("No se encontraron ventas para la fecha seleccionada.", "Sin Resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // No mostrar mensaje si la tabla está vacía, simplemente no habilitar el botón
                    btnExportarGuardados.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar el corte de caja: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportarCortePDF_Click(object sender, EventArgs e)
        private void BtnExportarGuardados_Click(object sender, EventArgs e)
        {
            var data = dgvCorteCaja.Tag as CorteCaja;
            if (data == null || data.Ventas.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
                List<Reporte> reportes = _reporteBLL.ObtenerTodosLosReportes();

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Archivos CSV (*.csv)|*.csv";
                sfd.FileName = $"Reportes_Guardados_{DateTime.Now:yyyyMMdd}.csv";

            using (var sfd = new SaveFileDialog { Filter = "PDF file (*.pdf)|*.pdf", FileName = $"CorteCaja_{dtpFechaCorte.Value:yyyyMMdd}.pdf" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _reporteBLL.ExportarCorteCajaPDF(data, sfd.FileName);
                        MessageBox.Show("Corte de caja exportado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al exportar a PDF: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
                }
            }
        }

        private void BtnGenerarReporte_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime desde = dtpDesde.Value.Date;
                DateTime hasta = dtpHasta.Value.Date.AddDays(1).AddSeconds(-1); // Incluir todo el día

                List<VentaDetalladaReporte> ventas = _reporteBLL.ObtenerVentasDetalladasPorFecha(desde, hasta);
                dgvVentasDetalladas.Rows.Clear();

                if (ventas != null && ventas.Any())
                {
                    foreach (var venta in ventas)
                    {
                        dgvVentasDetalladas.Rows.Add(
                            venta.VentaID,
                            venta.FechaVenta,
                            venta.NombreProducto,
                            venta.Cantidad,
                            venta.PrecioUnitario,
                            venta.Subtotal,
                            venta.MetodoPago,
                            venta.NombreEmpleado
                        );
                    }
                    btnExportarNuevo.Enabled = true;
                    btnGuardarReporte.Enabled = true;
                }
                else
                {
                    MessageBox.Show("No se encontraron ventas en el rango de fechas seleccionado.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnExportarNuevo.Enabled = false;
                    btnGuardarReporte.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el reporte: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
