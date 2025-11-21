using System;

namespace KIOSKO_Proyecto.Modelos
{
    public class Inventario
    {
        public int IdInventario { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } // Esta propiedad se llena con el JOIN
        public DateTime FechaRegistro { get; set; }

        // IMPORTANTE: Asegúrate de que esto coincida con tu DAL
        public int Cantidad { get; set; }

        public string Observaciones { get; set; }
        public string Proveedor { get; set; }
    }
}