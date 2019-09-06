using Microsoft.EntityFrameworkCore;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uarc.Core.Data;
using Uarc.Core.Opc;
using Uarc.Core.Xml;
using Uarc.Infrastructure.Xml.Models;

namespace Uarc.Core
{
    public class UarcCoreAPI
    {
        #region Properties
        public Dictionary<string, UarcOpcUaClient> OpcUaVerbindungen
        {
            get { return m_OpcUaVerbindungen; }
        }

        public string SqlConnectionString
        {
            get { return m_SqlConnectionString; }
        }
        #endregion

        #region Construction
        public UarcCoreAPI()
        {
            // Erstellt eine Liste für die einzelnen OPC Verbindungen
            m_OpcUaVerbindungen = new Dictionary<string, UarcOpcUaClient>();
        }
        #endregion

        #region public Methods
        /// <summary>
        /// Erstellt aus der XML Konfiguration Clients
        /// </summary>
        /// <param name="filePath"></param>
        public void CreateUarcClientsAndDbWithXml(string filePath)
        {
            // Erstellen der XmlUarcConfig.
            XmlUarcConfig opcXmlConfig = XmlConfigFileRead(filePath);

            // Erstellen der Clients und der Datenbank.
            CreateUarcClientsAndDbWithConfig(opcXmlConfig);
        }

        /// <summary>
        /// Hier wird eine weitere Verbindung zu einem OPC UA Server konfiguriert
        /// </summary>
        /// <param name="endpointURL"></param>
        /// <param name="serverlabel"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="subscriptions"></param>
        public void CreateUarcClient(string endpointURL, string serverlabel, string username, string password)
        {
            if (m_SqlDbOptions != null)
            {
                try
                {
                    // Erstellen eines Clients.
                    UarcOpcUaClient Client = new UarcOpcUaClient(m_SqlDbOptions, endpointURL, serverlabel, username, password, serverId);

                    // Ablegen des Objects im Dictionary.
                    string name = Convert.ToString(serverId) + ": " + serverlabel;
                    m_OpcUaVerbindungen.Add(name, Client);

                    // Hochzählen der Servernummerierung.
                    serverId = serverId + 1;
                }
                catch (Exception e)
                {
                    throw e;
                }              
            }
            else
            {
                throw new Exception("Es muss zuerst eine Datenbank verbunden werden.");
            }            
        }

