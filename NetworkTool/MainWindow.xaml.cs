using NetworkTool.Model;
using System.Windows;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NetworkTool.Services;
using System.Net;
using System.Windows.Controls;
using NetworkTool.Views;

/*
 * TODO: Find a better way to implement Logging
 * TODO: Reseach and design a small feature roadmap
 * 
 */


namespace NetworkTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly NmapService _nmapService;
        private IPAddress _myip;
        // load the network scan view
        private Views.NetworkScanView _networkScanView = new Views.NetworkScanView();

        public MainWindow()
        {
            InitializeComponent();
            _nmapService = new NmapService();
            _nmapService.OnLogMessage += LogMessage;
            SetLocalIP();

        }

        private void UpdateDeviceList()
        {
            _networkScanView.NetworkDataGrid.Items.Clear();
            foreach (var device in _nmapService.Devices)
            {
                _networkScanView.NetworkDataGrid.Items.Add(device);
            }
        }

        private async void NetworkScanButton_Click(object sender, RoutedEventArgs e)
        {

            LoadNetworkScanView();
            //TODO: Move to NetworkScanView
            LogMessage("Scanning network...");
            await _nmapService.UpdateNetworkDevicesAsync("192.168.1.0/24");
            //LogMessage("Network scan complete.");
            ////Log the devices
            LogMessage("Devices found:");
            foreach (var device in _nmapService.Devices)
            {
                LogMessage($"IP: {device.IpAddress}, Hostname: {device.HostName}, MAC: {device.MacAddress}, Last Seen: {device.LastSeen}");
            }
            UpdateDeviceList();
           
    
        }

        private async void NetworkScanButton_Grab_Click(object sender, RoutedEventArgs e)
        {
            
            LoadNetworkScanView();
            //TODO: Move to NetworkScanView and look at Load times(seems high)
            await _nmapService.ExtendedNetworkScan();
            UpdateDeviceList();
        }

        //TODO: should move to NetworkScanView 
        private async void RescanDevice_Click(object sender, RoutedEventArgs e)
        {
        // Not implemented
        }

        private async void PortScan_Click(object sender, RoutedEventArgs e)
        {
            LoadPortScannerView();
            LogMessage("Ready to Scan ports");
        }


        /* Views */

        private void LoadNetworkScanView()
        {
            MainContentControl.Content = _networkScanView;
        }

        private void LoadPortScannerView()
        {
            MainContentControl.Content = new PortScanView(_nmapService);
        }



        /* helpers */

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText($"[{DateTime.Now}] : {message}\n");
                LogTextBox.ScrollToEnd();
            });
        }

        private void SetLocalIP()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] adresses = Dns.GetHostAddresses(hostName);

            foreach (IPAddress ip in adresses)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    _myip = ip;
                    break;
                }
            }

            if (_myip == null)
            {
                LogMessage("Could not find local IP address.");
            }
            else
            {
                LogMessage($"Local IP address: {_myip}");
            }

            MyIp.Text = _myip.ToString();
        }
    }
}
