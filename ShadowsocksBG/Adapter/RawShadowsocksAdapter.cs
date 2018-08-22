using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Wintun2socks;
using System.Net.Sockets;
using ShadowsocksBG.DNS;

namespace ShadowsocksBG
{
    internal sealed class RawShadowsocksAdapter : ProxyAdapter
    {
        const int MAX_ENQUEUING_SEND_BUF = 8;

        //StreamSocket r = new StreamSocket();
        //TcpClient r = new TcpClient(AddressFamily.InterNetwork);
        Socket r = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        string server;
        int port;
        private IBuffer buf = WindowsRuntimeBuffer.Create(4096);
        private IBuffer localbuf = WindowsRuntimeBuffer.Create(4096);

        private bool remoteConnected = false;

        public RawShadowsocksAdapter(string srv, int port, TcpSocket socket, TunInterface tun) : base(socket, tun)
        {
            server = srv;
            this.port = port;

            Task.Run(() => Init());
        }

        async public void Init()
        {
            try
            {
                await r.ConnectAsync(server, port);
            }
            catch (Exception)
            {
                Debug.WriteLine("Error connecting to remote");
                return;
            }
            Debug.WriteLine("Connected");

            /*
            var header = new byte[7];
            header[0] = 0x01;
            header[1] = (byte)(_socket.RemoteAddr & 0xFF);
            header[2] = (byte)(_socket.RemoteAddr >> 8 & 0xFF);
            header[3] = (byte)(_socket.RemoteAddr >> 16 & 0xFF);
            header[4] = (byte)(_socket.RemoteAddr >> 24);
            header[5] = (byte)(_socket.RemotePort >> 8);
            header[6] = (byte)(_socket.RemotePort & 0xFF);
            */
            string domain = DnsProxyServer.Lookup((byte)(_socket.RemoteAddr >> 24 & 0xFF));
            var header = new byte[domain.Length + 4];
            header[0] = 0x03;
            header[1] = (byte)domain.Length;
            Encoding.ASCII.GetBytes(domain).CopyTo(header, 2);
            header[header.Length - 2] = (byte)(_socket.RemotePort >> 8);
            header[header.Length - 1] = (byte)(_socket.RemotePort & 0xFF);

            // Let header be sent first
            remoteConnected = true;
            SendToRemote(header);

            byte[] remotebuf = new byte[2048];
            while (r.Connected)
            {
                while (sendBuffers.Count > 1)
                {
                    checkSendBufferHandle.WaitOne();
                    checkSendBufferHandle.Reset();
                }
                try
                {
                    var len = await r.ReceiveAsync(new ArraySegment<byte>(remotebuf), SocketFlags.None);
                    if (len == 0)
                    {
                        Debug.WriteLine("Empty run");
                        break;
                    }
                    Debug.WriteLine($"Received {len} bytes");
                    RemoteReceived(remotebuf.Take(len).ToArray());
                }
                catch (Exception)
                {
                    break;
                }
            }

            DisconnectRemote();
        }

        protected override void RemoteReceived(byte[] e)
        {
            byte[] decryptedBytes = e;
            LogData("Decrypted ", decryptedBytes);

            base.RemoteReceived(decryptedBytes);
        }

        protected override void DisconnectRemote()
        {
            remoteConnected = false;
            try
            {
                if (r.Connected) r.Shutdown(SocketShutdown.Both);
                r.Dispose();
                Debug.WriteLine("remote socket disposed");
            }
            catch (Exception)
            {
                Debug.WriteLine("remote socket already disposed");
            }
        }

        protected override void SendToRemote(byte[] e)
        {
            if (remoteConnected)
            {
                try
                {
                    r.Send(e);
                }
                catch (Exception)
                {
                    Debug.WriteLine("Cannot send to remote");
                    Dispose();
                    return;
                }
                LogData("Sent to remote ", e);
                // Send header first, followed by local buffer
                if (localbuf != null)
                {
                    IBuffer newLocalbuf;
                    lock (localbuf)
                    {
                        if (localbuf != null && localbuf.Length > 0)
                        {
                            newLocalbuf = WindowsRuntimeBuffer.Create((int)localbuf.Length);
                            localbuf.CopyTo(newLocalbuf);
                            newLocalbuf.Length = localbuf.Length;
                            //LogData("Sending local buffer ", newLocalbuf.ToArray());
                            localbuf = null;
                        }
                        else
                        {
                            localbuf = null;
                            return;
                        }
                    }
                    r.Send(newLocalbuf.ToArray());
                    Debug.WriteLine("Buffer sent");
                    //await r.OutputStream.WriteAsync(outData.Take((int)len).ToArray().AsBuffer());
                }
                return;
            }
            else
            {
                lock (localbuf)
                {
                    e.CopyTo(0, localbuf, localbuf.Length, e.Length);
                    localbuf.Length += (uint)e.Length;
                }
                return;
            }
        }
    }
}
