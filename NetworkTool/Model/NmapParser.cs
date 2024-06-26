﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetworkTool.Model
{

    /* TODO: Should move to Services instead or integrate in NmapService class 
     */
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
                device.Ports.Add(port);
            }

            return device;
        }
    }
}
