using NetworkTool.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NetworkTool;

namespace NetworkTool.Services
{
    public class NmapService 
    {

        public List<NetworkDevice> Devices { get; set; }
        private NmapParser _nmapParser = new NmapParser();

        // Define a delegate for the log message
        public delegate void LogHandler(string message);
        public event LogHandler OnLogMessage;

        public NmapService()
        {
            Devices = new List<NetworkDevice>();
        }



        // using ICMP to ping devices on the network and then updating the list of devices, so it should add new devices, but still keep check on the old ones. 
        public async Task UpdateNetworkDevicesAsync(string subnet)
        {
            
            var newDevices = await IcmpScanAsync(subnet);

            foreach (var newDevice in newDevices)
            {
                var existingDevice = Devices.FirstOrDefault(d => d.IpAddress == newDevice.IpAddress);
                if (existingDevice == null)
                {
                    Devices.Add(newDevice);
                }
                else
                {
                    existingDevice.LastSeen = DateTime.Now;
                }
            }
        }



        public async Task<List<NetworkDevice>> IcmpScanAsync(string subnet)
        {
            var devices = new List<NetworkDevice>();
            var subnetParts = subnet.Split('.');
            string baseSubnet = $"{subnetParts[0]}.{subnetParts[1]}.{subnetParts[2]}.";

            var pingTasks = new List<Task>();

            for (int i = 1; i < 255; i++)
            {
                var ip = baseSubnet + i.ToString();
                pingTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var ping = new Ping();
                        var reply = await ping.SendPingAsync(ip, 200); // 200 ms timeout
                        if (reply.Status == IPStatus.Success)
                        {
                            lock (devices) // Ensure thread-safety when adding to the list
                            {
                                devices.Add(new NetworkDevice { 
                                    IpAddress = ip,
                                    HostName = "N/S",
                                    MacAddress = "N/S",
                                    LastSeen = DateTime.Now
                                });
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore ping exceptions
                    }
                }));
            }

            await Task.WhenAll(pingTasks);
            return devices;
        }
        // TODO: wanna do so that a Device in Devices list is changed it will update the DataGrid in the MainWindow as well, so i dont have to call UpdateDeviceList() every time i want to update the DataGrid
        public async Task ExtendedNetworkScan()
        {
            // use nmap command "namp <target>" to scan each device in the networkDevices list and update the properties of each device
            OnLogMessage?.Invoke("Starting extended scan...");
            // Loop through each device in the list and scan only the Devices that are not already scanned or havent been scanned in a while
            foreach (var device in Devices)
  
            {
                OnLogMessage?.Invoke($"Scanning device {device.IpAddress}...");
                // Check if the device has been scanned in the last 5 minutes or if MacAdress is equal to "N/S"
                if (device.LastSeen < DateTime.Now.AddMinutes(-5) || device.MacAddress == "N/S")
                {
                    // Run nmap command to scan the device
                    string nmapOutput = await RunNmapAsyncCmd(device.IpAddress);
                    NetworkDevice newDevice = _nmapParser.ParseNmapOutput(nmapOutput);

                    // Update the device properties
                    device.HostName = newDevice.HostName;
                    device.MacAddress = newDevice.MacAddress;
                    device.Manufacturer = newDevice.Manufacturer;
                    device.Ports = newDevice.Ports;
                    device.LastSeen = DateTime.Now;
                }

            }
            OnLogMessage?.Invoke("Extended scan complete.");
        }



        // Run nmap command and return the output
        public async Task<string> RunNmapAsyncCmd(string target, string arguments = "")
        {
            // Create a new process to run nmap
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "nmap",
                    Arguments = $"{arguments} {target}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();

            return output;
        }






    }
}
