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

                VpnContext context = null;
                if (channel.PlugInContext == null)
                {
                    LogLine("Initializing new context", channel);
                    channel.PlugInContext = context = new VpnContext();
                }
                else
                {
                    LogLine("Context exists", channel);
                    context = (VpnContext)channel.PlugInContext;
                }
                transport.BindEndpointAsync(new HostName("127.0.0.1"), "9007").AsTask().ContinueWith(t =>
                {
                    LogLine("Binded", channel);
                }).Wait();
#if !DEBUG
                context.Init("9008");
#endif
                /* var rport = context.Init(transport.Information.LocalPort, str =>
                {
                    LogLine(str, channel);
                    return null;
                }); */
                var rport = "9008";
                transport.ConnectAsync(new HostName("127.0.0.1"), rport).AsTask().ContinueWith(t =>
                {
                    LogLine("r Connected", channel);
                });

                VpnRouteAssignment routeScope = new VpnRouteAssignment()
                {
                    ExcludeLocalSubnets = true
                };

                var inclusionRoutes = routeScope.Ipv4InclusionRoutes;
                // myip.ipip.net
                //inclusionRoutes.Add(new VpnRoute(new HostName("36.99.18.134"), 32));
                // qzworld.net
                //inclusionRoutes.Add(new VpnRoute(new HostName("188.166.248.242"), 32));
                // DNS server
                inclusionRoutes.Add(new VpnRoute(new HostName("1.1.1.1"), 32));
                // main CIDR
                inclusionRoutes.Add(new VpnRoute(new HostName("172.17.0.0"), 16));

                var assignment = new VpnDomainNameAssignment();
                var dnsServers = new[]
                {
                    // DNS servers
                    // new HostName("192.168.1.1"),
                    new HostName("1.1.1.1")
                };
                assignment.DomainNameList.Add(new VpnDomainNameInfo(".", VpnDomainNameType.Suffix, dnsServers, new HostName[] { }));

                var now = DateTime.Now;
                LogLine("Starting transport", channel);
                channel.StartWithMainTransport(
                new[] { new HostName("192.168.3.1") },
                null,
                null,
                routeScope,
                assignment,
                1500u,
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
            var context = (VpnContext)channel.PlugInContext;
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
