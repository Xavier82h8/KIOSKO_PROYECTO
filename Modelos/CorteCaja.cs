using System;
using System.Collections.Generic;

namespace KIOSKO_Proyecto.Modelos
{
    public class CorteCaja
    {
        public DateTime Fecha { get; set; }
        public List<Venta> Ventas { get; set; } = new List<Venta>();
        
        public decimal TotalDia { get; set; }
        public decimal TotalEfectivo { get; set; }
        public decimal TotalTarjeta { get; set; }
    }
}
