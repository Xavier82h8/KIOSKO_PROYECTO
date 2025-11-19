using System;
using System.Collections.Generic;

namespace KIOSKO_Proyecto.Modelos
{
    public class CorteCaja
    {
        public DateTime Fecha { get; set; }
        public decimal TotalDia { get; set; }
        public decimal TotalEfectivo { get; set; }
        public decimal TotalTarjeta { get; set; }
        public List<Venta> Ventas { get; set; }

        public CorteCaja()
        {
            Ventas = new List<Venta>();
        }
    }
}
