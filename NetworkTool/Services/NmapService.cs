using NetworkTool.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetworkTool.Services
{
    public class NmapService
    {
        public async Task<List<NetworkDevice>> ScanNetworkAsync(string subnet)
        {
            return await Task.Run(() =>
            {
                var devices = new List<NetworkDevice>();
                try
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "nmap",
                            Arguments = $"-sn {subnet}",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    var regex = new Regex(@"Nmap scan report for (.*)\nHost is up.*\nMAC Address: ([\w:]+) \((.*)\)");
                    var matches = regex.Matches(output);

                    foreach (Match match in matches)
                    {
                        var ip = match.Groups[1].Value;
                        var mac = match.Groups[2].Value;
                        var hostName = match.Groups[3].Value;

                        devices.Add(new NetworkDevice
                        {
                            IpAddress = ip,
                            HostName = hostName,
                            MacAddress = mac
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Log eller håndter fejl
                }

                return devices;
            });
        }

        public async Task<string> RunNmapScanAsync(string ipAddress)
        {
            return await Task.Run(() =>
            {
                try
                {
                    ipAddress = ipAddress.Trim();
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = @"C:\Program Files (x86)\Nmap\nmap.exe",
                            Arguments = $"-p 1-1024 {ipAddress}",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(error))
                    {
                        output += "\n" + error;
                    }

                    return output;
                }
                catch (Exception ex)
                {
                    return $"Error running nmap: {ex.Message}";
                }
            });
        }
    }
}
