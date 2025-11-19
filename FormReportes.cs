using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using KIOSKO_Proyecto.Modelos;
using KIOSKO_Proyecto.BLL;
using System.IO;
using System.Text;

namespace KIOSKO_Proyecto
{
    public class FormReportes : Form
    {
        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private Button btnGenerar;
        private Button btnExportarCSV;
        private Button btnGenerarCortePdf;
        private Label lblTotalVentas;
        private DataGridView dgvReporte;
        
        private VentaBLL _ventaBLL = new VentaBLL();
        private ReporteBLL _reporteBLL = new ReporteBLL();
        private List<VentaDetalladaReporte> _reporteActual; // Store the current report data

        public FormReportes(Modelos.Empleado empleado)
        {
            this.Text = "Reporte Detallado de Ventas";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(1000, 700);

            // Top Panel for controls
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10) };
            dtpDesde = new DateTimePicker { Value = DateTime.Today.AddDays(-7), Location = new Point(60, 12) };
            dtpHasta = new DateTimePicker { Value = DateTime.Today, Location = new Point(280, 12) };
            btnGenerar = new Button { Text = "Generar Reporte", Location = new Point(500, 10), Width = 120, Height = 25 };
            btnExportarCSV = new Button { Text = "Exportar a CSV", Location = new Point(630, 10), Width = 120, Height = 25, Enabled = false };
            btnGenerarCortePdf = new Button { Text = "Corte de Caja (PDF)", Location = new Point(760, 10), Width = 150, Height = 25, Enabled = false };
            
            topPanel.Controls.Add(new Label { Text = "Desde:", Location = new Point(10, 15), AutoSize = true });
            topPanel.Controls.Add(dtpDesde);
            topPanel.Controls.Add(new Label { Text = "Hasta:", Location = new Point(230, 15), AutoSize = true });
            topPanel.Controls.Add(dtpHasta);
            topPanel.Controls.Add(btnGenerar);
            topPanel.Controls.Add(btnExportarCSV);
            topPanel.Controls.Add(btnGenerarCortePdf);
            
            // Bottom Panel for total
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(10) };
            lblTotalVentas = new Label { Text = "Total ventas: $0.00", AutoSize = true, Location = new Point(10, 15), Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            bottomPanel.Controls.Add(lblTotalVentas);

            // DataGridView for report
            dgvReporte = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };
            
            this.Controls.Add(dgvReporte);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(topPanel);

            // Event Handlers
            btnGenerar.Click += BtnGenerar_Click;
            btnExportarCSV.Click += BtnExportarCSV_Click;
            btnGenerarCortePdf.Click += BtnGenerarCortePdf_Click;
            
            ConfigurarDgvReporte();
        }

        private void ConfigurarDgvReporte()
        {
            dgvReporte.Columns.Clear();
            dgvReporte.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReporte.Columns.Add("VentaID", "ID Venta");
            dgvReporte.Columns.Add("FechaVenta", "Fecha");
            dgvReporte.Columns.Add("NombreEmpleado", "Empleado");
            dgvReporte.Columns.Add("NombreProducto", "Producto");
            dgvReporte.Columns.Add("Cantidad", "Cantidad");
            dgvReporte.Columns.Add("PrecioUnitario", "Precio Unit.");
            dgvReporte.Columns.Add("Subtotal", "Subtotal");
            dgvReporte.Columns.Add("MetodoPago", "Método Pago");

            // Adjust column styles
            dgvReporte.Columns["VentaID"].FillWeight = 8;
            dgvReporte.Columns["FechaVenta"].FillWeight = 15;
            dgvReporte.Columns["NombreEmpleado"].FillWeight = 15;
            dgvReporte.Columns["NombreProducto"].FillWeight = 20;
            dgvReporte.Columns["Cantidad"].FillWeight = 8;
            dgvReporte.Columns["PrecioUnitario"].FillWeight = 12;
            dgvReporte.Columns["Subtotal"].FillWeight = 12;
            dgvReporte.Columns["MetodoPago"].FillWeight = 10;

            dgvReporte.Columns["PrecioUnitario"].DefaultCellStyle.Format = "C2";
            dgvReporte.Columns["Subtotal"].DefaultCellStyle.Format = "C2";
        }

        private void BtnGenerar_Click(object sender, EventArgs e)
        {
            var desde = dtpDesde.Value.Date;
            var hasta = dtpHasta.Value.Date.AddDays(1).AddTicks(-1);

            try
            {
                _reporteActual = _ventaBLL.ObtenerVentasDetalladasPorRango(desde, hasta);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener el reporte detallado: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _reporteActual = new List<VentaDetalladaReporte>();
            }

            dgvReporte.Rows.Clear();
            if (_reporteActual != null && _reporteActual.Any())
            {
                foreach (var item in _reporteActual)
                {
                    dgvReporte.Rows.Add(
                        item.VentaID,
                        item.FechaVenta,
                        item.NombreEmpleado,
                        item.NombreProducto,
                        item.Cantidad,
                        item.PrecioUnitario,
                        item.Subtotal,
                        item.MetodoPago
                    );
                }
                
                decimal total = _reporteActual.Sum(item => item.Subtotal);
                lblTotalVentas.Text = $"Total ventas: {total:C2}";
                btnExportarCSV.Enabled = true;
                btnGenerarCortePdf.Enabled = true;
            }
            else
            {
                lblTotalVentas.Text = "Total ventas: $0.00";
                btnExportarCSV.Enabled = false;
                btnGenerarCortePdf.Enabled = false;
                MessageBox.Show("No se encontraron ventas para el rango de fechas seleccionado.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnExportarCSV_Click(object sender, EventArgs e)
        {
            if (_reporteActual == null || !_reporteActual.Any())
            {
                MessageBox.Show("No hay datos para exportar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Archivo CSV (*.csv)|*.csv",
                FileName = $"Reporte_Detallado_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var sb = new StringBuilder();
                    // Headers
                    var headers = dgvReporte.Columns.Cast<DataGridViewColumn>();
                    sb.AppendLine(string.Join(",", headers.Select(column => $"\"{column.HeaderText.Replace("\"", "\"\"")}\"").ToArray()));

                    // Rows
                    foreach (var item in _reporteActual)
                    {
                        sb.AppendLine(string.Join(",", new string[]
                        {
                            $"\"{item.VentaID}\"",
                            $"\"{item.FechaVenta}\"",
                            $"\"{item.NombreEmpleado.Replace("\"", "\"\"")}\"",
                            $"\"{item.NombreProducto.Replace("\"", "\"\"")}\"",
                            $"\"{item.Cantidad}\"",
                            $"\"{item.PrecioUnitario}\"",
                            $"\"{item.Subtotal}\"",
                            $"\"{item.MetodoPago.Replace("\"", "\"\"")}\""
                        }));
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Reporte exportado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar a CSV: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnGenerarCortePdf_Click(object sender, EventArgs e)
        {
            if (_reporteActual == null || !_reporteActual.Any())
            {
                MessageBox.Show("Primero debe generar un reporte.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Archivo PDF (*.pdf)|*.pdf",
                FileName = $"Corte_De_Caja_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _reporteBLL.GenerarCorteDeCajaPdf(_reporteActual, dtpDesde.Value, dtpHasta.Value, sfd.FileName);
                    MessageBox.Show("Reporte 'Corte de Caja' exportado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al generar el PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}