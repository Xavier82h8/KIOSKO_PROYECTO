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
        // Variables globales de la clase
        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private Button btnGenerar;
        private Button btnExportarCSV;
        private Button btnGenerarCortePdf;
        private Label lblTotalVentas;
        private DataGridView dgvReporte;

        private VentaBLL _ventaBLL = new VentaBLL();
        private ReporteBLL _reporteBLL = new ReporteBLL();
        private List<VentaDetalladaReporte> _reporteActual;

        // Agregamos esta variable para usarla al generar el PDF
        private Modelos.Empleado _empleado;

        public FormReportes(Modelos.Empleado empleado)
        {
            _empleado = empleado; // Guardamos el empleado

            this.Text = "Reporte Detallado de Ventas";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(1000, 700);

            // --- Construcción de la Interfaz ---
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10) };
            dtpDesde = new DateTimePicker { Value = DateTime.Today.AddDays(-7), Location = new Point(60, 12), Format = DateTimePickerFormat.Short };
            dtpHasta = new DateTimePicker { Value = DateTime.Today, Location = new Point(280, 12), Format = DateTimePickerFormat.Short };
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

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(10) };
            lblTotalVentas = new Label { Text = "Total ventas: $0.00", AutoSize = true, Location = new Point(10, 15), Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            bottomPanel.Controls.Add(lblTotalVentas);

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

            dgvReporte.Columns["PrecioUnitario"].DefaultCellStyle.Format = "C2";
            dgvReporte.Columns["Subtotal"].DefaultCellStyle.Format = "C2";
        }

        private void BtnGenerar_Click(object sender, EventArgs e)
        {
            var desde = dtpDesde.Value.Date;
            var hasta = dtpHasta.Value.Date.AddDays(1).AddTicks(-1);

            try
            {
                // Asumiendo que VentaBLL tiene este método. Si no, usa _reporteBLL.GenerarReporteVentasDetallado
                _reporteActual = _reporteBLL.GenerarReporteVentasDetallado(desde, hasta);
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
            if (_reporteActual == null || !_reporteActual.Any()) return;

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Archivo CSV (*.csv)|*.csv",
                FileName = $"Reporte_Detallado_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _reporteBLL.ExportarVentasDetalladasCSV(_reporteActual, sfd.FileName);
                    MessageBox.Show("Reporte exportado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar a CSV: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // --- AQUÍ ESTÁ LA SOLUCIÓN AL ERROR DE VARIABLES ---
        private void BtnGenerarCortePdf_Click(object sender, EventArgs e)
        {
            if (_reporteActual == null || !_reporteActual.Any()) return;

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Archivo PDF (*.pdf)|*.pdf",
                FileName = $"Reporte_Ventas_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // 1. CREAR LOS OBJETOS NECESARIOS AL VUELO
                    // Usamos la fecha "Hasta" como referencia para el corte simulado
                    var corteData = _reporteBLL.GenerarCorteCajaDiario(dtpHasta.Value);

                    // 2. Crear un historial "Dummy" (Simulado)
                    // Como este form no hace arqueo manual, asumimos que Sistema = Real para poder imprimir
                    var historialDummy = new HistorialCorte
                    {
                        IdEmpleado = _empleado.IdEmpleado,
                        NombreEmpleado = _empleado.NombreEmp,
                        FechaCorte = DateTime.Now,
                        TotalSistema = corteData.TotalDia,
                        TotalReal = corteData.TotalDia, // Asumimos exactitud para el reporte histórico
                        Diferencia = 0,
                        TotalEfectivo = corteData.TotalEfectivo,
                        TotalTarjeta = corteData.TotalTarjeta,
                        Comentarios = "Reporte generado desde Historial de Ventas"
                    };

                    // 3. LLAMAR AL MÉTODO CON LOS OBJETOS CREADOS
                    _reporteBLL.ExportarCorteCajaPDF(corteData, historialDummy, sfd.FileName);

                    MessageBox.Show("Reporte PDF exportado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al generar el PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}