using System;

namespace KIOSKO_Proyecto.Modelos
{
    public class Reporte
    {
        public int IdReporte { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal TotalVentas { get; set; }
        public int GeneradoPorEmpleadoId { get; set; }
        public string NombreEmpleadoGenerador { get; set; } // To display in UI
    }
}
