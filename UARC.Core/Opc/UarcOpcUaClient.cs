//=============================================================================
// Siemens AG
// (c)Copyright (2018) All Rights Reserved
//----------------------------------------------------------------------------- 
// Getestet mit: Windows 10 Ultimate x64
// Entwickelt mit Visual Studio 2017 Professional
// Funktion: Fasst die wichtigsten Funktionen des OPC UA .NET Standard Stack zusammen und stellt diese für den UARC-Collector zur Verfügung
//=============================================================================

using Microsoft.EntityFrameworkCore;
using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Uarc.Core.Data;
using Uarc.Infrastructure.Xml.Models;

namespace Uarc.Core.Opc
{
    public class UarcOpcUaClient
    {
        #region Construction
        public UarcOpcUaClient(DbContextOptions<OpcUaDataDbContext> sqlDbOptions, string endpointUrl, string serverLabel, string username, string password, int serverId, List<XmlOpcUaSubscription> xmlSubscriptions)
        {
            EndpointUrl = endpointUrl;
            ServerLabel = serverLabel;
            Username = username;
            Password = password;
            ServerId = serverId;


            Init(sqlDbOptions);

            //Subscription Objekte werden ertsellt.
            CreateSubscriptionsFromXmlAsync(xmlSubscriptions).Wait();
        }

        public UarcOpcUaClient(DbContextOptions<OpcUaDataDbContext> sqlDbOptions, string endpointUrl, string serverLabel, string username, string password, int serverId)
        {
            m_Subscriptions = new List<Subscription>();

            EndpointUrl = endpointUrl;
            ServerLabel = serverLabel;
            Username = username;
            Password = password;
            ServerId = serverId;

            Init(sqlDbOptions);
        }

