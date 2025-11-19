using System;

namespace KIOSKO_Proyecto.Modelos
{
    public class VentaDetalladaReporte
    {
        public int VentaID { get; set; }
        public DateTime FechaVenta { get; set; }
        public string NombreEmpleado { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public string MetodoPago { get; set; }
    }
}
