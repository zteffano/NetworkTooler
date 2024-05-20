using NetworkTool.Model;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

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

        private async void NetworkScanButton_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Starting network scan...");
            var devices = await ScanNetworkAsync("192.168.1.1", "192.168.1.254");
            NetworkDataGrid.ItemsSource = devices;
            LogMessage("Network scan completed.");
        }

        private async Task<List<NetworkDevice>> ScanNetworkAsync(string startIp, string endIp)
        {
            var devices = new List<NetworkDevice>();
            var startIpParts = startIp.Split('.').Select(int.Parse).ToArray();
            var endIpParts = endIp.Split('.').Select(int.Parse).ToArray();

            var tasks = new List<Task>();

            for (int i = startIpParts[3]; i <= endIpParts[3]; i++)
            {
                var ip = $"{startIpParts[0]}.{startIpParts[1]}.{startIpParts[2]}.{i}";
                tasks.Add(Task.Run(async () =>
                {
                    var ping = new Ping();
                    try
                    {
                        var reply = await ping.SendPingAsync(ip, 500); // Forlænget timeout til 500 ms
                        if (reply.Status == IPStatus.Success)
                        {
                            var hostName = await GetHostNameAsync(ip);
                            var macAddress = GetMacAddress(ip);
                            devices.Add(new NetworkDevice
                            {
                                IpAddress = ip,
                                HostName = hostName,
                                MacAddress = macAddress
                            });
                            LogMessage($"Device found: IP={ip}, HostName={hostName}, MAC={macAddress}");
                        }
                    }
                    catch (PingException)
                    {
                        // Ignorer ping exceptions indtil videre
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return devices;
        }

        private async Task<string> GetHostNameAsync(string ipAddress)
        {
            try
            {
                var hostEntry = await Dns.GetHostEntryAsync(ipAddress);
                return hostEntry.HostName;
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetMacAddress(string ipAddress)
        {
            var p = new Process();
            p.StartInfo.FileName = "arp";
            p.StartInfo.Arguments = "-a " + ipAddress;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            var substrings = output.Split('\n');
            foreach (var substring in substrings)
            {
                if (substring.Trim().StartsWith(ipAddress))
                {
                    var parts = substring.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        return parts[1];
                    }
                }
            }

            return "Unknown";
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText($"{DateTime.Now}: {message}\n");
                LogTextBox.ScrollToEnd();
            });
        }
    }
}