using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Uarc.Web.Models
{
    public class OpcUaSubscriptionModel
    {
        public List<string> ClientKeys { get; set; }
        public string ClientKey { get; set; }
        public int PublishingInterval { get; set; }
    }
}
