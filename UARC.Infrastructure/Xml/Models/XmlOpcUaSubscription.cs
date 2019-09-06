using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml;
using System.Xml.Serialization;


namespace Uarc.Infrastructure.Xml.Models
{
    public class XmlOpcUaSubscription
    {
        // Konstruktor.
        public XmlOpcUaSubscription()
        {
            XmlOpcUaVariablen = new List<XmlOpcUaVariable>();
        }

        [Required]
        [XmlAttribute("AbfrageInterval")]
        public string XmlPublishingInterval { get; set; }

        // Variablen Liste, welche jeder Subscription zugeordnet werden.
        [XmlArrayItem("OPCVariable")]
        [XmlArray("OPCVariablen")]
        public List<XmlOpcUaVariable> XmlOpcUaVariablen { get; set; }
    }
}
