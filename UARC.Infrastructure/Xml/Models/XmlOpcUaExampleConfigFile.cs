namespace Uarc.Infrastructure.Xml.Models
{
    public class XmlOpcUaExampleConfigFile
    {
        // Konstruktor.
        public XmlOpcUaExampleConfigFile()
        { }

        // Erzeugen der Xml Beispiel Konfiguration.
        public XmlUarcConfig OpcUaXmlExampleFileCreate()
        {
            XmlUarcConfig config = new XmlUarcConfig();

            XmlOpcUaServer StandardOpcUaServer = new XmlOpcUaServer
            {
                XmlUser = "Default",
                XmlPassword = "0815",
                XmlServerlabel = "OPC UA Server - OPC Foundation",
                XmlOpcUrl = "opc.tcp://localhost:53530/OPCUA/SimulationServer"
            };

            XmlOpcUaServer S7OpcUaServer = new XmlOpcUaServer
            {
                XmlUser = "SIEMENS",
                XmlPassword = "0815",
                XmlServerlabel = "Anlage1",
                XmlOpcUrl = "opc.tcp://192.168.0.1:4840"
            };

            XmlOpcUaVariable StandardOpcUaVar = new XmlOpcUaVariable
            {
                XmlVarLabel = "Var1",
                XmlNodeId = "ns=5;s=Counter1",
                XmlS7db = "",
                XmlS7var = "",
                XmlSamplingInterval = "100"
            };

            XmlOpcUaVariable S7OpcUaVar = new XmlOpcUaVariable
            {
                XmlVarLabel = "Var1",
                XmlNodeId = "",
                XmlS7db = "OpcUaData",
                XmlS7var = "Counter 1",
                XmlSamplingInterval = "1000"
            };

            XmlSql SqlConfig = new XmlSql
            {
                XmlSqlConnectionString = @"Server=localhost\SQLEXPRESS;Database=OpcUaDataDb;Trusted_Connection=True;",
            };

            XmlOpcUaSubscription StandardSubscription = new XmlOpcUaSubscription
            {
                XmlPublishingInterval = "1",
            };

            XmlOpcUaSubscription BeispielSubscription = new XmlOpcUaSubscription
            {
                XmlPublishingInterval = "10",
            };

            StandardSubscription.XmlOpcUaVariablen.Add(StandardOpcUaVar);
            BeispielSubscription.XmlOpcUaVariablen.Add(S7OpcUaVar);

            StandardOpcUaServer.XmlOpcUaSubscriptions.Add(StandardSubscription);
            S7OpcUaServer.XmlOpcUaSubscriptions.Add(BeispielSubscription);

            config.XmlOpcUaServers.Add(StandardOpcUaServer);
            config.XmlOpcUaServers.Add(S7OpcUaServer);

            config.XmlSQLConfig = SqlConfig;

            return config;
        }
    }
}