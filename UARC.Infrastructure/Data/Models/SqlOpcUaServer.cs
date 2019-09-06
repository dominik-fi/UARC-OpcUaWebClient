using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Uarc.Infrastructure.Data.Models
{
    public partial class SqlOpcUaServer
    {
        // Konstruktoren.
        public SqlOpcUaServer()
        { }

        public SqlOpcUaServer(string serverlabel, string opcuaurl)
        {
            this.ServerLabel = serverlabel;
            this.OpcUaUrl = opcuaurl;
        }

        // Primäschlüssel.  
        [Key]
        public int OpcUaServerId { get; set; }

        // Einfache Eigenschaften.
        [Required]
        public string ServerLabel { get; set; }
        [Required]
        public string OpcUaUrl { get; set; }

        // Navigationseigenschaften.
        public ICollection<SqlOpcUaVariable> OpcUaVariablen { get; set; } = new List<SqlOpcUaVariable>();
    }
}