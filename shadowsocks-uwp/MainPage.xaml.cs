using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Vpn;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace shadowsocks_uwp
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            VpnManagementAgent vma = new VpnManagementAgent();
            VpnPlugInProfile profile = new VpnPlugInProfile();
            
        }

        class VPN : IVpnPlugIn
        {
            public void Connect(VpnChannel channel)
            {
                throw new NotImplementedException();
            }

            public void Disconnect(VpnChannel channel)
            {
                throw new NotImplementedException();
            }

            public void GetKeepAlivePayload(VpnChannel channel, out VpnPacketBuffer keepAlivePacket)
            {
                throw new NotImplementedException();
            }

            public void Encapsulate(VpnChannel channel, VpnPacketBufferList packets, VpnPacketBufferList encapulatedPackets)
            {
                throw new NotImplementedException();
            }

            public void Decapsulate(VpnChannel channel, VpnPacketBuffer encapBuffer, VpnPacketBufferList decapsulatedPackets,
                VpnPacketBufferList controlPacketsToSend)
            {
                throw new NotImplementedException();
            }
        }
    }
}
