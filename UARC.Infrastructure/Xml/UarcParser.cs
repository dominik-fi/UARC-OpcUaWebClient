using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Uarc.Infrastructure.Xml.Models;

namespace Uarc.Infrastructure.Xml
{
    public static class UarcParser
    {
        /// <summary>
        /// Hier wird eine OpcXML_Konfigurations Datei in das Filesystem geschrieben.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="config"></param>
        public static void OpcUaXmlFileWrite(string path, XmlUarcConfig config)
        {
            XmlSerializer serialzer = new XmlSerializer(typeof(XmlUarcConfig));
            TextWriter writer = File.CreateText(path);
            serialzer.Serialize(writer, config);
            writer.Close();
        }

        /// <summary>
        /// Hier wird eine XML-Konfigurations Datei aus dem Filesystem eingelesen.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>UarcConfig</returns>
        public static XmlUarcConfig OpcUaXmlFileRead(string path)
        {
            // Instanze vom XmlSerializer.
            XmlSerializer serialzer = new XmlSerializer(typeof(XmlUarcConfig));

            // Instanz von Filestrem und Reader für das XML Dokument.
            FileStream fs = new FileStream(path, FileMode.Open);
            XmlReader reader = XmlReader.Create(fs);

            // Deklarierung des Objekts welches erstellt werden soll.
            XmlUarcConfig config;

            // Befüllung des Objekts durch Deserialization.
            config = (XmlUarcConfig)serialzer.Deserialize(reader);

            // Beenden von Reader und Filestream.
            reader.Close();
            fs.Close();

            return config;
        }
    }
}