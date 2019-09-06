using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Uarc.Infrastructure.Xml.Models
{
    public class XmlUarcConfig
    {
        // Konstruktor.
        public XmlUarcConfig()
        {
            XmlOpcUaServers = new List<XmlOpcUaServer>();
        }

        // Enthält die Liste der abzufragenden OPC UA Server.
        [XmlArrayItem("OPCUAServer")]
        [XmlArray("OPCUAServers")]
        public List<XmlOpcUaServer> XmlOpcUaServers { get; set; }

        // Enthält die SQL Konfiguration.
        [XmlElement("SQLServerKonfiguration")]
        public XmlSql XmlSQLConfig { get; set; }
    }
}
