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
            _nmapService.OnLogMessage += LogMessage;

        }

        private void UpdateDeviceList()
        {
            NetworkDataGrid.Items.Clear();
            foreach (var device in _nmapService.Devices)
            {
                NetworkDataGrid.Items.Add(device);
            }
        }

        private async void NetworkScanButton_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Scanning network...");
            await _nmapService.UpdateNetworkDevicesAsync("192.168.1.0/24");
            LogMessage("Network scan complete.");
            //Log the devices
            LogMessage("Devices found:");
            foreach (var device in _nmapService.Devices)
            {
                LogMessage($"IP: {device.IpAddress}, Hostname: {device.HostName}, MAC: {device.MacAddress}, Last Seen: {device.LastSeen}");
            }
            UpdateDeviceList();
           
    
        }

        private async void NetworkScanButton_Grab_Click(object sender, RoutedEventArgs e)
        {
            await _nmapService.ExtendedNetworkScan();
            UpdateDeviceList();
        }

        private async void RescanDevice_Click(object sender, RoutedEventArgs e)
        {
        // Not implemented
        }

        private async void PortScan_Click(object sender, RoutedEventArgs e)
        {
            // Not implemented
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
