using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using KIOSKO_Proyecto.BLL;
using KIOSKO_Proyecto.Modelos;
using Microsoft.VisualBasic; // Necesario para Interaction.InputBox

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

        // Controles Pestaña Ventas
        private DateTimePicker dtpInicioVentas, dtpFinVentas;
        private Button btnGenerarReporteVentas, btnExportarVentasCSV;
        private DataGridView dgvVentasDetalladas;

        // Controles Pestaña Corte
        private DateTimePicker dtpFechaCorte;
        private Button btnGenerarCorte;
        private Button btnExportarCortePDF;
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
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Padding = new Padding(10),
                Height = 60,
                WrapContents = false
            };

            dtpInicioVentas = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 120,
                Value = DateTime.Today.AddDays(-7) // Por defecto última semana
            };

            dtpFinVentas = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 120,
                Value = DateTime.Today
            };

            btnGenerarReporteVentas = new Button
            {
                Text = "Buscar",
                BackColor = Color.DodgerBlue,
                ForeColor = Color.White,
                Height = 30,
                Width = 100
            };

            btnExportarVentasCSV = new Button
            {
                Text = "Exportar CSV",
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                Height = 30,
                Width = 120,
                Enabled = false
            };

            panel.Controls.AddRange(new Control[]
            {
                new Label { Text = "Del:", AutoSize = true, Margin = new Padding(0, 5, 0, 0) },
                dtpInicioVentas,
                new Label { Text = "Al:", AutoSize = true, Margin = new Padding(10, 5, 0, 0) },
                dtpFinVentas,
                btnGenerarReporteVentas,
                btnExportarVentasCSV
            });

            // ============================================================
            // CORRECCIÓN: Configurar columnas manualmente para evitar NULL
            // ============================================================
            dgvVentasDetalladas = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoGenerateColumns = false, // ¡IMPORTANTE! Desactivamos auto-generación
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.WhiteSmoke,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // Definir columnas manualmente
            dgvVentasDetalladas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "VentaID",
                HeaderText = "ID Venta",
                DataPropertyName = "VentaID",
                FillWeight = 10
            });

            dgvVentasDetalladas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FechaVenta",
                HeaderText = "Fecha",
                DataPropertyName = "FechaVenta",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "g" },
                FillWeight = 20
            });

            dgvVentasDetalladas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NombreEmpleado",
                HeaderText = "Empleado",
                DataPropertyName = "NombreEmpleado",
                FillWeight = 20
            });

            dgvVentasDetalladas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NombreProducto",
                HeaderText = "Producto",
                DataPropertyName = "NombreProducto",
                FillWeight = 25
            });

            dgvVentasDetalladas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Cantidad",
                HeaderText = "Cant.",
                DataPropertyName = "Cantidad",
                FillWeight = 8
            });

            dgvVentasDetalladas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PrecioUnitario",
                HeaderText = "Precio Unit.",
                DataPropertyName = "PrecioUnitario",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" },
                FillWeight = 12
            });

            dgvVentasDetalladas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Subtotal",
                HeaderText = "Subtotal",
                DataPropertyName = "Subtotal",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" },
                FillWeight = 12
            });

            dgvVentasDetalladas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MetodoPago",
                HeaderText = "Método Pago",
                DataPropertyName = "MetodoPago",
                FillWeight = 15
            });

            tabVentasDetalladas.Controls.Add(dgvVentasDetalladas);
            tabVentasDetalladas.Controls.Add(panel);

            btnGenerarReporteVentas.Click += BtnGenerarReporteVentas_Click;
            btnExportarVentasCSV.Click += BtnExportarVentasCSV_Click;
        }

        private void InicializarTabCorteCaja()
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Padding = new Padding(10),
                Height = 60,
                WrapContents = false
            };

            dtpFechaCorte = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 120,
                Value = DateTime.Today
            };

            btnGenerarCorte = new Button
            {
                Text = "Realizar Cierre y Arqueo",
                Width = 180,
                BackColor = Color.OrangeRed,
                ForeColor = Color.White,
                Height = 30,
                FlatStyle = FlatStyle.Flat
            };

            btnExportarCortePDF = new Button
            {
                Text = "Exportar PDF",
                Width = 120,
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };

            panel.Controls.AddRange(new Control[]
            {
                new Label { Text = "Fecha:", AutoSize = true, Margin = new Padding(0, 5, 0, 0) },
                dtpFechaCorte,
                btnGenerarCorte,
                btnExportarCortePDF
            });

            dgvCorteCaja = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.WhiteSmoke,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            lblTotalCorte = new Label
            {
                Dock = DockStyle.Bottom,
                Text = "Esperando corte...",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Padding = new Padding(10),
                Height = 40,
                BackColor = Color.LightGray,
                TextAlign = ContentAlignment.MiddleRight
            };

            tabCorteCaja.Controls.Add(dgvCorteCaja);
            tabCorteCaja.Controls.Add(lblTotalCorte);
            tabCorteCaja.Controls.Add(panel);

            btnGenerarCorte.Click += BtnGenerarCorte_Click;
            btnExportarCortePDF.Click += BtnExportarCortePDF_Click;
        }

        // --- EVENTOS DE VENTAS ---
        private void BtnGenerarReporteVentas_Click(object sender, EventArgs e)
        {
            try
            {
                var desde = dtpInicioVentas.Value.Date;
                var hasta = dtpFinVentas.Value.Date.AddDays(1).AddTicks(-1); // Hasta las 23:59:59

                var data = _reporteBLL.GenerarReporteVentasDetallado(desde, hasta);

                // ============================================================
                // CORRECCIÓN: Llenar manualmente para evitar NULL
                // ============================================================
                dgvVentasDetalladas.Rows.Clear();

                if (data != null && data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        dgvVentasDetalladas.Rows.Add(
                            item.VentaID,
                            item.FechaVenta,
                            item.NombreEmpleado ?? "N/A",
                            item.NombreProducto ?? "N/A",
                            item.Cantidad,
                            item.PrecioUnitario,
                            item.Subtotal,
                            item.MetodoPago ?? "N/A"
                        );
                    }

                    dgvVentasDetalladas.Tag = data; // Guardamos para exportar
                    btnExportarVentasCSV.Enabled = true;
                    MessageBox.Show($"Se encontraron {data.Count} registros de ventas.", "Reporte Generado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    btnExportarVentasCSV.Enabled = false;
                    MessageBox.Show("No hay ventas en este rango de fechas.", "Sin Resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte: {ex.Message}\n\nDetalles: {ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            using (var sfd = new SaveFileDialog
            {
                Filter = "Archivo CSV (*.csv)|*.csv",
                FileName = $"Ventas_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _reporteBLL.ExportarVentasDetalladasCSV(data, sfd.FileName);
                        MessageBox.Show($"Archivo exportado correctamente en:\n{sfd.FileName}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // --- EVENTOS DE CORTE DE CAJA ---
        private void BtnGenerarCorte_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Obtener datos del sistema
                var corteData = _reporteBLL.GenerarCorteCajaDiario(dtpFechaCorte.Value);

                // ============================================================
                // CORRECCIÓN: No usar DataSource, llenar manualmente
                // ============================================================
                dgvCorteCaja.Rows.Clear();
                dgvCorteCaja.Columns.Clear();

                // Definir columnas del grid de corte
                dgvCorteCaja.Columns.Add("VentaID", "ID Venta");
                dgvCorteCaja.Columns.Add("FechaVenta", "Fecha/Hora");
                dgvCorteCaja.Columns.Add("TotalVenta", "Total");
                dgvCorteCaja.Columns.Add("MontoEfectivo", "Efectivo");
                dgvCorteCaja.Columns.Add("MontoTarjeta", "Tarjeta");
                dgvCorteCaja.Columns.Add("NombreEmpleado", "Cajero");

                dgvCorteCaja.Columns["TotalVenta"].DefaultCellStyle.Format = "C2";
                dgvCorteCaja.Columns["MontoEfectivo"].DefaultCellStyle.Format = "C2";
                dgvCorteCaja.Columns["MontoTarjeta"].DefaultCellStyle.Format = "C2";

                if (corteData.Ventas.Count == 0)
                {
                    lblTotalCorte.Text = "Sin ventas registradas en esta fecha.";
                    MessageBox.Show("No hay ventas para realizar el corte.", "Sin Ventas", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnExportarCortePDF.Enabled = false;
                    dgvCorteCaja.Tag = null;
                    return;
                }

                // Llenar el grid manualmente
                foreach (var v in corteData.Ventas)
                {
                    dgvCorteCaja.Rows.Add(
                        v.VentaID,
                        v.FechaVenta.ToString("g"),
                        v.TotalVenta,
                        v.MontoEfectivo,
                        v.MontoTarjeta,
                        v.NombreEmpleado ?? "N/A"
                    );
                }

                dgvCorteCaja.Tag = corteData;
                lblTotalCorte.Text = $"Sistema Espera: {corteData.TotalDia:C2} | Efectivo: {corteData.TotalEfectivo:C2} | Tarjeta: {corteData.TotalTarjeta:C2}";
                btnExportarCortePDF.Enabled = true;

                // 2. Pedir Arqueo Ciego
                string input = Interaction.InputBox(
                    $"El sistema indica ventas por: {corteData.TotalDia:C2}\n\n" +
                    $"Desglose:\n" +
                    $"• Efectivo: {corteData.TotalEfectivo:C2}\n" +
                    $"• Tarjeta: {corteData.TotalTarjeta:C2}\n\n" +
                    $"Ingresa la cantidad TOTAL REAL (Efectivo + Vouchers) que tienes en caja:",
                    "Arqueo de Caja",
                    "0");

                if (string.IsNullOrEmpty(input)) return;

                if (decimal.TryParse(input, out decimal montoReal))
                {
                    decimal diferencia = montoReal - corteData.TotalDia;
                    string estado = diferencia == 0 ? "✅ EXACTO" : (diferencia < 0 ? "⚠️ FALTANTE" : "⚠️ SOBRANTE");

                    var msg = $"Sistema: {corteData.TotalDia:C2}\n" +
                             $"Real: {montoReal:C2}\n\n" +
                             $"Estado: {estado} de {Math.Abs(diferencia):C2}\n\n" +
                             $"¿Deseas GUARDAR este corte y generar PDF?";

                    if (MessageBox.Show(msg, "Confirmar Cierre", MessageBoxButtons.YesNo,
                        diferencia < 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        // 3. Crear objeto Historial
                        var historial = new HistorialCorte
                        {
                            IdEmpleado = _empleado.IdEmpleado,
                            NombreEmpleado = _empleado.NombreEmp,
                            FechaCorte = DateTime.Now,
                            TotalSistema = corteData.TotalDia,
                            TotalReal = montoReal,
                            Diferencia = diferencia,
                            TotalEfectivo = corteData.TotalEfectivo,
                            TotalTarjeta = corteData.TotalTarjeta,
                            Comentarios = estado
                        };

                        // 4. Guardar en BD
                        if (_reporteBLL.RegistrarCorteCaja(historial))
                        {
                            // 5. Generar PDF
                            using (var sfd = new SaveFileDialog
                            {
                                Filter = "Archivo PDF (*.pdf)|*.pdf",
                                FileName = $"CierreCaja_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
                            })
                            {
                                if (sfd.ShowDialog() == DialogResult.OK)
                                {
                                    _reporteBLL.ExportarCorteCajaPDF(corteData, historial, sfd.FileName);
                                    MessageBox.Show($"✅ Cierre guardado y PDF generado exitosamente en:\n{sfd.FileName}", "Proceso Terminado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Error al guardar el corte en la base de datos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Monto inválido. Debes ingresar un número.", "Error de Formato", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en corte: {ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportarCortePDF_Click(object sender, EventArgs e)
        {
            try
            {
                var corteData = dgvCorteCaja.Tag as CorteCaja;

                if (corteData == null || corteData.Ventas.Count == 0)
                {
                    MessageBox.Show("Primero debes generar los datos del corte.", "Sin Datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Historial dummy para reimpresión
                var historialDummy = new HistorialCorte
                {
                    IdEmpleado = _empleado.IdEmpleado,
                    NombreEmpleado = _empleado.NombreEmp,
                    FechaCorte = corteData.Fecha,
                    TotalSistema = corteData.TotalDia,
                    TotalReal = corteData.TotalDia,
                    Diferencia = 0,
                    TotalEfectivo = corteData.TotalEfectivo,
                    TotalTarjeta = corteData.TotalTarjeta,
                    Comentarios = "Reimpresión de vista previa"
                };

                using (var sfd = new SaveFileDialog
                {
                    Filter = "Archivo PDF (*.pdf)|*.pdf",
                    FileName = $"ReporteVista_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
                })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        _reporteBLL.ExportarCorteCajaPDF(corteData, historialDummy, sfd.FileName);
                        MessageBox.Show($"PDF Exportado en:\n{sfd.FileName}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}