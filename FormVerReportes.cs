using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using KIOSKO_Proyecto.BLL;
using KIOSKO_Proyecto.Modelos;

namespace KIOSKO_Proyecto
{
    public class FormVerReportes : Form
    {
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

        public FormVerReportes(Empleado empleado)
        {
            _empleadoActual = empleado;
            InitializeComponent();
            this.Load += FormVerReportes_Load;
        }

        private void InitializeComponent()
        {
            this.Text = "Gestión de Reportes";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(900, 600);

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
            dgvReportesGuardados.Columns.Clear();
            dgvReportesGuardados.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReportesGuardados.Columns.Add("IdReporte", "ID Reporte");
            dgvReportesGuardados.Columns.Add("FechaGeneracion", "Fecha de Generación");
            dgvReportesGuardados.Columns.Add("FechaInicio", "Desde");
            dgvReportesGuardados.Columns.Add("FechaFin", "Hasta");
            dgvReportesGuardados.Columns.Add("TotalVentas", "Total Ventas");
            dgvReportesGuardados.Columns.Add("NombreEmpleadoGenerador", "Generado Por");

            dgvReportesGuardados.Columns["IdReporte"].Width = 80;
            dgvReportesGuardados.Columns["TotalVentas"].DefaultCellStyle.Format = "C2";
            dgvReportesGuardados.Columns["TotalVentas"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        private void FormVerReportes_Load(object sender, EventArgs e)
        {
            CargarReportesGuardados();
        }

        private void CargarReportesGuardados()
        {
            try
            {
                List<Reporte> reportes = _reporteBLL.ObtenerTodosLosReportes();
                dgvReportesGuardados.Rows.Clear();

                if (reportes != null && reportes.Count > 0)
                {
                    foreach (var reporte in reportes)
                    {
                        dgvReportesGuardados.Rows.Add(
                            reporte.IdReporte,
                            reporte.FechaGeneracion,
                            reporte.FechaInicio,
                            reporte.FechaFin,
                            reporte.TotalVentas,
                            reporte.NombreEmpleadoGenerador
                        );
                    }
                    btnExportarGuardados.Enabled = true;
                }
                else
                {
                    // No mostrar mensaje si la tabla está vacía, simplemente no habilitar el botón
                    btnExportarGuardados.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los reportes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportarGuardados_Click(object sender, EventArgs e)
        {
            try
            {
                List<Reporte> reportes = _reporteBLL.ObtenerTodosLosReportes();

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Archivos CSV (*.csv)|*.csv";
                sfd.FileName = $"Reportes_Guardados_{DateTime.Now:yyyyMMdd}.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    _reporteBLL.ExportarReportesCSV(reportes, sfd.FileName);
                    MessageBox.Show("Reportes exportados exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar los reportes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