        private void Init(DbContextOptions<OpcUaDataDbContext> sqlDbOptions)
        {
            addData = new AddData(sqlDbOptions);

            //Server in Datenbanlk ablegen
            SaveServerDataInSql().Wait();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Stellt die Session bereit, die mit einem OPC UA Server eingerichtet wird.
        /// </summary>
        public Session Session
        {
            get { return m_Session; }
        }

        /// <summary>
        /// Stellt einen Zwischenspeicher bereit, welche der Session zugeordnet werden.
        /// </summary>
        public IEnumerable<Subscription> Subscriptions
        {
            get { return m_Subscriptions; }
        }

        /// <summary>
        /// Jede Instanz von UarcOpcUaClient bekommt bei der Instanzierung einen Int-Wert zugewiesen,um diese eindeutige identifizieren zu können.
        /// Dieser Wert ist nur im UARC-Collector von Bedeutung.
        /// </summary>
        public int ServerID
        {
            get { return ServerId; }
        }
        #endregion

        #region Create/Delete Session
        /// <summary>
        /// Hier wird eine Sesseion ertsellt.
        /// Zu dieser Session werden alle Subscriptions aus dem Zwischenspeicher hinzugefügt.
        /// </summary>
        /// <returns></returns>
        public async Task InitUarcOpcUaClientAsync()
        {
            // Eine Session für den Client wird erstellt.
            try
            {
                await CreateSessionAsync();
            }
            catch (Exception e)
            {
                if (e.Message == null)
                {
                    e = new Exception(String.Format("Verbindung zum Server '{0}' konnte nicht erfolgreich initialisiert werden.", ServerLabel));
                    throw e;
                }
                else
                {
                    throw e;
                }
            }

            // Alle Subscriptions die in UarcOpcUaClient gespeichert sind, werden zur Session hinzugefügt.
            foreach (Subscription sub in m_Subscriptions)
            {
                try
                {
                    AddSubscriptionToSession(sub);
                }
                catch (Exception ex)
                {
                    throw new Exception("Die Subscription mit dem Interval " + sub.PublishingInterval + "konnte nicht zum Server " + ServerLabel + " hinzugefügt werden", ex);
                }
            }

            // Nach der Verwendung des Zwischenspeichers für Subscriptions wird dieser geleert.
            m_Subscriptions.Clear();
        }

        /// <summary> 
        /// Beendet die Session.
        /// </summary>
        public async Task SaveAndCloseClientAsync()
        {
            //Hier werden alle Subscriptions gesichert
            await SaveAllSubscriptionsAsync();

            //Hier wird die Session geschlossen
            m_Session.Close();
            m_Session.Dispose();
            m_Session = null;
        }
        #endregion

        #region Subscribe
        /// <summary>
        /// Hier wird eine Subscription erstellt und in der Liste(m_Subscription) abgelegt
        /// </summary>
        /// <param name="pubInt"></param>
        /// <param name="monitoredItems"></param>
        public string AddSubscriptionToList(int pubInt)
        {
            // Subscription wird erstellt.
            Subscription sub = CreateSubscription(pubInt);

            //Subscription wird zur internen Liste hinzugefügt.
            m_Subscriptions.Add(sub);

            //Rückgabe des Subscription Name
            return sub.DisplayName;
        }

        /// <summary>
        /// Hinzufügen eines MoitoredItem zu einer Subscription.
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="monitoredItem"></param>
        public async Task AddMonitoredItemToSubscriptionInListAsync(string subscriptionName, string varLabel, string nodeId, int samplingInterval)
        {
            // Subscription herraussuchen.
            Subscription subscription = m_Subscriptions.Find(x => x.DisplayName == subscriptionName);

            // Das Monitored Item wird erstellt.
            MonitoredItem monitoredItem = await CreateMonitoredItemAsync(varLabel, nodeId, samplingInterval);

            // Das MonitoredItem wird zur Subscription hinzugefügt.
            subscription.AddItem(monitoredItem);
        }

        /// <summary> 
        /// Entfernt ein einzelnes Monitored Item von einer Subscription, der Session von MyClient
        /// </summary>
        public void RemoveMonitoredItemFromSubscription(string subscriptionName, string varLabel)
        {
            try
            {
                // Subscription herraussuchen.
                Subscription subscription = m_Subscriptions.Find(x => x.DisplayName == subscriptionName);

                // Das Monitored Item suchen.
                MonitoredItem monitoredItem = subscription.MonitoredItems.ToList().Find(x => x.DisplayName == varLabel);

                // Entfernen des Elements von der Subscription.
                subscription.RemoveItem(monitoredItem);
            }
            catch (Exception e)
            {
                if (e.Message == null)
                {
                    if (m_Session != null)
                        e = new Exception("MonitoredItem konnte nicht gelöscht werden, da noch eine Session aktiv ist.");
                    else
                        e = new Exception("MonitoredItem konnte nicht gelöscht werden.");
                    throw e;
                }
                else
                {
                    throw e;
                }
            }
        }

        /// <summary> 
        /// Entfernung einer Subscription der Session von UarcOpcUaClient.
        /// </summary>
        public void RemoveSubscriptionFromList(string subscriptionName)
        {
            if (m_Session == null)
            {
                // Subscription herraussuchen.
                Subscription subscription = m_Subscriptions.Find(x => x.DisplayName == subscriptionName);

                // Subscription löschen.
                m_Subscriptions.Remove(subscription);
            }
            else
            {
                throw new Exception("Die Subscription konnte nicht gelöscht werden, da eine Session aktiv ist.");
            }
        }

        ///// <summary>
        ///// Hier soll eine Aktive Subscription entfernt werden können.
        ///// </summary>
        ////public void RemoveAktiveSubscription(string subscriptionName)
        ////{
        ////    if (m_Session != null)
        ////    {
        ////        Subscription subscription = new Subscription();

        ////        subscription = m_Session.Subscriptions.Where<Subscription>(x => x.DisplayName == subscriptionName);

        ////        m_Session.RemoveSubscription(subscription);
        ////    }
        ////    else       
        ////    {
        ////        throw new Exception("Die Subscription konnte nicht gelöscht werden.");
        ////    }
        ////}

        /// <summary>
        /// Hier werden bei einer aktiven Session alrle Subscriptions gesichert.
        /// </summary>
        /// <returns></returns>
        public async Task SaveAllSubscriptionsAsync()
        {
            //Hier werden die Subcriptions gesichert, um bei der nächsten Initialisierung wieder erstellt werden zu können.
            try
            {
                if (m_Session != null)
                {
                    //Leeren des Zwischenspeichers für Subscriptions
                    m_Subscriptions.Clear();

                    //Erstellen einer temporären Liste für Subscription
                    List<Subscription> subscriptions = m_Session.Subscriptions.ToList();

                    //Sichern und Konvertiern der Subscriptions
                    foreach (Subscription sub in subscriptions)
                    {
                        string subName = AddSubscriptionToList(Convert.ToInt32(sub.CurrentPublishingInterval));

                        List<MonitoredItem> monitoredItems = sub.MonitoredItems.ToList();
                        foreach (MonitoredItem item in monitoredItems)
                        {
                            await AddMonitoredItemToSubscriptionInListAsync(subName, item.DisplayName, Convert.ToString(item.StartNodeId), item.SamplingInterval);
                        }
                    }
                }
                else
                {
                    throw new Exception("Die Subscriptions vom Server " + ServerLabel + " wurden nicht gesichert, da noch eine Session aktiv ist.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Die Subscriptions vom Server " + ServerLabel + " konnte nicht gesichert werden", ex);
            }
        }
        #endregion

        #region private Methods
        /// <summary>
        /// Erstellt eine Session mit den Informationen des Konstruktors.
        /// Es ist nur eine Session pro Server zulässig.
        /// </summary>
        /// <returns></returns>
        private async Task CreateSessionAsync()
        {
            if (m_Session == null)
            {
                // Die Konfiguration für die Client/Server Beziehung wird erstellt.
                m_Config = new ApplicationConfiguration()
                {
                    ApplicationName = "UARC-Collector",
                    ApplicationType = ApplicationType.Client,

                    // ApplicationUri definiert den Namespace (Gültigkeitsbereich) des Zertifikats.
                    // Dieser Syntax muss für eine hohe Kompatibilität beibehalten werden.
                    ApplicationUri = "urn:" + Utils.GetHostName(),

                    SecurityConfiguration = new SecurityConfiguration
                    {
                        ApplicationCertificate = new CertificateIdentifier
                        {
                            StoreType = CertificateStoreType.X509Store,
                            StorePath = "CurrentUser\\My",
                            SubjectName = "UARC-Collector"
                        },

                        TrustedPeerCertificates = new CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "OPC Foundation/CertificateStores/UA Applications"),
                        },
                        TrustedIssuerCertificates = new CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "OPC Foundation/CertificateStores/UA Certificate Authorities"),
                        },
                        RejectedCertificateStore = new CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "OPC Foundation/CertificateStores/RejectedCertificates"),
                        },
                        NonceLength = 32,

