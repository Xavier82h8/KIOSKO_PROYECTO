using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using KIOSKO_Proyecto.BLL;
using KIOSKO_Proyecto.Modelos;

namespace KIOSKO_Proyecto
{
    public class FormHistorialVentas : Form
    {
        private DataGridView dgvVentas;
        private Button btnExportarPDF;
        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private Button btnFiltrar;
        private VentaBLL ventaBLL = new VentaBLL();
        private List<Venta> ventasMostradas;

        public FormHistorialVentas()
        {
            this.Text = "Historial de Ventas";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(1000, 700);
            this.BackColor = Color.FromArgb(248, 249, 250);

            // Controles de filtrado
            var panelFiltros = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(10) };
            dtpDesde = new DateTimePicker { Value = DateTime.Now.Date, Location = new Point(60, 15) };
            dtpHasta = new DateTimePicker { Value = DateTime.Now.Date.AddDays(1).AddSeconds(-1), Location = new Point(280, 15) };
            btnFiltrar = new Button { Text = "Filtrar", Location = new Point(500, 15) };
            btnExportarPDF = new Button { Text = "Exportar Ticket a PDF", Enabled = false, Location = new Point(600, 15), BackColor = Color.FromArgb(217, 83, 79), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            
            panelFiltros.Controls.Add(new Label { Text = "Desde:", Location = new Point(10, 18), AutoSize = true });
            panelFiltros.Controls.Add(dtpDesde);
            panelFiltros.Controls.Add(new Label { Text = "Hasta:", Location = new Point(230, 18), AutoSize = true });
            panelFiltros.Controls.Add(dtpHasta);
            panelFiltros.Controls.Add(btnFiltrar);
            panelFiltros.Controls.Add(btnExportarPDF);
            this.Controls.Add(panelFiltros);

            // DataGridView para las ventas
            dgvVentas = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            ConfigurarDgv();
            this.Controls.Add(dgvVentas);
            dgvVentas.BringToFront();

            // Eventos
            btnFiltrar.Click += BtnFiltrar_Click;
            btnExportarPDF.Click += BtnExportarPDF_Click;
            dgvVentas.SelectionChanged += DgvVentas_SelectionChanged;
            this.Load += (s, e) => BtnFiltrar_Click(s, e); // Carga inicial
        }

        private void ConfigurarDgv()
        {
            dgvVentas.Columns.Clear();
            dgvVentas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvVentas.Columns.Add("VentaID", "ID Venta");
            dgvVentas.Columns.Add("FechaVenta", "Fecha y Hora");
            dgvVentas.Columns.Add("TotalVenta", "Total");
            dgvVentas.Columns.Add("MontoEfectivo", "Pagado (Efectivo)");
            dgvVentas.Columns.Add("MontoTarjeta", "Pagado (Tarjeta)");
            dgvVentas.Columns.Add("Cambio", "Cambio");

            dgvVentas.Columns["VentaID"].FillWeight = 10;
            dgvVentas.Columns["FechaVenta"].FillWeight = 25;
            dgvVentas.Columns["TotalVenta"].DefaultCellStyle.Format = "C2";
            dgvVentas.Columns["MontoEfectivo"].DefaultCellStyle.Format = "C2";
            dgvVentas.Columns["MontoTarjeta"].DefaultCellStyle.Format = "C2";
            dgvVentas.Columns["Cambio"].DefaultCellStyle.Format = "C2";
        }

        private void BtnFiltrar_Click(object sender, EventArgs e)
        {
            try
            {
                ventasMostradas = ventaBLL.ObtenerVentasPorRango(dtpDesde.Value, dtpHasta.Value);
                dgvVentas.Rows.Clear();

                if (ventasMostradas != null && ventasMostradas.Count > 0)
                {
                    foreach (var venta in ventasMostradas)
                    {
                        dgvVentas.Rows.Add(
                            venta.VentaID,
                            venta.FechaVenta,
                            venta.TotalVenta,
                            venta.MontoEfectivo,
                            venta.MontoTarjeta,
                            venta.Cambio
                        );
                    }
                }
                else
                {
                    MessageBox.Show("No se encontraron ventas en el rango de fechas seleccionado.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las ventas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvVentas_SelectionChanged(object sender, EventArgs e)
        {
            btnExportarPDF.Enabled = dgvVentas.SelectedRows.Count > 0;
        }

        private void BtnExportarPDF_Click(object sender, EventArgs e)
        {
            if (dgvVentas.SelectedRows.Count == 0) return;

            int ventaId = Convert.ToInt32(dgvVentas.SelectedRows[0].Cells["VentaID"].Value);
            Venta ventaSeleccionada = ventasMostradas.Find(v => v.VentaID == ventaId);
            
            if (ventaSeleccionada == null) return;

            // Cargar detalles de la venta
            ventaSeleccionada.Detalles = ventaBLL.ObtenerDetalleVenta(ventaId);

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Archivo PDF (*.pdf)|*.pdf",
                FileName = $"Ticket_Venta_{ventaId}.pdf"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Aquí llamamos al BLL para generar el PDF
                    ventaBLL.ExportarTicketPDF(ventaSeleccionada, sfd.FileName);
                    MessageBox.Show("Ticket exportado a PDF exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar el ticket: {ex.Message}\n\nAsegúrese de tener instalado el paquete NuGet 'iTextSharp'.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}