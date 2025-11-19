using System;

namespace KIOSKO_Proyecto.Modelos
{
    public class Pago
    {
        public int IdPago { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal Monto { get; set; }
        public string TipoPago { get; set; }
        public int IdVenta { get; set; }
    }
}
