using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSKO_Proyecto.Modelos
{
    public class HistorialCorte
    {
        // Identificador único del corte (PK en base de datos)
        public int IdCorte { get; set; }

        // Relación con el empleado que hizo el corte
        public int IdEmpleado { get; set; }

        // Esta propiedad no está en la tabla HISTORIAL, pero la llenamos
        // haciendo JOIN con la tabla EMPLEADO en el DAL. Sirve para reportes.
        public string NombreEmpleado { get; set; }

        // Fecha y Hora del cierre
        public DateTime FechaCorte { get; set; }

        // --- Datos Financieros Críticos ---

        // Cuánto dice el software que se vendió
        public decimal TotalSistema { get; set; }

        // Cuánto dinero contó físicamente el cajero (Blind Count)
        public decimal TotalReal { get; set; }

        // La resta: (Real - Sistema). Si es negativo, falta dinero.
        public decimal Diferencia { get; set; }

        // --- Desglose ---
        public decimal TotalEfectivo { get; set; }
        public decimal TotalTarjeta { get; set; }

        // Observaciones automáticas (ej. "Faltante de $50") o manuales
        public string Comentarios { get; set; }
    }
}