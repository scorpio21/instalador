using System;

namespace Instalador.Models
{
    public class LogEntry
    {
        public string Hora { get; set; } = "";
        public string Mensaje { get; set; } = "";
        public DateTime FechaCompleta { get; set; } = DateTime.Now;
    }
}
