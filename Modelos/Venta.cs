using System;
using System.Collections.Generic;

namespace KIOSKO_Proyecto.Modelos
{
    public class Venta
    {
        public int VentaID { get; set; }
        public DateTime FechaVenta { get; set; }
        public decimal TotalVenta { get; set; }
        public decimal? MontoEfectivo { get; set; }
        public decimal? MontoTarjeta { get; set; }
        public decimal? Cambio { get; set; }
        public int EmpleadoID { get; set; }

        // Propiedad de navegación para los detalles
        public List<DetalleVenta> Detalles { get; set; }

        public Venta()
        {
            Detalles = new List<DetalleVenta>();
        }
    }
}
