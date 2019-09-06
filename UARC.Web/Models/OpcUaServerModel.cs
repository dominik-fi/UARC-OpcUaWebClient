using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Uarc.Web.Models
{
    public class OpcUaServerModel
    {
        public string EndpointUrl { get; set; }
        public string OpcUaServerLabel { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
