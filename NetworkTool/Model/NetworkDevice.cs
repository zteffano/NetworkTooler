using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetworkTool.Model
{
    public class NetworkDevice
    {
        public string IpAddress { get; set; }
        public string HostName { get; set; }
        public string MacAddress { get; set; }
        public string Manufacturer { get; set; }
        public List<Port> Ports { get; set; } = new List<Port>();
        public DateTime LastSeen { get; set; }
    }
    
}
