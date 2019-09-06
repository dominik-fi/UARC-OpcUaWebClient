using System;
using System.ComponentModel.DataAnnotations;

namespace Uarc.Infrastructure.Data.Models
{
    public class SqlOpcUaWert
    {
        // Konstruktor.
        public SqlOpcUaWert()
        { }

        // Primäschlüssel. 
        [Key]
        public int WertId { get; set; }

        // Einfache Eigenschaften.
        public DateTime Zeitstempel { get; set; }
        public string DisplayName { get; set; }
        public string Wert { get; set; }
        public string Status { get; set; }
    }
}