                        AutoAcceptUntrustedCertificates = autoAccept
                    },
                    TransportConfigurations = new TransportConfigurationCollection(),
                    TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                    ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = TimeOut }
                };

                // Die Konfiguration wird überprüft.
                await m_Config.Validate(ApplicationType.Client);

                // Wenn kein Zertifikat vorhanden ist, ist das Bool "haveAppCertificate" false.
                bool haveAppCertificate = m_Config.SecurityConfiguration.ApplicationCertificate.Certificate != null;

                // Wenn kein Zertifikat vorhanden ist, wird eine selbstsigniertes Zertifikat erzeugt.
                if (!haveAppCertificate)
                {
                    #region Varibalen zur Zertifikats erstellung
                    string storeType = m_Config.SecurityConfiguration.ApplicationCertificate.StoreType;
                    string storePath = m_Config.SecurityConfiguration.ApplicationCertificate.StorePath;
                    string password = null;
                    string applicationUri = m_Config.ApplicationUri;
                    string applicationName = m_Config.ApplicationName;
                    string subjectName = m_Config.SecurityConfiguration.ApplicationCertificate.SubjectName;
                    IList<String> domainNames = null; //GetLocalIpAddressAndDns(), von UAClient Helper API
                    ushort keySize = CertificateFactory.defaultKeySize; //must be multiples of 1024 (Beispiel 2048),
                    DateTime startTime = System.DateTime.Now;
                    ushort lifetimeInMonths = CertificateFactory.defaultLifeTime; //in month (Beispiel 24)
                    ushort hashSizeInBits = CertificateFactory.defaultHashSize; //(Beispiel 128)
                    bool isCA = false;
                    X509Certificate2 issuerCAKeyCert = null;
                    byte[] publicKey = null;
                    #endregion

                    X509Certificate2 certificate = CertificateFactory.CreateCertificate
                        (
                        storeType,
                        storePath,
                        password,
                        applicationUri,
                        applicationName,
                        subjectName,
                        domainNames,
                        keySize,
                        startTime,
                        lifetimeInMonths,
                        hashSizeInBits,
                        isCA,
                        issuerCAKeyCert,
                        publicKey
                        );

                    certificate.Verify();

                    m_Config.SecurityConfiguration.ApplicationCertificate.Certificate = certificate;
                }

                // "haveAppCertificate"  wird neu beschrieben.
                haveAppCertificate = (m_Config.SecurityConfiguration.ApplicationCertificate.Certificate != null);

                if (haveAppCertificate)
                {
                    //m_config.ApplicationUri = Utils.GetApplicationUriFromCertificate(m_config.SecurityConfiguration.ApplicationCertificate.Certificate);

                    if (m_Config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                    {
                        m_Config.CertificateValidator.CertificateValidation += CertificateValidator_CertificateValidation;
                    }
                }

                // Der passende Enpoint wird ausgewählt.
                var selectedEndpoint = CoreClientUtils.SelectEndpoint(EndpointUrl, haveAppCertificate, 15000);
                // EndpointConfiguration wiird erstellt.
                var endpointConfiguration = EndpointConfiguration.Create(m_Config);
                // Enpoint für Client wird erstellt.
                var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);

                // UserIdentity wird erstellt.
                UserIdentity userIdentity = null;

                if ((Username != null && Username != "") && (Password != null && Password != ""))
                    userIdentity = new UserIdentity(Username, Password);
                else
                    userIdentity = new UserIdentity(); // --> AnonymousIdentityToken

                // Session wird erstellt.
                try
                {
                    m_Session = await Session.Create(m_Config, endpoint, true, true, "Session_" + ServerLabel, 60000, userIdentity, null);
                }
                catch (Exception e)
                {
                    if (e.Message == null)
                    {
                        e = new Exception(String.Format("Session {0} konnte nicht erfolgreich erstellt werden.", ServerLabel));
                        throw e;
                    }
                    else
                    {
                        throw e;
                    }
                }

                // KeepAliveHandler wird hinzugefügt.
                m_Session.KeepAlive += Client_KeepAlive;

                // Default Subscription null setzen, um Fehler zu verhindern
                m_Session.DefaultSubscription = null;
            }
            else
            {
                throw new Exception("Eine Session ist bereits vorhanden");
            }
        }

        /// <summary>
        /// Erstellt eine Subscription mit beliebigen PublishingInterval.
        /// Dieses Objekt muss noch verarbeitet werden. Entweder es wird zur Session(m_Session) oder zur Liste(m_Subscription) hinzugefügt.
        /// </summary>
        /// <param name="publishinginterval"></param>
        /// <returns>Subscription</returns>
        private Subscription CreateSubscription(int publishingInterval)
        {
            // Überprüfung auf ein Abfrageintervall von 0.
            if (publishingInterval == 0)
            {
                throw new Exception("Bitte geben Sie einen gültigen Wert für das Abrufintervall ein.");
            }

            //Sicherstellen, dass jedes Abfrage Intervall nur einmal konfiguriert wird.
            foreach (Subscription sub in m_Subscriptions)
            {
                if (sub.PublishingInterval == publishingInterval)
                {
                    throw new Exception("Eine Subscription mit diesem Intervall ist bereits verfügbar.");
                }
            }

            // Erstellen der Subscription.
            var subscription = new Subscription
            {
                PublishingEnabled = true,
                PublishingInterval = publishingInterval,
                MinLifetimeInterval = Convert.ToUInt32(TimeOut),
                DisplayName = "Subscription_" + Convert.ToString(publishingInterval)
            };

            // Gibt die erstellte Subscription zurück
            return subscription;
        }

        /// <summary>
        /// Erstellt ein Monitored Item.
        /// Dieses Objekt muss noch verarbeitet werden. es muss zu einer Session hinzugefügt werden.
        /// </summary>
        /// <param name="varLabel"></param>
        /// <param name="nodeId"></param>
        /// <param name="samplingInterval"></param>
        /// <returns>MoitoredItem</returns>
        private async Task<MonitoredItem> CreateMonitoredItemAsync(string varLabel, string nodeId, int samplingInterval)
        {
            MonitoredItem monitoredItem = new MonitoredItem
            {
                DisplayName = varLabel,
                StartNodeId = nodeId, //ns=5;s=Counter1
                AttributeId = Attributes.Value,
                MonitoringMode = MonitoringMode.Reporting,
                SamplingInterval = samplingInterval, // Gibt, das Intervall an wie oft die Variable abgefragt und auf Änderung überprüft wird Einheit: ms
                QueueSize = 500, //Zwischenpuffergröße
                DiscardOldest = true
            };

            //Zu jedem Monitored Item wird eine Aktion bei Datenänderung hinzugefügt
            monitoredItem.Notification += WriteDataOnNotificationAsync;
            //monitoredItem.Notification += WriteConsoleOnNotification;

            //Für jede Variable wird Datenbankeintrag erstellen
            await addData.SaveVariableAsync(sqlOpcUaServerId, varLabel, nodeId);

            //Jedes Monitored Item wird überprüft
            if (monitoredItem.Status.Error != null && StatusCode.IsBad(monitoredItem.Status.Error.StatusCode))
            {
                throw ServiceResultException.Create(monitoredItem.Status.Error.StatusCode.Code, "Erstellung des Monitored Item fehlgeschlagen");
            }

            //Rückgabe
            return monitoredItem;
        }

        /// <summary>
        /// Hier wird aus der XML-Datei ein MonitoredItem ertsellt
        /// </summary>
        /// <param name="opcvariable"></param>
        /// <returns>MonitoredItem</returns>
        private async Task<MonitoredItem> CreateMonitoredItemFromXmlAsync(XmlOpcUaVariable opcvariable)
        {
            MonitoredItem monitoredItem = new MonitoredItem
            {
                DisplayName = opcvariable.XmlVarLabel,
                StartNodeId = opcvariable.XmlNodeId, //ns=5;s=Counter1
                AttributeId = Attributes.Value,
                MonitoringMode = MonitoringMode.Reporting,
                SamplingInterval = Convert.ToInt32(opcvariable.XmlSamplingInterval), // Gibt, das Intervall an wie oft die Variable abgefragt und auf Änderung überprüft wird Einheit: ms
                QueueSize = 500, //Zwischenpuffergröße
                DiscardOldest = true
            };

            //Zu jedem Monitored Item wird eine Aktion bei Datenänderung hinzugefügt
            monitoredItem.Notification += WriteDataOnNotificationAsync;
            //monitoredItem.Notification += WriteConsoleOnNotification;

            //Für jede Variable wird Datenbankeintrag erstellen
            await addData.SaveVariableAsync(sqlOpcUaServerId, opcvariable.XmlVarLabel, opcvariable.XmlNodeId);

            //Jedes Monitored Item wird überprüft
            if (monitoredItem.Status.Error != null && StatusCode.IsBad(monitoredItem.Status.Error.StatusCode))
            {
                throw ServiceResultException.Create(monitoredItem.Status.Error.StatusCode.Code, "Erstellung des Monitored Item fehlgeschlagen");
            }

            //Rückgabe
            return monitoredItem;
        }

        /// <summary>
        /// Hier werden aus der XML-Konfiguration OpcUa Subscriptions erstellt.
        /// </summary>
        /// <param name="xmlSubscriptions"></param>
        /// <returns></returns>
        private async Task CreateSubscriptionsFromXmlAsync(List<XmlOpcUaSubscription> xmlSubscriptions)
        {
            m_Subscriptions = new List<Subscription>();

            foreach (XmlOpcUaSubscription xmlSub in xmlSubscriptions)
            {
                try
                {
                    int pubInt = Convert.ToInt32(xmlSub.XmlPublishingInterval);

                    // Eine Liste aus Monitored Items wird erstellt, die Später vom Client aufgezeichnet werden.
                    var monitoredItems = new List<MonitoredItem>();

                    // Dazu wird die übergeben Liste der Opc Variablen aus der XML Konfiguration übergeben.
                    foreach (XmlOpcUaVariable opcvariable in xmlSub.XmlOpcUaVariablen)
                    {
                        // Für jedes C_OpcUaVariable Objekt wir eine Monitored Item erstellt.
                        MonitoredItem monitoredItem = await CreateMonitoredItemFromXmlAsync(opcvariable);

                        // Das erstellte monitored Item wird zu der Liste hinzugefügt.
                        monitoredItems.Add(monitoredItem);
                    }

                    // Subscription wird erstellt.
                    Subscription sub = CreateSubscription(pubInt);

                    // MonitoredItems werden hinzugefügt.
                    if (monitoredItems != null)
                    {
                        sub.AddItems(monitoredItems);
                    }

                    //Subscription wird zur internen Liste hinzugefügt.
                    m_Subscriptions.Add(sub);
                }
                catch (Exception ex)
                {
                    throw new Exception("Die Subscription mit dem Interval " + xmlSub.XmlPublishingInterval + " vom Server " + ServerLabel + " konnte nicht erstellt werden", ex);
                }
            }
        }

        /// <summary>
        /// Hier wird eine erstellte Subscription zur Session hinzugefügt.
        /// </summary>
        /// <param name="subscription"></param>
        private void AddSubscriptionToSession(Subscription subscription)
        {
            // Name wird vereinheitlicht. Ändert den Name nur, wenn das Publishinginterval im geändert wurde
            subscription.DisplayName = "Subscription_" + Convert.ToString(subscription.PublishingInterval);

            // Hinzufügen der Subscription zur Session.
            m_Session.AddSubscription(subscription);

            // Die Subscription wird auf der Session erstellt.
            subscription.Create();
        }

        /// <summary>
        /// Hier wird die SQL ID des Serverdatensatz aus der Datenbank gelesen
        /// </summary>
        /// <returns></returns>
        private async Task SaveServerDataInSql()
        {
            //Serverdaten werden in die Sql Datenbank geschrieben.
            addData.SaveServerAsync(ServerLabel, EndpointUrl).Wait();

            // ServerSqlId wird aus der Datenbank gelesen.
            sqlOpcUaServerId = await addData.GetSqlServerId(ServerLabel, EndpointUrl);
        }
        #endregion

        #region EventHandling
        /// <summary>
        /// Hier wird die Session überwacht und gegebenenfalls ein Reconnect angestoßen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_KeepAlive(Session sender, KeepAliveEventArgs e)
        {
            if (e.Status != null && ServiceResult.IsNotGood(e.Status))
            {
                if (reconnectHandler == null)
                {
                    Console.WriteLine("--- RECONNECTING ---");
                    reconnectHandler = new SessionReconnectHandler();
                    reconnectHandler.BeginReconnect(sender, reconnectPeriod * 1000, Client_ReconnectComplete);
                }
            }
        }
        /// <summary>
        /// Hier wird das Reconnect abgeschlossen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_ReconnectComplete(object sender, EventArgs e)
        {
            // ignore callbacks from discarded objects.
            if (!Object.ReferenceEquals(sender, reconnectHandler))
            {
                return;
            }

            m_Session = reconnectHandler.Session;
            reconnectHandler.Dispose();
            reconnectHandler = null;

            Console.WriteLine("--- RECONNECTED ---");
        }

        /// <summary>
        /// Bei einer Konsolen Apllication können die Werte bei Änderung mit dieser Notification ausgegeben werden
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        private void WriteConsoleOnNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0}: {1}, {2}, {3}, {4}", item.DisplayName, value.Value, value.SourceTimestamp, value.StatusCode, item.Handle);
            }
        }

        /// <summary>
        /// Hier werden die Werte der Variablen bei Änderung in die Datenbank geschrieben
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        private void WriteDataOnNotificationAsync(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            foreach (var value in item.DequeueValues())
            {
                addData.SaveValueAsync(sqlOpcUaServerId, item.StartNodeId.ToString(), item.DisplayName, value.Value.ToString(), value.StatusCode.ToString(), value.SourceTimestamp);
            }
        }

        /// <summary>
        /// Hier werden die Zertifikate überprüft
        /// Bei autoAccept aktiv wird diese Funktion obsolet
        /// </summary>
        /// <param name="validator"></param>
        /// <param name="e"></param>
        private static void CertificateValidator_CertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            //e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted);
            if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
            {
                //e.Accept = autoAccept;
                if (autoAccept)
                {
                    Console.WriteLine("Accepted Certificate: {0}", e.Certificate.Subject);
                }
                else
                {
                    Console.WriteLine("Rejected Certificate: {0}", e.Certificate.Subject);
                    throw new Exception("Das Zertifikat {0} wurde nicht akzeptiert" + e.Certificate.Subject);
                }
            }
        }
        #endregion

        #region private Fields
        //Dieses Objekt beinhaltet die Funktionen für den Datenbankzugriff
        private AddData addData = null;
        //Dieser int beinhaltet den Primärschlüssel des zugörigen OpcUaServer in den SqlDatenbank
        private int sqlOpcUaServerId;
        //Dieses Objekt beinheltet die Grundkonfiguration
        private ApplicationConfiguration m_Config = null;

        //Dieses Objekt enthält die Session der OpcUa Kommunikation
        private Session m_Session = null;

        //Dieses Objekt enthält die Subscriptions der OpcUa Kommunikation
        private List<Subscription> m_Subscriptions = null;

        //Variablen die mit  bei der Instanzierung definiert werden
        public readonly string EndpointUrl = null;
        public readonly string ServerLabel = null;
        public readonly string Username = null;
        public readonly string Password = null;

        //Dieser Int wird bei der Erstellung definiert und dient zur Indentifikation im Programm
        private readonly int ServerId;

        //autoAccept gibt an, ob Zertifakte vom OpcUaServer automatisch akzeptiert werden
        private static bool autoAccept = true;

        //TimeOut der Session und der Subscriptions
        private int TimeOut = 30000;

        //Stellt das Ereignis für einenVerbindungsabbruch bereit.
        private SessionReconnectHandler reconnectHandler = null;
        private const int reconnectPeriod = 10;
        #endregion
    }
}
