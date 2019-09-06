using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using Uarc.Core.Opc;
using Uarc.Web;
using Uarc.Web.Models;

namespace UARC.Web.Controllers
{
    [Authorize(Roles = "Admins")]
    public class ConfigOpcUaSubscriptionController : Controller
    {
        public IActionResult Index()
        {
            return View(Program.UarcCollector.OpcUaVerbindungen);
        }

        public IActionResult MakeOpcUaSubscription(string clientKey)
        {
            OpcUaSubscriptionModel opcUaSubscriptionModel = new OpcUaSubscriptionModel();

            if (clientKey != null)
            {
                opcUaSubscriptionModel.ClientKey = clientKey;
            }
            else
            {
                opcUaSubscriptionModel.ClientKeys = new List<string>();
                foreach (KeyValuePair<string, UarcOpcUaClient> client in Program.UarcCollector.OpcUaVerbindungen)
                {
                    opcUaSubscriptionModel.ClientKeys.Add(client.Key);
                }            
            }

            return View(opcUaSubscriptionModel);
        }

        [HttpPost]
        public IActionResult CreateOpcUaSubscription(string clientKey, int publishingInterval)
        {
            Program.UarcCollector.OpcUaVerbindungen.TryGetValue(clientKey, out UarcOpcUaClient Client);

            //Subscription erstellen
            try
            {
                Client.AddSubscriptionToList(publishingInterval);
            }
            catch (Exception e)
            {
                if (e.Message == null)
                {
                    e = new Exception("Subscription konnte nicht erstellt werden.");
                    throw e;
                }
                else
                    throw e;
            }

            return RedirectToAction("Index");
        }

        public IActionResult DeleteOpcUaSubscription(string clientKey, string subDisplayName)
        {
            Program.UarcCollector.OpcUaVerbindungen.TryGetValue(clientKey, out UarcOpcUaClient client);

            try
            {
                client.RemoveSubscriptionFromList(subDisplayName);
            }
            catch (Exception e)
            {
                throw e;
            }

            return RedirectToAction("Index");
        }

        public IActionResult EditOpcUaSubscription(string clientKey, string subDisplayName)
        {
            Program.UarcCollector.OpcUaVerbindungen.TryGetValue(clientKey, out UarcOpcUaClient client);

            Subscription subscription = client.Subscriptions.ToList().Find(x => x.DisplayName == subDisplayName);

            OpcUaSubscriptionViewModel opcUaSubscriptionViewModel = new OpcUaSubscriptionViewModel
            {
                ClientKey = clientKey,
                SubDisplayName = subscription.DisplayName,
                PublishingInterval = subscription.PublishingInterval,
                SubId = subscription.Id
            };

            return View(opcUaSubscriptionViewModel);
        }

        [HttpPost]
        public IActionResult EditOpcUaSubscription(uint id, string clientKey, string subDisplayName, int publishingInterval)
        {
            Program.UarcCollector.OpcUaVerbindungen.TryGetValue(clientKey, out UarcOpcUaClient client);

            Subscription subscription = client.Subscriptions.ToList().Find(x => x.Id == id);

            if (subDisplayName != null)
            {
                subscription.DisplayName = subDisplayName;
            }

            if (publishingInterval != 0)
            {
                subscription.PublishingInterval = publishingInterval;
            }           

            return RedirectToAction("Index");
        }

        public IActionResult OverviewOpcUaSubscription(string clientKey)
        {
            OpcUaSubscriptionViewModel opcUaSubscriptionViewModel = new OpcUaSubscriptionViewModel();

            Program.UarcCollector.OpcUaVerbindungen.TryGetValue(clientKey, out UarcOpcUaClient client);
            opcUaSubscriptionViewModel.ClientKey = clientKey;

            if (client.Session == null)
            {
                opcUaSubscriptionViewModel.ActivatedSession = false;              
                opcUaSubscriptionViewModel.Subscriptions = client.Subscriptions.ToList();
            }
            else
            {
                opcUaSubscriptionViewModel.ActivatedSession = true;
                opcUaSubscriptionViewModel.Subscriptions = client.Session.Subscriptions.ToList();
            }

            return View(opcUaSubscriptionViewModel);
        }
    }
}