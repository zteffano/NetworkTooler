using NetworkTool.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetworkTool.Views;

/// <summary>
/// Interaction logic for PortScanView.xaml
/// </summary>
public partial class PortScanView : UserControl
{

    private readonly NmapService _nmapService;

    public PortScanView(NmapService nmapService)
    {
        InitializeComponent();
        _nmapService = nmapService;
    }

    private async void ScanButton_Click(object sender, RoutedEventArgs e)
    {
        //TODO: needs logging integration
        string scanIp = ScanIP.Text;
        int startPort = int.Parse(StartPort.Text);
        int endPort = int.Parse(EndPort.Text);
        if (string.IsNullOrEmpty(scanIp))
        {
            MessageBox.Show("Please enter an IP address to scan.");
            return;
        }

        string nmapOutput = await _nmapService.RunNmapAsyncCmd(scanIp, $"-p {startPort}-{endPort}"); // TODO: make a port range selection later 

        var device = _nmapService.ParseNmapOutput(nmapOutput);


        if (device != null)
        {
            ScannedIp.Text = device.IpAddress;
            MacAddress.Text = device.MacAddress;
            Manufacturer.Text = device.Manufacturer;
            PortDataGrid.ItemsSource = device.Ports;

        }
        else
        {
            MessageBox.Show("Invalid Device");
        }


    }
}
