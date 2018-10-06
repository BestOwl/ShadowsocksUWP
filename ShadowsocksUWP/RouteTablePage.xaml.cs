using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Vpn;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace ShadowsocksUWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RouteTablePage : Page
    {
        public ObservableCollection<VpnRoute> Ipv4InclusionRoutes = new ObservableCollection<VpnRoute>();
        public ObservableCollection<VpnRoute> Ipv4ExclusionRoutes = new ObservableCollection<VpnRoute>();

        public RouteTablePage()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            int size;
            if (!int.TryParse(in_prefixsize.Text, out size))
            {
                return;
            }
            try
            {
                Ipv4InclusionRoutes.Add(new VpnRoute(new HostName(in_host.Text), (byte)size));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                System.Diagnostics.Debug.WriteLine(ex.Message);

                return;
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            int size;
            if (!int.TryParse(in_prefixsize.Text, out size))
            {
                return;
            }
            try
            {
                Ipv4ExclusionRoutes.Add(new VpnRoute(new HostName(in_host.Text), (byte)size));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                System.Diagnostics.Debug.WriteLine(ex.Message);

                return;
            }
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await dialog.ShowAsync();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (VpnRoute r in inList.SelectedItems)
            {
                Ipv4InclusionRoutes.Remove(r);
            }
            foreach (VpnRoute r in exList.SelectedItems)
            {
                Ipv4ExclusionRoutes.Remove(r);
            }
        }
    }
}
