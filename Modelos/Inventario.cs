using System;

namespace KIOSKO_Proyecto.Modelos
{
    public class Inventario
    {
        public int IdInventario { get; set; }
        public int IdProducto { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int TotalProductos { get; set; }
        public string Observaciones { get; set; }
        public string Proveedor { get; set; }
    }
}
