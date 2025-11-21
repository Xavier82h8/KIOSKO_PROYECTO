using System;

namespace KIOSKO_Proyecto.Modelos
{
    public class Inventario
    {
        // Campos de la tabla INVENTARIO
        public int IdInventario { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } // Para mostrar en grids
        public DateTime FechaRegistro { get; set; }
        public int Cantidad { get; set; }
        public string Observaciones { get; set; }
        public string Proveedor { get; set; }

        // --- CAMPOS VIRTUALES (Para la tabla PAGO) ---
        // No se guardan en INVENTARIO, pero el DAL los usa para insertar en PAGO
        public decimal CostoTotal { get; set; }
        public string MetodoPago { get; set; }
    }
}