using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Vpn;

namespace ShadowsocksBG
{
    public sealed class VpnPlugInImpl : IVpnPlugIn
    {
        public void Connect(VpnChannel channel)
        {
            //VpnCustomPromptTextInput i = new VpnCustomPromptTextInput();
            //i.Compulsory = true;
            //i.DisplayName = "aaaa";

            //await channel.RequestCustomPromptAsync(new IVpnCustomPromptElement[] {i});

            //channel.RequestCredentials(VpnCredentialType.UsernamePassword, false, false, null);
            channel.LogDiagnosticMessage("asdasdasd");
        }

        public void Disconnect(VpnChannel channel)
        {
            channel.LogDiagnosticMessage("asdasdasd");
        }

        public void GetKeepAlivePayload(VpnChannel channel, out VpnPacketBuffer keepAlivePacket)
        {
            keepAlivePacket = null;
            channel.LogDiagnosticMessage("asdasdasd");
        }

        public void Encapsulate(VpnChannel channel, VpnPacketBufferList packets, VpnPacketBufferList encapulatedPackets)
        {
            channel.LogDiagnosticMessage("asdasdasd");
        }

        public void Decapsulate(VpnChannel channel, VpnPacketBuffer encapBuffer, VpnPacketBufferList decapsulatedPackets,
            VpnPacketBufferList controlPacketsToSend)
        {
            channel.LogDiagnosticMessage("asdasdasd");
        }
    }
}
