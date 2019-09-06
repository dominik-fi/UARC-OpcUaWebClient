using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System;
using Uarc.Web.Models;

namespace Uarc.Web.Controllers
{
    [Authorize]
    public class ConfigController : Controller
    {
        private readonly IFileProvider _fileXmlProvider;

        public ConfigController(IFileProvider fileXmlProvider)
        {
            _fileXmlProvider = fileXmlProvider;
        }

        public IActionResult Index()
        {
            var xmlContent = _fileXmlProvider.GetDirectoryContents("/Konfiguration");
            return View(xmlContent);
        }

        public IActionResult CreateUarcClients(string filePath)
        {
            XmlConfigViewModel configModel = new XmlConfigViewModel();

            if ((Program.UarcCollector.OpcUaVerbindungen.Count == 0) && (Program.UarcCollector.SqlConnectionString == null))
            {
                try
                {
                    Program.UarcCollector.CreateUarcClientsAndDbWithXml(filePath);
                }
                catch (Exception e)
                {
                    if (e.Message == null)
                    {
                        e = new Exception("UARC-Collector konnte nicht alle OPC UA Verbindungen erstellen. + /n + Überprüfen Sie die Konfigurationsdatei");
                        throw e;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
            else
            {
                if ((Program.UarcCollector.OpcUaVerbindungen.Count != 0) && (Program.UarcCollector.SqlConnectionString == null))
                {
                    throw new Exception("Es sind bereits Verbindungen und eine Datenbank konfiguriert. + /n + Entfernen Sie diese, um eine neue Konfiguration zu nutzen.");
                }
                else if (Program.UarcCollector.OpcUaVerbindungen.Count != 0)
                {
                    throw new Exception("Es sind bereits Verbindungen konfiguriert. + /n + Entfernen Sie diese, um eine neue Konfiguration zu nutzen.");
                }
                else if (Program.UarcCollector.SqlConnectionString != null)
                {
                    throw new Exception("Es ist bereits eine Datenbank konfiguriert. + /n + Entfernen Sie diese, um eine neue Konfiguration zu nutzen.");
                }
                
            }

            configModel.Path = filePath;

            return View(configModel);
        }

        public FileResult DownloadXmlConfig(string filePath, string exampleFileName)
        {
            try
            {
                //Ausgeben der Beispiel Konfiguration
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/x-msdownload", exampleFileName);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IActionResult DeleteXmlConfig(string filePath, string exampleFileName)
        {
            XmlConfigViewModel configModel = new XmlConfigViewModel();

            // Delete a file by using File class static method...
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch (System.IO.IOException e)
                {
                    throw e;
                }
            }

            configModel.Path = filePath;
            configModel.ConfigXmlLabel = exampleFileName;

            return View(configModel);
        }
    }
}