using NetworkTool.Model;
using System.Windows;
using System.Diagnostics;
using System.Text.RegularExpressions;




namespace NetworkTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


        }

        /* WPF event logic*/

        private async void NetworkScanButton_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Starting network scan...");
            var devices = await ScanNetworkAsync("192.168.1.0/24"); // Scan entire subnet
            NetworkDataGrid.ItemsSource = devices;
            LogMessage("Network scan completed.");
        }

        private async void RescanDevice_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkDataGrid.SelectedItem is NetworkDevice selectedDevice)
            {
                LogMessage($"Rescanning device: {selectedDevice.IpAddress}...");
                var devices = await ScanNetworkAsync(selectedDevice.IpAddress);
                var device = devices.FirstOrDefault();
                if (device != null)
                {
                    selectedDevice.HostName = device.HostName;
                    selectedDevice.MacAddress = device.MacAddress;
                    NetworkDataGrid.Items.Refresh();
                    LogMessage($"Device rescan completed: IP={selectedDevice.IpAddress}, HostName={device.HostName}, MAC={device.MacAddress}");
                }
                else
                {
                    LogMessage($"Device rescan failed: {selectedDevice.IpAddress} is not responding.");
                }
            }
        }

        private async void PortScan_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkDataGrid.SelectedItem is NetworkDevice selectedDevice)
            {
                LogMessage($"Port scan initiated for device: {selectedDevice.IpAddress}");
                var result = await RunNmapScan(selectedDevice.IpAddress);
                LogMessage(result);
            }
        }

        /* Helper methods */

        private async Task<List<NetworkDevice>> ScanNetworkAsync(string subnet)
        {
            return await Task.Run(() =>
            {
                var devices = new List<NetworkDevice>();
                try
                {
                    var process = new Process();
                    process.StartInfo.FileName = "nmap";
                    process.StartInfo.Arguments = $"-sn {subnet}";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
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

                        LogMessage($"Device found: IP={ip}, HostName={hostName}, MAC={mac}");
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"Error running nmap: {ex.Message}");
                }

                return devices;
            });
        }

        private async Task<string> RunNmapScan(string ipAddress)
        {
            return await Task.Run(() =>
            {
                try
                {
                    ipAddress = ipAddress.Trim();
                    LogMessage($"Running nmap scan for {ipAddress}");
                    var process = new Process();
                    process.StartInfo.FileName = "nmap";
                    process.StartInfo.Arguments = $"-p 1-1024 {ipAddress}";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    LogMessage($"Running nmap {process.StartInfo.Arguments}");
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    LogMessage($"nmap standard output: {output}");
                    return output;
                }
                catch (Exception ex)
                {
                    return $"Error running nmap: {ex.Message}";
                }
            });
        
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText($"[{DateTime.Now}] : {message}\n");
                LogTextBox.ScrollToEnd();
            });
        }
    }
}
