using System.IO;
using System.Text;
using Uarc.Infrastructure.Xml.Models;

namespace Uarc.Core.Xml
{
    public static class OpcUaXmlConfigChecker
    {
        /// <summary>
        /// Hier wird das Objekt einer Konfigurationsdatei übergeben, danach formatiert, überprüft und zurückgegeben
        /// </summary>
        /// <param name="config"></param>
        public static void FormatAndValidate(XmlUarcConfig config)
        {
            Format(config);
            Validate(config);
        }

        /// <summary>
        /// Hier werden alle Felder die keine Inforamtion erhalten auf "null" gestzt, um einen einfacherne Umgang mit der Konfiguration zu ermöglichen
        /// </summary>
        private static void Format(XmlUarcConfig config)
        {
            /// <summary>
            /// Hier wird die Konfigurationsdatei bearabeitet
            /// </summary>
            foreach (XmlOpcUaServer c_OpcUaServer in config.XmlOpcUaServers)
            {
                if (c_OpcUaServer.XmlServerlabel == "")
                { c_OpcUaServer.XmlServerlabel = null; }

                if (c_OpcUaServer.XmlOpcUrl == "")
                { c_OpcUaServer.XmlOpcUrl = null; }

                if (c_OpcUaServer.XmlUser == "")
                { c_OpcUaServer.XmlUser = null; }

                if (c_OpcUaServer.XmlPassword == "")
                { c_OpcUaServer.XmlPassword = null; }

                foreach (XmlOpcUaSubscription c_OpcUaSubscription in c_OpcUaServer.XmlOpcUaSubscriptions)
                {
                    if (c_OpcUaSubscription.XmlPublishingInterval == "")
                    { c_OpcUaSubscription.XmlPublishingInterval = null; }

                    foreach (XmlOpcUaVariable c_OpcUaVariable in c_OpcUaSubscription.XmlOpcUaVariablen)
                    {
                        if (c_OpcUaVariable.XmlVarLabel == "")
                        { c_OpcUaVariable.XmlVarLabel = null; }

                        if (c_OpcUaVariable.XmlNodeId == "")
                        { c_OpcUaVariable.XmlNodeId = null; }

                        if (c_OpcUaVariable.XmlS7db == "")
                        { c_OpcUaVariable.XmlS7db = null; }

                        if (c_OpcUaVariable.XmlS7var == "")
                        { c_OpcUaVariable.XmlS7var = null; }

                        if (c_OpcUaVariable.XmlSamplingInterval == "")
                        { c_OpcUaVariable.XmlSamplingInterval = null; }
                    }
                }
            }
        }

        /// <summary>
        /// Hier wird überprüft, ob alle notwendigen Felder vorhanden sind
        /// </summary>
        private static void Validate(XmlUarcConfig config)
        {
            foreach (XmlOpcUaServer c_OpcUaServer in config.XmlOpcUaServers)
            {
                //Überprüfen, ob Serverbezeichnung vorhanden ist
                if (c_OpcUaServer.XmlServerlabel == null)
                {
                    string e = "Ein OPC UA Server enthält keinen \"Serverbezeichnung\"";
                    throw new IOException(e);
                }

                //Überprüfen, ob OpcURL vorhanden ist
                if (c_OpcUaServer.XmlOpcUrl == null)
                {
                    string e = "Der Server: " + c_OpcUaServer.XmlServerlabel + " besitzt keine \"URL\"";
                    throw new IOException(e);
                }

                //Überprüfen der OPCSubscription Konfiguration
                foreach (XmlOpcUaSubscription c_OpcUaSubscription in c_OpcUaServer.XmlOpcUaSubscriptions)
                {
                    if (c_OpcUaSubscription.XmlPublishingInterval == null)
                    {
                        string e = "Eine Subscription vom " + c_OpcUaServer.XmlServerlabel + " besitzt kein \"PublishingInterval\"";
                        throw new IOException(e);
                    }

                    //Überprüfen der OPCVariablen Konfiguration
                    foreach (XmlOpcUaVariable c_OpcUaVariable in c_OpcUaSubscription.XmlOpcUaVariablen)
                    {
                        //Überprüfen, ob ein Name vorhanden ist
                        if (c_OpcUaVariable.XmlVarLabel == null)
                        {
                            string e = "Eine Variable vom " + c_OpcUaServer.XmlServerlabel + " besitzt keinen \"Name\"";
                            throw new IOException(e);
                        }

                        //Überprüfen, ob eine NodeID vorhanden ist. Wenn nur die Daten der S7 vorhanden sind, wird die zugehörige NodeID erstellt
                        if (c_OpcUaVariable.XmlNodeId == null && c_OpcUaVariable.XmlS7db != null && c_OpcUaVariable.XmlS7var != null)
                        {
                            c_OpcUaVariable.XmlNodeId = ConvertS7DataToNodeID(c_OpcUaVariable.XmlS7db, c_OpcUaVariable.XmlS7var);
                        }
                        else if (c_OpcUaVariable.XmlNodeId != null && c_OpcUaVariable.XmlS7db == null && c_OpcUaVariable.XmlS7var == null)
                        { }
                        else
                        {
                            string e = "Die Variable " + c_OpcUaVariable.XmlVarLabel + " vom " + c_OpcUaServer.XmlServerlabel + " ist nicht ausreichend definiert";
                            throw new IOException(e);
                        }

                        //Überprüfen, ob ein Samling Intervall eingestellt wurde, falls nicht wird 1 Sekunde eingestellt
                        if (c_OpcUaVariable.XmlSamplingInterval == "")
                        { c_OpcUaVariable.XmlSamplingInterval = "100"; }
                    }
                }
            }
        }

        /// <summary>
        /// Hier wird aus den Daten eines S7 DatenBausteins und einer dessen Variable, die NodeID des S7 OPC UA Severs erstellt
        /// </summary>
        /// <param name="s7db"></param>
        /// <param name="s7var"></param>
        /// <returns>string NodeId</returns>
        private static string ConvertS7DataToNodeID(string s7db, string s7var)
        {
            string m_nodeId = null;
            //Shema für S7-NodeID: ns3;s="S7_DB"."S7_Var"
            StringBuilder e = new StringBuilder();
            e.Append("ns=3;s=\"");
            e.Append(s7db);
            e.Append("\".\"");
            e.Append(s7var);
            e.Append("\"");
            return m_nodeId = e.ToString();
        }
    }
}