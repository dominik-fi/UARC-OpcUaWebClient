using Uarc.Infrastructure.Xml;
using Uarc.Infrastructure.Xml.Models;

namespace Uarc.Core.Xml
{
    public static class OpcUaXmlConfigIO
    {
        /// <summary>
        /// XML Konfigurations Datei einlesen
        /// </summary>
        /// <param name="path"></param>
        /// <returns>XML-Konfigurations Objekt</returns>
        public static XmlUarcConfig OpcUaXmlConfigFileRead(string path)
        {
            XmlUarcConfig config = UarcParser.OpcUaXmlFileRead(path);
            return config;
        }

        /// <summary>
        /// XML Konfigurations Datei schreiben
        /// </summary>
        /// <param name="path"></param>
        /// <param name="config"></param>
        public static void OpcUaXmlConfigFileWrite(string path, XmlUarcConfig config)
        {
            UarcParser.OpcUaXmlFileWrite(path, config);
        }

        /// <summary>
        /// XML Beispiel Konfigurations Datei schreiben
        /// </summary>
        /// <param name="path"></param>
        public static void OpcUaXmlExampleFileWrite(string path)
        {
            //Initialisierung der Beipsiel Konfiguration als Beispiel-Objekt
            XmlOpcUaExampleConfigFile BeispielConfig = new XmlOpcUaExampleConfigFile();
            XmlUarcConfig Config = BeispielConfig.OpcUaXmlExampleFileCreate();

            //Erstellen der XML-Datei aus dem Beispiel-Objekt
            UarcParser.OpcUaXmlFileWrite(path, Config);
        }
    }
}