using System;

namespace KIOSKO_Proyecto.Modelos
{
    public class Producto
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public decimal Precio { get; set; }
        public int CantidadDisponible { get; set; }
        public DateTime? FechaCaducidad { get; set; }
    }
}