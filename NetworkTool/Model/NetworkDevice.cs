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

    public class Port
{
        public int PortNumber { get; set; }
        public string Protocol { get; set; }
        public bool IsOpen { get; set; }
        public string Service { get; set; }
    }

    public class NmapParser
    {
        public NetworkDevice ParseNmapOutput(string nmapOutput)
        {
            var device = new NetworkDevice
            {
                LastSeen = DateTime.Now
            };

            var ipRegex = new Regex(@"Nmap scan report for ([\d.]+)");
            var macRegex = new Regex(@"MAC Address: ([0-9A-Fa-f:]+) \(([^)]+)\)");
            // manufacturerRegex is the regex to extract the manufacturer from the MAC address line
            var manufacturerRegex = new Regex(@"MAC Address: [0-9A-Fa-f:]+ \(([^)]+)\)");
            var portRegex = new Regex(@"(\d+)/(\w+)\s+(\w+)\s+(\S+)");

            var ipMatch = ipRegex.Match(nmapOutput);
            if (ipMatch.Success)
            {
                device.IpAddress = ipMatch.Groups[1].Value;
            }

            var macMatch = macRegex.Match(nmapOutput);
            if (macMatch.Success)
            {
                device.MacAddress = macMatch.Groups[1].Value;
            }
            var manufacturerMatch = manufacturerRegex.Match(nmapOutput);
            if (manufacturerMatch.Success)
            {
                device.Manufacturer = manufacturerMatch.Groups[1].Value;
            }

            var portMatches = portRegex.Matches(nmapOutput);
            foreach (Match match in portMatches)
            {
                var port = new Port
                {
                    PortNumber = int.Parse(match.Groups[1].Value),
                    Protocol = match.Groups[2].Value,
                    IsOpen = match.Groups[3].Value == "open",
                    Service = match.Groups[4].Value
                };
                //device.Ports.Add(port);
            }

            return device;
        }
    }
}
