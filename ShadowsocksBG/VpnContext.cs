using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace ShadowsocksBG
{
    internal class VpnContext
    {
        private DatagramSocket s;
        private TunInterface tun = new TunInterface();
        public void Init(string port)
        {
            s = new DatagramSocket();
            s.MessageReceived += S_MessageReceived;
            s.BindEndpointAsync(new HostName("127.0.0.1"), "9008").AsTask().Wait();
            s.ConnectAsync(new HostName("127.0.0.1"), "9007").AsTask().Wait();
            tun.Init();
            tun.PacketPoped += Tun_PacketPoped;
        }

        private void Tun_PacketPoped(object sender, byte[] e)
        {
            s.OutputStream.WriteAsync(e.AsBuffer()).AsTask();
        }

        private void S_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            var remotePort = args.RemotePort;
            var reader = args.GetDataReader();
            byte[] b = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(b);
            tun.PushPacket(b);
        }
    }
}
