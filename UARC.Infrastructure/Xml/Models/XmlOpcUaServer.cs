using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml;
using System.Xml.Serialization;

namespace Uarc.Infrastructure.Xml.Models
{
    public class XmlOpcUaServer
    {
        // Konstruktor.
        public XmlOpcUaServer()
        {
            XmlOpcUaSubscriptions = new List<XmlOpcUaSubscription>();
        }

        [Required]
        [XmlAttribute("Serverbezeichnung")]
        public string XmlServerlabel { get; set; }

        [Required]
        [XmlAttribute("URL")]
        public string XmlOpcUrl { get; set; }

        [XmlElement("Benutzer")]
        public string XmlUser { get; set; }

        [XmlElement("Passwort")]
        public string XmlPassword { get; set; }

        // Subscription Liste jedes OPC UA Servers.
        [XmlArrayItem("Subscription")]
        [XmlArray("Subscriptions")]
        public List<XmlOpcUaSubscription> XmlOpcUaSubscriptions { get; set; }
    }
}
