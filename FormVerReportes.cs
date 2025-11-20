using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using KIOSKO_Proyecto.BLL;
using KIOSKO_Proyecto.Modelos;
using Microsoft.VisualBasic; // ¡Asegúrate de agregar esta referencia!

namespace KIOSKO_Proyecto
{
    public partial class FormVerReportes : Form
    {
        private Empleado _empleado;
        private ReporteBLL _reporteBLL = new ReporteBLL();

        // --- Controles UI ---
        private TabControl tabControlPrincipal;
        private TabPage tabVentasDetalladas;
        private TabPage tabCorteCaja;

        private DateTimePicker dtpInicioVentas, dtpFinVentas;
        private Button btnGenerarReporteVentas, btnExportarVentasCSV;
        private DataGridView dgvVentasDetalladas;

        private DateTimePicker dtpFechaCorte;
        private Button btnGenerarCorte; 
        private DataGridView dgvCorteCaja;
        private Label lblTotalCorte;

        public FormVerReportes(Empleado empleado)
        {
            _empleado = empleado;
            this.Text = "Módulo de Reportes y Administración";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(1000, 700);
            this.BackColor = Color.White;
            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            tabControlPrincipal = new TabControl { Dock = DockStyle.Fill };
            tabVentasDetalladas = new TabPage("Reporte de Ventas");
            tabCorteCaja = new TabPage("Cierre de Caja (Arqueo)");

            tabControlPrincipal.TabPages.Add(tabVentasDetalladas);
            tabControlPrincipal.TabPages.Add(tabCorteCaja);

            this.Controls.Add(tabControlPrincipal);
            InicializarTabVentas();
            InicializarTabCorteCaja();
        }

