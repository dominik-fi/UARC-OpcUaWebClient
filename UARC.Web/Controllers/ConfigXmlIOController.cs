using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Uarc.Core.Xml;
using Uarc.Infrastructure.Xml.Models;
using Uarc.Web.Models;

namespace Uarc.Web.Controllers
{
    [Authorize(Roles = "Admins")]
    public class ConfigXmlIOController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            string fileName = Path.GetFileName(file.FileName);
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "XML Dateien", "Konfiguration", fileName);

            //Wird verwendet im string der View zu übergeben
            XmlConfigViewModel configModel = new XmlConfigViewModel();

            //Temporäres Object um eingebenes Object zuspeichern
            XmlDocument xDoc = new XmlDocument();

            try
            {
                if (file.Length > 0)
                {
                    using (Stream mStream = new MemoryStream())
                    {
                        mStream.Position = 0;
                        await file.CopyToAsync(mStream);
                        mStream.Position = 0;
                        xDoc.Load(mStream);
                    }
                }
                xDoc.Save(filePath);
            }
            catch
            {
                throw new System.IO.IOException("Konfigurationsdatei kann nicht geladen werden oder ist nicht korrekt");
            }

            configModel.Path = filePath;
            configModel.ConfigXmlLabel = fileName;

            return View(configModel);
        }

        [HttpPost("Check")]
        public IActionResult Check(string path)
        {
            XmlConfigViewModel configModel = new XmlConfigViewModel();
            XmlUarcConfig config = OpcUaXmlConfigIO.OpcUaXmlConfigFileRead(path);

            try
            {
                OpcUaXmlConfigChecker.FormatAndValidate(config);
            }
            catch
            {

                throw new Exception("Konfigurationsdatei ist nicht korrekt");
            }

            OpcUaXmlConfigIO.OpcUaXmlConfigFileWrite(path, config); ;

            configModel.Path = path;

            return View(configModel);
        }

        public FileResult Download()
        {
            //XML-Dokumentname
            string exampleFileName = "UARCexampleConfig.xml";

            //Erzeugen der Beispiel Konfiguration
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "XML Dateien", exampleFileName);
            OpcUaXmlConfigIO.OpcUaXmlExampleFileWrite(filePath);

            //Ausgeben der Beispiel Konfiguration
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/x-msdownload", exampleFileName);
        }
    }
}