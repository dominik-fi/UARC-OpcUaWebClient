using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Uarc.Core.Xml;
using Uarc.Infrastructure.Xml.Models;
using Uarc.Web;

namespace UARC.Web.Controllers
{
    [Authorize(Roles = "Admins")]
    public class ConfigBackupController : Controller
    {
        public IActionResult Index()
        {
            return View(Program.UarcCollector.OpcUaVerbindungen);
        }

        public async Task<FileResult> Export()
        {
            try
            {
                // Erstellen der XmlUarcConfig aus dem Dicitionary vom UarcColletor.
                XmlUarcConfig xmlUarcConfig = await Program.UarcCollector.ExportXmlConfigAsync();

                // Erstellen des Strings mit Datum und Uhrzeit.
                string dateAndTime = (Convert.ToString(System.DateTime.Now)).Replace(':', '-');

                // XML-Dokumentname.
                string backupFileName = "UARC_Collector Backup, vom: " + dateAndTime + ".xml";

                // XML-Ablageort.
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "XML Dateien", "Backups", backupFileName);

                // Erzeugen der XML Konfiguration.
                OpcUaXmlConfigIO.OpcUaXmlConfigFileWrite(filePath, xmlUarcConfig);

                // Ausgeben der Beispiel Konfiguration.
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/x-msdownload", backupFileName);
            }
            catch (Exception e)
            {
                if (e.Message == null)
                {
                    e = new Exception("Konfiguration konnte nicht exportiert werden.");
                    throw e;
                }
                else
                    throw e;
            }
        }
    }
}