        /// <summary>
        /// Hier kann eine beliebige Verbindung initiert werden
        /// </summary>
        /// <param name="clientKey"></param>
        /// <returns></returns>
        public async Task InitUarcClientAsync(string clientKey)
        {
            try
            {
                m_OpcUaVerbindungen.TryGetValue(clientKey, out UarcOpcUaClient client);
                await client.InitUarcOpcUaClientAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Initialisiert alle Clients
        /// </summary>
        /// <param name="filePath"></param>
        public async Task InitAllUarcClientsAsync()
        {
            // Clients werden initiert.
            foreach (KeyValuePair<string, UarcOpcUaClient> client in m_OpcUaVerbindungen)
            {
                try
                {
                    await InitUarcClientAsync(client.Key);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        /// <summary>
        /// Hier werden alle Clients beendet und neu erstellt.
        /// </summary>
        /// <returns></returns>
        public async Task RecreateAllUarcClientsAsync()
        {
            // Die Konfiguration wird gesichert.
            XmlUarcConfig config = await ExportXmlConfigAsync();

            // Alle Sessions werden geschlossen.
            foreach (KeyValuePair<string, UarcOpcUaClient> client in m_OpcUaVerbindungen)
            {
                await client.Value.SaveAndCloseClientAsync();
            }

            // Interne Parameter werden zurückgesetzt.
            m_SqlDbOptions = null;
            m_SqlConnectionString = null;
            m_OpcUaVerbindungen.Clear();
            serverId = 0;


            // Alle Clients und Datenbanken werden neu erstellt.
            CreateUarcClientsAndDbWithConfig(config);

            // Alle Clients werden initiert.
            await InitAllUarcClientsAsync();
        }

        /// <summary>
        /// Mit dieser Methode wird die Konfiguration wieder aus dem Dictionary exportiert
        /// </summary>
        /// <returns>XmlUarcConfig</returns>
        public async Task<XmlUarcConfig> ExportXmlConfigAsync()
        {
            // XmlUarcConfig Objekt wird erzeugt.
            XmlUarcConfig opcXmlConfig = new XmlUarcConfig();

            // XmlUarcConfig Objekt wird mit den Daten aus dem Dicitionary befüllt.
            foreach (UarcOpcUaClient client in m_OpcUaVerbindungen.Values)
            {
                // Das Server Abbild wird erstellt.
                XmlOpcUaServer xmlOpcUaServer = new XmlOpcUaServer
                {
                    XmlUser = client.Username,
                    XmlPassword = client.Password,
                    XmlServerlabel = client.ServerLabel,
                    XmlOpcUrl = client.EndpointUrl
                };

                // Alle Subscriptions werden aus der Session gelesen und im Client Obj gesichert.
                await client.SaveAllSubscriptionsAsync();

                // Für alle Subscription wird ein Abbild erstellt.
                foreach (Subscription sub in client.Subscriptions)
                {
                    XmlOpcUaSubscription xmlOpcUaSubscription = new XmlOpcUaSubscription
                    {
                        XmlPublishingInterval = Convert.ToString(sub.PublishingInterval)
                    };

                    foreach (MonitoredItem item in sub.MonitoredItems)
                    {
                        XmlOpcUaVariable xmlOpcUaVariable = new XmlOpcUaVariable
                        {
                            XmlVarLabel = item.DisplayName,
                            XmlNodeId = Convert.ToString(item.ResolvedNodeId),
                            XmlSamplingInterval = Convert.ToString(item.SamplingInterval)
                        };

                        xmlOpcUaSubscription.XmlOpcUaVariablen.Add(xmlOpcUaVariable);
                    }
                    xmlOpcUaServer.XmlOpcUaSubscriptions.Add(xmlOpcUaSubscription);
                }

                // Alle gesicherten Subscriptions werden entfernt.
                client.Subscriptions.ToList().Clear();

                // Jede Client Konfiguration wird dem KonfigurationsObjekt hinzugefügt
                opcXmlConfig.XmlOpcUaServers.Add(xmlOpcUaServer);
            }

            XmlSql SqlConfig = new XmlSql
            {
                XmlSqlConnectionString = m_SqlConnectionString,
            };

            // Der Connectionstring wird ermittelt.
            opcXmlConfig.XmlSQLConfig = SqlConfig;

            // Das XmlUarcConfig Objekt wird überprüft.
            OpcUaXmlConfigChecker.FormatAndValidate(opcXmlConfig);

            // XmlUarcConfig Objekt wird zurückgegeben.
            return opcXmlConfig;
        }

        /// <summary>
        /// Erstellt eine SqlDatenbank mit dem definiertem Shema von OpcUaDataDbContext und einem Connectionstring.
        /// </summary>
        public void CreateDatabase(string connectionString)
        {
            if (m_SqlConnectionString == null)
            {
                m_SqlConnectionString = connectionString;

                var ContextFactory = new OpcUaDataDbContextFactory
                {
                    ConnectionString = connectionString
                };

                // Erstellen bzw. aktulisieren der SQl Datenbank mithilfe der Context Klasse.
                using (var context = ContextFactory.CreateDbContext(null))
                {
                    context.Database.Migrate();
                }

                //Optionsbuilder wird für später SQL Aktionen in der UarcCoreAPI gespeichert.
                m_SqlDbOptions = ContextFactory.OptionsBuilder.Options;
            }
            else
            {
                Exception e = new Exception("Datenbank wurde nicht erstellt, da noch eine Konfiguration vorhanden ist.");
                throw e;
            }
        }

        /// <summary>
        /// Entfernt die Sql Konfiguration
        /// </summary>
        public void DeleteDatebaseConfig()
        {
            m_SqlConnectionString = null;
            m_SqlDbOptions = null;
        }
        #endregion

        #region private Methods
        /// <summary>
        /// Mit dieser Methode wird die XML Konfigurationsdatei eingelesen und als Objekt ausgegeben.
        /// </summary>
        /// <param name="Pfad">Enthält den Ablageort der Konfigurations-Datei</param>
        private XmlUarcConfig XmlConfigFileRead(string Pfad)
        {
            XmlUarcConfig opcXmlConfig = null;

            // Einlesen der XML-Datei.
            opcXmlConfig = OpcUaXmlConfigIO.OpcUaXmlConfigFileRead(Pfad);

            // Überprüfen der Daten.
            OpcUaXmlConfigChecker.FormatAndValidate(opcXmlConfig);

            return opcXmlConfig;
        }

        public void CreateUarcClientsAndDbWithConfig(XmlUarcConfig opcXmlConfig)
        {
            // Erstellen der Datenbank.
            try
            {
                CreateDatabase(opcXmlConfig.XmlSQLConfig.XmlSqlConnectionString);
            }
            catch (Exception e)
            {
                if (e.Message == null)
                {
                    e = new Exception(String.Format("Die SQL Datenbank konnte nicht erstellt werden"));
                    throw e;
                }
                else
                {
                    throw e;
                }
            }

            // Erstellen der Clients.
            if (opcXmlConfig != null)
            {
                // Das opcXmlConfig Object wird in ein Array umgewandelt, um es mit einer foreach Schleife zu durchlaufen.
                XmlOpcUaServer[] C_OPCUAServers = opcXmlConfig.XmlOpcUaServers.ToArray();

                // In dieser foreach Schleife werden jeweils die Instanzen von MyClient für jeden Server erzeugt und im Dictionary abgelegt.
                foreach (XmlOpcUaServer c_opcuaserver in C_OPCUAServers)
                {
                    UarcOpcUaClient Client = new UarcOpcUaClient(m_SqlDbOptions, c_opcuaserver.XmlOpcUrl, c_opcuaserver.XmlServerlabel, c_opcuaserver.XmlUser, c_opcuaserver.XmlPassword, serverId, c_opcuaserver.XmlOpcUaSubscriptions);

                    //Ablegen des Objects im Dictionary
                    m_OpcUaVerbindungen.Add(Convert.ToString(serverId) + ": " + c_opcuaserver.XmlServerlabel, Client);
                    serverId = serverId + 1;
                }
            }
            else
            {
                throw new Exception("Es sind keine Konfigurationsdaten verfügbar. Bitte lesen Sie eine gültige Konfigurationsdatei ein");
            }
        }
        #endregion

        #region  private Fields
        // Enthält die Konfiguration der SQl Datenbank.
        private DbContextOptions<OpcUaDataDbContext> m_SqlDbOptions = null;
        private string m_SqlConnectionString = null;

        // Dictionary opcUaVerbindungen speichert alle aktuellen Opc Verbindungen in Form von Instanzen von UarcOpcUaClient.
        private Dictionary<string, UarcOpcUaClient> m_OpcUaVerbindungen;

        //ServerID nummer
        private int serverId = 0;
        #endregion
    }
}