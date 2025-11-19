using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using KIOSKO_Proyecto.BLL;
using KIOSKO_Proyecto.Modelos;

namespace KIOSKO_Proyecto
{
    public class FormDetalleVentas : Form
    {
        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private Button btnBuscar;
        private DataGridView dgv;
        private VentaBLL _ventaBLL = new VentaBLL();

        public FormDetalleVentas()
        {
            this.Text = "Detalle de Ventas";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(900, 600);

            var top = new Panel { Dock = DockStyle.Top, Height = 50 };
            dtpDesde = new DateTimePicker { Value = DateTime.Today.AddDays(-7) };
            dtpHasta = new DateTimePicker { Value = DateTime.Today };
            btnBuscar = new Button { Text = "Buscar", AutoSize = true };
            btnBuscar.Click += (s, e) => CargarVentas();

            top.Controls.Add(new Label { Text = "Desde:", Location = new Point(8, 14) });
            dtpDesde.Location = new Point(60, 10);
            top.Controls.Add(dtpDesde);
            top.Controls.Add(new Label { Text = "Hasta:", Location = new Point(260, 14) });
            dtpHasta.Location = new Point(305, 10);
            top.Controls.Add(dtpHasta);
            btnBuscar.Location = new Point(520, 8);
            top.Controls.Add(btnBuscar);

            dgv = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true };

            this.Controls.Add(dgv);
            this.Controls.Add(top);

            Load += (s, e) => CargarVentas();
        }

        private void CargarVentas()
        {
            dgv.Columns.Clear();
            dgv.Rows.Clear();
            dgv.Columns.Add("IdVenta", "ID Venta");
            dgv.Columns.Add("Fecha", "Fecha");
            dgv.Columns.Add("Empleado", "Empleado");
            dgv.Columns.Add("Total", "Total");
            dgv.Columns.Add("Metodo", "M�todo");

            var desde = dtpDesde.Value.Date;
            var hasta = dtpHasta.Value.Date.AddDays(1).AddTicks(-1);

            // VentaBLL debe implementar ObtenerVentasPorRango; si no, devuelve vac�o
            List<Venta> ventas = null;
            try
            {
                ventas = _ventaBLL.ObtenerVentasPorRango(desde, hasta);
            }
            catch
            {
                ventas = new List<Venta>();
            }

            foreach (var v in ventas)
            {
                string metodoPago = "N/A";
                if (v.MontoEfectivo.HasValue && v.MontoEfectivo > 0 && v.MontoTarjeta.HasValue && v.MontoTarjeta > 0)
                {
                    metodoPago = "Mixto";
                }
                else if (v.MontoEfectivo.HasValue && v.MontoEfectivo > 0)
                {
                    metodoPago = "Efectivo";
                }
                else if (v.MontoTarjeta.HasValue && v.MontoTarjeta > 0)
                {
                    metodoPago = "Tarjeta";
                }
                dgv.Rows.Add(v.VentaID, v.FechaVenta.ToShortDateString(), v.EmpleadoID, v.TotalVenta.ToString("C2"), metodoPago);
            }
        }
    }
}