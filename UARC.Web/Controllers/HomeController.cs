using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Uarc.Core.Opc;
using Uarc.Web.Models;

namespace Uarc.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(Program.UarcCollector.OpcUaVerbindungen);
        }

        //[Authorize(Roles = "Admins")]
        public async Task<IActionResult> InitOpcUaClient (string clientKey)
        {
            try
            {
                await Program.UarcCollector.InitUarcClientAsync(clientKey);
            }
            catch (Exception e)
            {
                if (e.Message == null)
                {
                    e = new Exception("Client " + clientKey + " konnte nicht initialisiert werden.");
                    throw e;
                }
                else
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

        //[Authorize(Roles = "Admins")]
        public async Task<IActionResult> CloseOpcUaClient(string clientKey)
        {
            Program.UarcCollector.OpcUaVerbindungen.TryGetValue(clientKey, out UarcOpcUaClient Client);

            await Client.SaveAndCloseClientAsync();

            OpcUaServerViewModel opcUaServerViewModel = new OpcUaServerViewModel
            {
                OpcUaServerLabel = clientKey
            };

            return View(opcUaServerViewModel);
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature != null)
            {
                ErrorViewModel Error = new ErrorViewModel
                {
                    ErrorMessage = Convert.ToString(exceptionFeature.Error),
                    ErrorPath = exceptionFeature.Path
                };

                return View(Error);
            }
            else
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

        }
    }
}