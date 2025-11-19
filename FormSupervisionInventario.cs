using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using KIOSKO_Proyecto.BLL;
using KIOSKO_Proyecto.Modelos;
using System.Diagnostics;

namespace KIOSKO_Proyecto
{
    public class FormSupervisionInventario : Form
    {
        private ProductoBLL _productoBLL = new ProductoBLL();
        private DataGridView dgv;
        private Button btnExportCsv;
        private Button btnVerDetalleVentas;
        private Modelos.Empleado _empleadoAutenticado;

        public FormSupervisionInventario(Modelos.Empleado empleado)
        {
            _empleadoAutenticado = empleado;
            this.Text = "Supervisión de Inventario";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(900, 600);

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            btnExportCsv = new Button
            {
                Text = "Exportar CSV",
                Dock = DockStyle.Top,
                Height = 36
            };
            btnExportCsv.Click += BtnExportCsv_Click;

            btnVerDetalleVentas = new Button
            {
                Text = "Detalle Ventas",
                Dock = DockStyle.Top,
                Height = 36
            };
            btnVerDetalleVentas.Click += BtnVerDetalleVentas_Click;

            this.Controls.Add(btnExportCsv);
            this.Controls.Add(btnVerDetalleVentas);

            // Role-based access for Detalle Ventas button (case-insensitive)
            var puesto = (_empleadoAutenticado?.Puesto ?? string.Empty).Trim().ToLowerInvariant();
            if (puesto.Contains("supervis") || puesto.Contains("encarg"))
            {
                btnVerDetalleVentas.Visible = false;
            }
            else
            {
                btnVerDetalleVentas.Visible = true;
            }

            Debug.WriteLine($"FormSupervisionInventario ctor: puesto='{_empleadoAutenticado?.Puesto}', btnVerDetalleVentas.Visible={btnVerDetalleVentas.Visible}");

            Load += (s, e) => CargarDatos();
        }

        private void CargarDatos()
        {
            var productos = _productoBLL.ObtenerTodosLosProductos() ?? new List<Producto>();
            dgv.Columns.Clear();
            dgv.Rows.Clear();
            dgv.Columns.Add("Id", "ID");
            dgv.Columns.Add("Nombre", "Producto");
            dgv.Columns.Add("Categoria", "Categoría");
            dgv.Columns.Add("Precio", "Precio");
            dgv.Columns.Add("Stock", "Stock");
            dgv.Columns.Add("Fecha", "Fecha Caducidad");

            foreach (var p in productos)
            {
                dgv.Rows.Add(p.IdProducto, p.Nombre, p.Categoria, p.Precio.ToString("C2"), p.CantidadDisponible, p.FechaCaducidad?.ToShortDateString() ?? "N/A");
            }
        }

        private void BtnExportCsv_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog { Filter = "CSV|*.csv", FileName = "inventario.csv" })
            {
                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    var lines = new List<string>();
                    var headers = dgv.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText).ToArray();
                    lines.Add(string.Join(",", headers));
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        var cells = row.Cells.Cast<DataGridViewCell>().Select(c => QuoteCsv(c.Value?.ToString() ?? string.Empty)).ToArray();
                        lines.Add(string.Join(",", cells));
                    }
                    System.IO.File.WriteAllLines(sfd.FileName, lines);
                    MessageBox.Show("CSV exportado.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnVerDetalleVentas_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("BtnVerDetalleVentas_Click fired. Opening FormDetalleVentas.");
            using (var f = new FormDetalleVentas())
            {
                f.ShowDialog(this);
            }
        }

        private string QuoteCsv(string v)
        {
            return $"\"{v.Replace("\"", "\"\"")}\"";
        }
    }
}