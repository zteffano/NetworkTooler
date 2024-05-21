using NetworkTool.Model;
using System.Windows;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NetworkTool.Services;




namespace NetworkTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly NmapService _nmapService;

        public MainWindow()
        {
            InitializeComponent();
            _nmapService = new NmapService();
        }

        private async void NetworkScanButton_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Starting network scan...");
            var devices = await _nmapService.ScanNetworkAsync("192.168.1.0/24");
            NetworkDataGrid.ItemsSource = devices;
            LogMessage("Network scan completed.");
        }

        private async void RescanDevice_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkDataGrid.SelectedItem is NetworkDevice selectedDevice)
            {
                LogMessage($"Rescanning device: {selectedDevice.IpAddress}...");
                var devices = await _nmapService.ScanNetworkAsync(selectedDevice.IpAddress);
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
                var result = await _nmapService.RunNmapScanAsync(selectedDevice.IpAddress);
                LogMessage(result);
            }
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
