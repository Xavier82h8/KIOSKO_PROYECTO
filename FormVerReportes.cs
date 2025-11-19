using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using KIOSKO_Proyecto.BLL;
using KIOSKO_Proyecto.Modelos;

namespace KIOSKO_Proyecto
{
    public partial class FormVerReportes : Form
    {
        private Empleado _empleadoActual;
        private ReporteBLL _reporteBLL = new ReporteBLL();

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
            _empleadoActual = empleado;
            this.Text = "Módulo de Reportes";
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
            var panelSuperiorVentas = new FlowLayoutPanel 
            { 
                Dock = DockStyle.Top, 
                Padding = new Padding(10), 
                Height = 60, 
                WrapContents = false 
            };
            
            dtpInicioVentas = new DateTimePicker 
            { 
                Value = DateTime.Now.Date, 
                Format = DateTimePickerFormat.Short, 
                Width = 120 
            };
            
            dtpFinVentas = new DateTimePicker 
            { 
                Value = DateTime.Now.Date.AddDays(1).AddSeconds(-1), 
                Format = DateTimePickerFormat.Short, 
                Width = 120 
            };
            
            btnGenerarReporteVentas = new Button 
            { 
                Text = "Generar Reporte", 
                Width = 150, 
                Height = 30, 
                BackColor = Color.DodgerBlue, 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat 
            };
            
            btnExportarVentasCSV = new Button 
            { 
                Text = "Exportar a CSV", 
                Width = 150, 
                Height = 30, 
                BackColor = Color.SeaGreen, 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, 
                Enabled = false 
            };

            panelSuperiorVentas.Controls.AddRange(new Control[] {
                new Label { Text = "Desde:", AutoSize = true, Margin = new Padding(5, 5, 0, 0) }, 
                dtpInicioVentas,
                new Label { Text = "Hasta:", AutoSize = true, Margin = new Padding(10, 5, 0, 0) }, 
                dtpFinVentas,
                btnGenerarReporteVentas, 
                btnExportarVentasCSV
            });

            dgvVentasDetalladas = new DataGridView 
            { 
                Dock = DockStyle.Fill, 
                AllowUserToAddRows = false, 
                ReadOnly = true, 
                BackgroundColor = Color.WhiteSmoke,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            tabVentasDetalladas.Controls.Add(dgvVentasDetalladas);
            tabVentasDetalladas.Controls.Add(panelSuperiorVentas);

            // Eventos
            btnGenerarReporteVentas.Click += BtnGenerarReporteVentas_Click;
            btnExportarVentasCSV.Click += BtnExportarVentasCSV_Click;
        }

        private void InicializarTabCorteCaja()
        {
            var panelSuperiorCorte = new FlowLayoutPanel 
            { 
                Dock = DockStyle.Top, 
                Padding = new Padding(10), 
                Height = 60, 
                WrapContents = false 
            };
            
            dtpFechaCorte = new DateTimePicker 
            { 
                Value = DateTime.Now.Date, 
                Format = DateTimePickerFormat.Short, 
                Width = 120 
            };
            
            btnGenerarCorte = new Button 
            { 
                Text = "Generar Corte", 
                Width = 150, 
                Height = 30, 
                BackColor = Color.DodgerBlue, 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat 
            };
            
            btnExportarCortePDF = new Button 
            { 
                Text = "Exportar a PDF", 
                Width = 150, 
                Height = 30, 
                BackColor = Color.IndianRed, 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, 
                Enabled = false 
            };

            panelSuperiorCorte.Controls.AddRange(new Control[] {
                new Label { Text = "Fecha:", AutoSize = true, Margin = new Padding(5, 5, 0, 0) }, 
                dtpFechaCorte,
                btnGenerarCorte, 
                btnExportarCortePDF
            });

            dgvCorteCaja = new DataGridView 
            { 
                Dock = DockStyle.Fill, 
                AllowUserToAddRows = false, 
                ReadOnly = true, 
                BackgroundColor = Color.WhiteSmoke,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            lblTotalCorte = new Label 
            { 
                Dock = DockStyle.Bottom, 
                Text = "Total del Día: $0.00", 
                Font = new Font("Segoe UI", 14, FontStyle.Bold), 
                Padding = new Padding(10), 
                Height = 50, 
                TextAlign = ContentAlignment.MiddleRight, 
                BackColor = Color.LightGray 
            };

            tabCorteCaja.Controls.Add(dgvCorteCaja);
            tabCorteCaja.Controls.Add(lblTotalCorte);
            tabCorteCaja.Controls.Add(panelSuperiorCorte);

            // Eventos
            btnGenerarCorte.Click += BtnGenerarCorte_Click;
            btnExportarCortePDF.Click += BtnExportarCortePDF_Click;
        }

        private void BtnGenerarReporteVentas_Click(object sender, EventArgs e)
        {
            try
            {
                var reportData = _reporteBLL.GenerarReporteVentasDetallado(dtpInicioVentas.Value, dtpFinVentas.Value);
                dgvVentasDetalladas.DataSource = reportData;
                dgvVentasDetalladas.Tag = reportData; // Guardar datos para exportación
                btnExportarVentasCSV.Enabled = reportData.Count > 0;
                
                if (reportData.Count == 0)
                {
                    MessageBox.Show("No se encontraron ventas en el rango de fechas seleccionado.", 
                        "Sin Resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar el reporte de ventas: " + ex.Message, 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportarVentasCSV_Click(object sender, EventArgs e)
        {
            var data = dgvVentasDetalladas.Tag as List<VentaDetalladaReporte>;
            if (data == null || data.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Aviso", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var sfd = new SaveFileDialog 
            { 
                Filter = "CSV file (*.csv)|*.csv", 
                FileName = $"ReporteVentas_{DateTime.Now:yyyyMMdd}.csv" 
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _reporteBLL.ExportarVentasDetalladasCSV(data, sfd.FileName);
                        MessageBox.Show("Reporte exportado exitosamente.", "Éxito", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al exportar a CSV: " + ex.Message, 
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                
                lblTotalCorte.Text = $"Total del Día: {corteData.TotalDia:C2} | " +
                                    $"Efectivo: {corteData.TotalEfectivo:C2} | " +
                                    $"Tarjeta: {corteData.TotalTarjeta:C2}";
                
                btnExportarCortePDF.Enabled = corteData.Ventas.Count > 0;

                if (corteData.Ventas.Count == 0)
                {
                    MessageBox.Show("No se encontraron ventas para la fecha seleccionada.", 
                        "Sin Resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar el corte de caja: " + ex.Message, 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportarCortePDF_Click(object sender, EventArgs e)
        {
            var data = dgvCorteCaja.Tag as CorteCaja;
            if (data == null || data.Ventas.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Aviso", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var sfd = new SaveFileDialog 
            { 
                Filter = "PDF file (*.pdf)|*.pdf", 
                FileName = $"CorteCaja_{dtpFechaCorte.Value:yyyyMMdd}.pdf" 
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _reporteBLL.ExportarCorteCajaPDF(data, sfd.FileName);
                        MessageBox.Show("Corte de caja exportado exitosamente.", "Éxito", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al exportar a PDF: " + ex.Message, 
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}