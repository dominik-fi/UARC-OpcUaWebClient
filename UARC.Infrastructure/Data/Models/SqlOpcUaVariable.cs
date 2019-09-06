using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Uarc.Infrastructure.Data.Models
{
    public class SqlOpcUaVariable
    {
        // Konstruktoren.
        public SqlOpcUaVariable()
        { }

        public SqlOpcUaVariable(string varlabel, string nodeid)
        {
            VarLabel = varlabel;
            NodeID = nodeid;
        }

        // Primäschlüssel.
        [Key]
        public int OpcUaVariableId { get; set; }

        // Einfache Eigenschaften.
        [Required]
        public string VarLabel { get; set; }
        [Required]
        public string NodeID { get; set; }

        // Navigationseigenschaften.
        public ICollection<SqlOpcUaWert> OpcUaWerte { get; set; } = new List<SqlOpcUaWert>();

        // Fremdenschlüssel.
        [ForeignKey("OpcUaServer")]
        public int OpcUaServerId { get; set; }
    }
}