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
        internal const uint VPN_MAX_FRAME = 1512;
        internal const string VPN_ADDR = "10.19.10.1";
        internal const string VPN_NETMASK = "255.255.255.255";
        internal const string TUN_SERVICE_NAME = "60000";
        internal readonly HostName VPN_HOST = new HostName(VPN_ADDR);

        VpnPluginState State = VpnPluginState.Disconnected;

        private void LogLine(string text, VpnChannel channel)
        {
            Debug.WriteLine(text);
            channel.LogDiagnosticMessage(text);
        }

        public void Connect(VpnChannel channel)
        {
            State = VpnPluginState.Connecting;
            LogLine("Connecting", channel);
            try
            {
                DatagramSocket transport = new DatagramSocket();
                channel.AssociateTransport(transport, null);

                VpnContext context = null;
                if (channel.PlugInContext == null)
                {
                    // create and Initialize context
                    LogLine("Initializing new context", channel);
                    channel.PlugInContext = context = new VpnContext();
                    context.InitTun2Socks(TUN_SERVICE_NAME, VPN_ADDR, VPN_NETMASK, (int)VPN_MTU, "192.168.1.107:60000", "aes-256-cfb", "SSTest");
                    context.Init();
                }
                else
                {
                    LogLine("Context exists", channel);
                    context = (VpnContext)channel.PlugInContext;
                }

                transport.ConnectAsync(new HostName("127.0.0.1"), TUN_SERVICE_NAME).AsTask().ContinueWith(t =>
                {
                    LogLine("r Connected", channel);
                });

                var now = DateTime.Now;
                LogLine("Starting transport", channel);

                channel.StartWithTrafficFilter(
                    new[] { VPN_HOST },
                    null,
                    null,
                    context.routeScope,
                    context.domainAssignment,
                    VPN_MTU,
                    VPN_MAX_FRAME,
                    false,
                    transport,
                    null,
                    context.trafficScope
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
                channel.Stop();
                State = VpnPluginState.Disconnected;
            }
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
