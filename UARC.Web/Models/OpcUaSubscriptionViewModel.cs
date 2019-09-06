using Opc.Ua.Client;
using System.Collections.Generic;

namespace Uarc.Web.Models
{
    public class OpcUaSubscriptionViewModel
    {
        public string ClientKey { get; set; }
        public string SubDisplayName { get; set; }
        public uint SubId { get; set; }
        public int PublishingInterval { get; set; }
        public bool ActivatedSession { get; set; }
        public List<Subscription> Subscriptions { get; set; }
    }
}
