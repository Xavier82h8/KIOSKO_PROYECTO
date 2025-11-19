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
        private DataGridView dgvReportesGuardados;
        private Button btnExportar;
        private ReporteBLL _reporteBLL = new ReporteBLL();

        public FormVerReportes()
        {
            this.Text = "Reportes Guardados";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(900, 600);

            // Export button
            btnExportar = new Button { Text = "Exportar a CSV", Location = new Point(12, 10), Width = 120, Height = 25, Enabled = false };
            btnExportar.Click += BtnExportar_Click;

            // DataGridView for saved reports
            dgvReportesGuardados = new DataGridView
            {
                Location = new Point(12, 45),
                Size = new Size(860, 500),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            ConfigurarDgv();

            this.Controls.Add(btnExportar);
            this.Controls.Add(dgvReportesGuardados);

            this.Load += FormVerReportes_Load;
        }

        private void ConfigurarDgv()
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
            CargarReportes();
        }

        private void CargarReportes()
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
                    btnExportar.Enabled = true;
                }
                else
                {
                    MessageBox.Show("No se encontraron reportes guardados.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnExportar.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los reportes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportar_Click(object sender, EventArgs e)
        {
            try
            {
                List<Reporte> reportes = (List<Reporte>)dgvReportesGuardados.Tag;
                if (reportes == null) {
                    reportes = _reporteBLL.ObtenerTodosLosReportes();
                }

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
    }
}
