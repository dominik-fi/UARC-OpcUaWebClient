using System.Xml;
using System.Xml.Serialization;


namespace Uarc.Infrastructure.Xml.Models
{
    public class XmlOpcUaVariable
    {
        // Konstruktor.
        public XmlOpcUaVariable()
        { }

        [XmlAttribute("Name")]
        public string XmlVarLabel { get; set; }

        [XmlAttribute("S7_Datenbaustein")]
        public string XmlS7db { get; set; }

        [XmlAttribute("S7_VariablenName")]
        public string XmlS7var { get; set; }

        [XmlAttribute("NodeID")]
        public string XmlNodeId { get; set; }

        [XmlAttribute("EinsammelInterval")]
        public string XmlSamplingInterval { get; set; }
    }
}
