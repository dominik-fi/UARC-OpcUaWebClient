using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Uarc.Infrastructure.Data.Models;

namespace Uarc.Core.Data
{
    public class AddData
    {
        public AddData(DbContextOptions<OpcUaDataDbContext> sqldboptions)
        {
            sqlDbOptionsBuilder = sqldboptions;
        }

        /// <summary>
        /// Diese Methode legt die Daten eines OpcUaServers in der SQL Datenbank ab.
        /// </summary>
        /// <param name="serverLabel"></param>
        /// <param name="serverOpcUrl"></param>
        public async Task SaveServerAsync(string serverLabel, string serverOpcUrl)
        {
            // Mit diesem using wird über das EF Core Framework auf die Datenbank zugegriffen.
            using (OpcUaDataDbContext ctx = new OpcUaDataDbContext(sqlDbOptionsBuilder))
            {
                // Hier werden Datenbankeinträge mit der gleichen OpcUaUrl abgefragt.
                var opcuaserver = await ctx.OpcUaServer
                    .Where(b => b.OpcUaUrl.Contains(serverOpcUrl))
                    .ToListAsync();

                // Wenn kein Eintrag vorhanden ist:
                if (opcuaserver.Count == 0)
                {
                    // Hier wird zuerst ein Instanz von OpcUaServer erstellt welche der Datenbank übergeben wird.
                    SqlOpcUaServer server = new SqlOpcUaServer(serverLabel, serverOpcUrl);
                    // Die Daten werden dem Kontext hinzugefügt.
                    ctx.OpcUaServer.Add(server);

                    // Es wird versucht die Daten des Kontexts der Datenbank hinzuzufügen, bzw. die Datenbank zu aktualisieren.
                    try
                    {
                        var anz = await ctx.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// Hier wird die SqlId des OPC UA Servers  in der Datenbank abgefragt und zurückgegeben.
        /// </summary>
        /// <param name="serverLabel"></param>
        /// <param name="serverOpcUrl"></param>
        /// <returns>int SQLID des OPC UA Servers</returns>
        public async Task<int> GetSqlServerId(string serverLabel, string serverOpcUrl)
        {
            // Hier wird die Variable für den Primärschlüssel definiert
            int key = 0;

            // Mit diesem using wird über das EF Core Framework auf die Datenbank zugegriffen.
            using (OpcUaDataDbContext ctx = new OpcUaDataDbContext(sqlDbOptionsBuilder))
            {
                // Hier wird abgefragt, ob die Datenbank Einträge mit der gleichen OpcUaURL enthält.
                var opcuaserver = await ctx.OpcUaServer
                    .Where(b => b.OpcUaUrl.Contains(serverOpcUrl))
                    .ToListAsync();

                // Wenn kein Eintrag vorhanden ist:
                if (opcuaserver.Count > 1)
                {
                    Exception ex = new Exception("Es liegt ein Datenbankfehler vor. Mehrere Server Elemente mit der gleichen URL wurden angelegt.");
                    throw ex;
                }

                //Der Primarykey wird ausgelesen
                key = opcuaserver.FirstOrDefault().OpcUaServerId;
            }

            //Der Primärykey wird zurückgegeben
            return key;
        }

        /// <summary>
        /// Hier werden die Variablen in die SQL Datenbank geschrieben.
        /// </summary>
        /// <param name="sqlOpcUaServerId"></param>
        /// <param name="varLabel"></param>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public async Task SaveVariableAsync(int sqlOpcUaServerId, string varLabel, string nodeId)
        {
            // Mit diesem Using wird über das EF Core Framework auf die Datenbank zugegriffen.
            using (var ctx = new OpcUaDataDbContext(sqlDbOptionsBuilder))
            {
                // Der Servereintrag, dem die Variable zugeordnet werden, wird gesucht.
                await ctx.OpcUaServer.ToListAsync();
                var server = await ctx.OpcUaServer.FindAsync(sqlOpcUaServerId);

                // Die bereits vorhandenen Variablen Einträge werden ausfindig gemacht.
                var opcuavars = await ctx.OpcUaVariable
                    .Where(b => b.OpcUaServerId.Equals(sqlOpcUaServerId))
                    .ToListAsync();

                bool varexists = false;

                foreach (var opcvar in opcuavars)
                {
                    if (opcvar.NodeID == nodeId)
                    {
                        varexists = true;
                    }
                }

                if (!varexists)
                {
                    SqlOpcUaVariable variable = new SqlOpcUaVariable(varLabel, nodeId);
                    server.OpcUaVariablen.Add(variable);

                    // Es wird versucht die Daten des Kontexts der Datenbank hinzuzufügen, bzw. die Datenbank zu aktualisieren.
                    try
                    {
                        var anz = await ctx.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// Die aktuellen Werte werden in die SQL Datenbank geschrieben.
        /// </summary>
        /// <param name="sqlOpcUaServerId"></param>
        /// <param name="nodeId"></param>
        /// <param name="displayName"></param>
        /// <param name="value"></param>
        /// <param name="status"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public async void SaveValueAsync(int sqlOpcUaServerId, string nodeId, string displayName, string value, string status, DateTime timestamp)
        {
            SqlOpcUaWert wert = new SqlOpcUaWert
            {
                DisplayName = displayName,
                Wert = value,
                Status = status,
                Zeitstempel = timestamp
            };

            // Mit diesem Using wird über das EF Core Framework auf die Datenbank zugegriffen.
            using (var ctx = new OpcUaDataDbContext(sqlDbOptionsBuilder))
            {
                // Der Servereintrag, dem die Variable zugeordnet werden, wird gesucht.
                await ctx.OpcUaServer.ToListAsync();
                var server = await ctx.OpcUaServer.FindAsync(sqlOpcUaServerId);

                // Die bereits vorhandenen Variablen Einträge werden ausfindig gemacht.
                var opcuavars = await ctx.OpcUaVariable
                    .Where(b => b.OpcUaServerId.Equals(sqlOpcUaServerId))
                    .ToListAsync();

                var variable = opcuavars.Where(b => b.NodeID.Contains(nodeId)).FirstOrDefault();

                // Wert wird zur Variable hinzugefügt.
                variable.OpcUaWerte.Add(wert);

                // Es wird versucht die Daten des Kontexts der Datenbank hinzuzufügen, bzw. die Datenbank zu aktualisieren.
                try
                {
                    var anz = await ctx.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Enthält die Konfiguration des SQL-Servers
        /// </summary>
        #region private Fields
        private readonly DbContextOptions<OpcUaDataDbContext> sqlDbOptionsBuilder;
        #endregion
    }
}
