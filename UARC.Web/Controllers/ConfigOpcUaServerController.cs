using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Uarc.Core.Opc;
using Uarc.Web.Models;

namespace Uarc.Web.Controllers
{
    [Authorize(Roles = "Admins")]
    public class ConfigOpcUaServerController : Controller
    {
        public IActionResult Index()
        {
            return View(Program.UarcCollector.OpcUaVerbindungen);
        }

        [HttpGet]
        public ViewResult MakeOpcUaClient()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateOpcUaClient(OpcUaServerModel opcUaServerModel)
        {
            try
            {
                Program.UarcCollector.CreateUarcClient(opcUaServerModel.EndpointUrl, opcUaServerModel.OpcUaServerLabel, opcUaServerModel.UserName, opcUaServerModel.Password);
            }
            catch (Exception e)
            {
                throw e;
            }

            OpcUaServerViewModel opcUaServerViewModel = new OpcUaServerViewModel
            {
                OpcUaServerLabel = opcUaServerModel.OpcUaServerLabel
            };

            return View(opcUaServerViewModel);
        }

        public IActionResult DeleteOpcUaClient(string clientKey)
        {
            Program.UarcCollector.OpcUaVerbindungen.TryGetValue(clientKey, out UarcOpcUaClient client);

            if (client.Session == null)
            {
                try
                {
                    Program.UarcCollector.OpcUaVerbindungen.Remove(clientKey);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            OpcUaServerViewModel opcUaServerViewModel = new OpcUaServerViewModel
            {
                OpcUaServerLabel = clientKey
            };

            return View(opcUaServerViewModel);
        }
    }
}