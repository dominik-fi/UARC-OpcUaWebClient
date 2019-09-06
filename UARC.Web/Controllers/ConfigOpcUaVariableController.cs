using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uarc.Core.Opc;
using Uarc.Web;
using Uarc.Web.Models;

namespace UARC.Web.Controllers
{
    [Authorize(Roles = "Admins")]
    public class ConfigOpcUaVariableController : Controller
    {
        public IActionResult Index()
        {
            return View(Program.UarcCollector.OpcUaVerbindungen);
        }

        public IActionResult OverviewOpcUaVar(string clientKey, string subDisplayName)
        {
            OpcUaVariableViewModel opcUaVariableViewModel = new OpcUaVariableViewModel
            {
                ClientKey = clientKey,
                SubDisplayName = subDisplayName
            };

            Program.UarcCollector.OpcUaVerbindungen.TryGetValue(clientKey, out UarcOpcUaClient client);
            Subscription subscription = client.Subscriptions.ToList().Find(x => x.DisplayName == subDisplayName);
            opcUaVariableViewModel.Vars = subscription.MonitoredItems.ToList();

            return View(opcUaVariableViewModel);
        }

        public IActionResult MakeOpcUaVar(string clientKey, string subDisplayName)
        {
            OpcUaVariableViewModel opcUaVariableViewModel = new OpcUaVariableViewModel
            {
                ClientKey = clientKey
            };

            if (subDisplayName == null)
            {
                Program.UarcCollector.OpcUaVerbindungen.TryGetValue(clientKey, out UarcOpcUaClient client);

                opcUaVariableViewModel.Subscriptions = new List<string>();

                List<Subscription> subscriptions = new List<Subscription>();
                subscriptions = client.Subscriptions.ToList();
                foreach (Subscription sub in subscriptions)
                {
                    opcUaVariableViewModel.Subscriptions.Add(sub.DisplayName);
                }
            }
            else
            {
                opcUaVariableViewModel.SubDisplayName = subDisplayName;
            }

            return View(opcUaVariableViewModel);
        }

        public IActionResult DeleteOpcUaVar(string clientKey, string subDisplayName, string varLabel)
        {
            try
            {
                Program.UarcCollector.OpcUaVerbindungen.TryGetValue(clientKey, out UarcOpcUaClient client);
                client.RemoveMonitoredItemFromSubscription(subDisplayName, varLabel);
            }
            catch (Exception e)
            {
                throw e;
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOpcUaVar(string clientKey, string subDisplayName, string varDisplayName, string nodeId, int samplingInterval)
        {
            try
            {
                Program.UarcCollector.OpcUaVerbindungen.TryGetValue(clientKey, out UarcOpcUaClient client);

                await client.AddMonitoredItemToSubscriptionInListAsync(subDisplayName, varDisplayName, nodeId, samplingInterval);
            }
            catch (Exception e)
            {
                throw e;
            }

            OpcUaVariableViewModel opcUaVariableViewModel = new OpcUaVariableViewModel
            {
                ClientKey = clientKey,
                VarDisplayName = varDisplayName
            };

            return View(opcUaVariableViewModel);
        }
    }
}