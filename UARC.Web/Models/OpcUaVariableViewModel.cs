using Opc.Ua.Client;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Uarc.Core.Opc;

namespace Uarc.Web.Models
{
    public class OpcUaVariableViewModel
    {
        public string ClientKey { get; set; }
        public List<string> Subscriptions { get; set; }
        public string SubDisplayName { get; set; }
        public List<MonitoredItem> Vars { get; set; }
        [Required]
        public string VarDisplayName { get; set; }
        [Required]
        public string NodeId { get; set; }
        [Required]
        public int SamplingInterval { get; set; }
    }
}