        private void InicializarTabVentas()
        {
            var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Padding = new Padding(10), Height = 60, WrapContents = false };
            dtpInicioVentas = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 120 };
            dtpFinVentas = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 120 };
            btnGenerarReporteVentas = new Button { Text = "Buscar", BackColor = Color.DodgerBlue, ForeColor = Color.White, Height = 30 };
            btnExportarVentasCSV = new Button { Text = "Exportar CSV", BackColor = Color.SeaGreen, ForeColor = Color.White, Height = 30, Enabled = false };

            panel.Controls.AddRange(new Control[] { new Label { Text = "Del:", AutoSize = true, Margin = new Padding(0,5,0,0) }, dtpInicioVentas, new Label { Text = "Al:", AutoSize = true, Margin = new Padding(10,5,0,0) }, dtpFinVentas, btnGenerarReporteVentas, btnExportarVentasCSV });
            
            dgvVentasDetalladas = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.WhiteSmoke };
            
            tabVentasDetalladas.Controls.Add(dgvVentasDetalladas);
            tabVentasDetalladas.Controls.Add(panel);

            btnGenerarReporteVentas.Click += BtnGenerarReporteVentas_Click;
            btnExportarVentasCSV.Click += BtnExportarVentasCSV_Click;
        }

        private void InicializarTabCorteCaja()
        {
            var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Padding = new Padding(10), Height = 60, WrapContents = false };
            dtpFechaCorte = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 120 };
            btnGenerarCorte = new Button { Text = "Realizar Cierre y Arqueo", Width = 200, BackColor = Color.OrangeRed, ForeColor = Color.White, Height = 30, FlatStyle = FlatStyle.Flat };
            
            panel.Controls.AddRange(new Control[] { new Label { Text = "Fecha:", AutoSize = true, Margin = new Padding(0,5,0,0) }, dtpFechaCorte, btnGenerarCorte });

            dgvCorteCaja = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.WhiteSmoke };
            lblTotalCorte = new Label { Dock = DockStyle.Bottom, Text = "Esperando corte...", Font = new Font("Segoe UI", 12, FontStyle.Bold), Padding = new Padding(10), Height = 40, BackColor = Color.LightGray, TextAlign = ContentAlignment.MiddleRight };

            tabCorteCaja.Controls.Add(dgvCorteCaja);
            tabCorteCaja.Controls.Add(lblTotalCorte);
            tabCorteCaja.Controls.Add(panel);

            btnGenerarCorte.Click += BtnGenerarCorte_Click;
        }

        // --- EVENTOS ---

        private void BtnGenerarReporteVentas_Click(object sender, EventArgs e)
        {
            try
            {
                var data = _reporteBLL.GenerarReporteVentasDetallado(dtpInicioVentas.Value, dtpFinVentas.Value);
                dgvVentasDetalladas.DataSource = data;
                dgvVentasDetalladas.Tag = data;
                btnExportarVentasCSV.Enabled = data.Count > 0;
                if (data.Count == 0) MessageBox.Show("No hay ventas en este rango.");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnExportarVentasCSV_Click(object sender, EventArgs e)
        {
            var data = dgvVentasDetalladas.Tag as List<VentaDetalladaReporte>;
            if (data == null) return;
            using (var sfd = new SaveFileDialog { Filter = "CSV|*.csv", FileName = $"Ventas_{DateTime.Now:yyyyMMdd}.csv" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    _reporteBLL.ExportarVentasDetalladasCSV(data, sfd.FileName);
                    MessageBox.Show("Exportado correctamente.");
                }
            }
        }

        // --- LÓGICA ROBUSTA DE CORTE ---
        private void BtnGenerarCorte_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Consultar Sistema
                var corteData = _reporteBLL.GenerarCorteCajaDiario(dtpFechaCorte.Value);
                dgvCorteCaja.DataSource = corteData.Ventas;
                
                if (corteData.Ventas.Count == 0)
                {
                    lblTotalCorte.Text = "Sin ventas registradas hoy.";
                    MessageBox.Show("No hay ventas para realizar el corte.", "Aviso");
                    return;
                }

                lblTotalCorte.Text = $"Sistema Espera: {corteData.TotalDia:C2}";

                // 2. Pedir Arqueo Ciego (InputBox)
                string input = Interaction.InputBox(
                    $"El sistema indica ventas por: {corteData.TotalDia:C2}\n\nIngresa la cantidad TOTAL REAL (Efectivo + Vouchers) que tienes en caja:", 
                    "Arqueo de Caja", "0");

                if (decimal.TryParse(input, out decimal montoReal))
                {
                    decimal diferencia = montoReal - corteData.TotalDia;
                    string estado = diferencia == 0 ? "EXACTO" : (diferencia < 0 ? "FALTANTE" : "SOBRANTE");
                    
                    // 3. Confirmación
                    var msg = $"Sistema: {corteData.TotalDia:C2}\nReal: {montoReal:C2}\n\nEstado: {estado} de {diferencia:C2}\n\n¿Deseas GUARDAR este corte y generar PDF?";
                    if (MessageBox.Show(msg, "Confirmar Cierre", MessageBoxButtons.YesNo, 
                        diferencia < 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        // 4. Crear objeto Historial
                        var historial = new HistorialCorte
                        {
                            IdEmpleado = _empleado.IdEmpleado,
                            NombreEmpleado = _empleado.NombreEmp,
                            FechaCorte = dtpFechaCorte.Value,
                            TotalSistema = corteData.TotalDia,
                            TotalReal = montoReal,
                            Diferencia = diferencia,
                            TotalEfectivo = corteData.TotalEfectivo,
                            TotalTarjeta = corteData.TotalTarjeta,
                            Comentarios = estado
                        };

                        // 5. Guardar en BD
                        if (_reporteBLL.RegistrarCorteCaja(historial))
                        {
                            // 6. Generar PDF
                            using (var sfd = new SaveFileDialog { Filter = "PDF|*.pdf", FileName = $"CierreCaja_{DateTime.Now:HHmm}.pdf" })
                            {
                                if (sfd.ShowDialog() == DialogResult.OK)
                                {
                                    _reporteBLL.ExportarCorteCajaPDF(corteData, historial, sfd.FileName);
                                    MessageBox.Show("Cierre guardado y PDF generado exitosamente.", "Proceso Terminado");
                                }
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Monto inválido. Ingrese solo números.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en corte: " + ex.Message);
            }
        }
    }
}