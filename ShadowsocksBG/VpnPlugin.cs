using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Networking.Vpn;

namespace ShadowsocksBG
{
    public sealed class VpnPlugin : IVpnPlugIn
    {
        internal const uint VPN_MTU = 1500;
        internal const string VPN_ADDR = "172.19.0.1";
        internal const string VPN_NETMASK = "255.255.255.255";
        internal const string TUN_SERVICE_NAME = "60000";
        internal readonly HostName VPN_HOST = new HostName(VPN_ADDR);

        BackgroundTaskDeferral def = null;
        VpnPluginState State = VpnPluginState.Disconnected;

        private void LogLine(string text, VpnChannel channel)
        {
            //Debug.WriteLine(text);
            channel.LogDiagnosticMessage(text);
        }
        public void Connect(VpnChannel channel)
        {
            State = VpnPluginState.Connecting;
            LogLine("Connecting", channel);
            try
            {
                var transport = new DatagramSocket();
                channel.AssociateTransport(transport, null);

                VpnContextNew context = null;
                if (channel.PlugInContext == null)
                {
                    LogLine("Initializing new context", channel);
                    channel.PlugInContext = context = new VpnContextNew();
                    context.InitTun2Socks(TUN_SERVICE_NAME, VPN_ADDR, VPN_NETMASK, (int) VPN_MTU, "192.168.1.106:60000", "");
                }
                else
                {
                    LogLine("Context exists", channel);
                    context = (VpnContextNew)channel.PlugInContext;
                }

                transport.ConnectAsync(new HostName("127.0.0.1"), TUN_SERVICE_NAME).AsTask().ContinueWith(t =>
                {
                    LogLine("r Connected", channel);
                });

                VpnRouteAssignment routeScope = new VpnRouteAssignment()
                {
                    ExcludeLocalSubnets = true
                };

                var inclusionRoutes = routeScope.Ipv4InclusionRoutes;
                // qzworld.net
                inclusionRoutes.Add(new VpnRoute(new HostName("188.166.248.242"), 32));
                // DNS server
                //inclusionRoutes.Add(new VpnRoute(new HostName("1.1.1.1"), 32));
                // main CIDR
                //inclusionRoutes.Add(new VpnRoute(new HostName("172.17.0.0"), 16));

                //var assignment = new VpnDomainNameAssignment();
                //var dnsServers = new[]
                //{
                //    // DNS servers
                //    new HostName("1.1.1.1")
                //};
                //assignment.DomainNameList.Add(new VpnDomainNameInfo(".", VpnDomainNameType.Suffix, dnsServers, new HostName[] { }));

                var now = DateTime.Now;
                LogLine("Starting transport", channel);
                channel.StartWithMainTransport(
                new[] { VPN_HOST },
                null,
                null,
                routeScope,
                null,
                VPN_MTU,
                1512u,
                false,
                transport
                );
                var delta = DateTime.Now - now;
                LogLine($"Finished starting transport in {delta.TotalMilliseconds} ms.", channel);
                LogLine("Connected", channel);
                State = VpnPluginState.Connected;
            }
            catch (Exception ex)
            {
                LogLine("Error connecting", channel);
                LogLine(ex.Message, channel);
                LogLine(ex.StackTrace, channel);
                State = VpnPluginState.Disconnected;
            }
            def?.Complete();
        }

        public void Disconnect(VpnChannel channel)
        {
            State = VpnPluginState.Disconnecting;
            if (channel.PlugInContext == null)
            {
                LogLine("Disconnecting with null context", channel);
                State = VpnPluginState.Disconnected;
                return;
            }
            else
            {
                LogLine("Disconnecting with non-null context", channel);
            }
            var context = (VpnContextNew)channel.PlugInContext;
            channel.Stop();
            LogLine("channel stopped", channel);
            channel.PlugInContext = null;
            LogLine("Disconnected", channel);
            State = VpnPluginState.Disconnected;
            def?.Complete();
        }

        public void GetKeepAlivePayload(VpnChannel channel, out VpnPacketBuffer keepAlivePacket)
        {
            // Not needed
            keepAlivePacket = new VpnPacketBuffer(null, 0, 0);
        }

        public void Encapsulate(VpnChannel channel, VpnPacketBufferList packets, VpnPacketBufferList encapulatedPackets)
        {
            // LogLine("Encapsulating", channel);
            while (packets.Size > 0)
            {
                var packet = packets.RemoveAtBegin();
                encapulatedPackets.Append(packet);
                //LogLine("Encapsulated one packet", channel);
            }
        }

        public void Decapsulate(VpnChannel channel, VpnPacketBuffer encapBuffer, VpnPacketBufferList decapsulatedPackets, VpnPacketBufferList controlPacketsToSend)
        {
            var buf = channel.GetVpnReceivePacketBuffer();
            // LogLine("Decapsulating one packet", channel);
            if (encapBuffer.Buffer.Length > buf.Buffer.Capacity)
            {
                LogLine("Dropped one packet", channel);
                //Drop larger packets.
                return;
            }

            encapBuffer.Buffer.CopyTo(buf.Buffer);
            buf.Buffer.Length = encapBuffer.Buffer.Length;
            decapsulatedPackets.Append(buf);
            // LogLine("Decapsulated one packet", channel);

        }
    }

}
