using System.Xml;
using System.Xml.Serialization;

namespace Uarc.Infrastructure.Xml.Models
{
    public class XmlSql
    {
        // Konstruktor.
        public XmlSql()
        { }

        [XmlElement("SqlConnectionString")]
        public string XmlSqlConnectionString { get; set; }
    }
}
