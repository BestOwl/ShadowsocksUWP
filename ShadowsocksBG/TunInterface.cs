using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Wintun2socks;
using ShadowsocksBG.DNS;

namespace ShadowsocksBG
{
    public delegate void PacketPopedHandler(object sender, [ReadOnlyArray] byte[] e);
    public sealed class TunInterface
    {
        ConcurrentQueue<Action> dispatchQ = new ConcurrentQueue<Action>();
        Task dispatchWorker;
        EventWaitHandle dispatchWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        Wintun w = Wintun.Instance;
        DnsProxyServer dnsServer = new DnsProxyServer();
        Action<string> Write;
        Action<string> WriteLine;

        public event PacketPopedHandler PacketPoped;

        internal void executeLwipTask(Action act)
        {
            dispatchQ.Enqueue(act);
            dispatchWaitHandle.Set();
        }
        public void Init()
        {
            WriteLine = Write = str =>
            {
                // logger(str);
                //return null;
            };
            dispatchWorker = Task.Run(() =>
            {
                while (true)
                {
                    Debug.WriteLine($"{dispatchQ.Count} tasks remain");
                    while (dispatchQ.TryDequeue(out Action act))
                    {
                        //Task.Run(() =>
                        {
                            act();
                        }//).Wait(2000);
                    }
                    dispatchWaitHandle.Reset();
                    dispatchWaitHandle.WaitOne();
                }
            });

            w.PacketPoped += W_PopPacket;
            w.DnsPacketPoped += W_DnsPacketPoped; ;
            TcpSocket.EstablishedTcp += W_EstablishTcp;

            w.Init();
        }

        async private void W_DnsPacketPoped(object sender, byte[] e, uint addr, ushort port)
        {
            try
            {
                var res = await dnsServer.QueryAsync(e);
                executeLwipTask(() => w.PushDnsPayload(addr, port, new List<byte>(res).ToArray()));
            }
            catch (Exception)
            {
                // DNS timeout?
            }
        }

        private void W_EstablishTcp(TcpSocket socket)
        {
            Debug.WriteLine($"{TcpSocket.ConnectionCount()} connections now");
            // ShadowsocksR server with procotol=origin, method=none
            RawShadowsocksAdapter a = new RawShadowsocksAdapter("80.80.80.80", 1234, socket, this);
        }

        private void W_PopPacket(object sender, byte[] e)
        {
            PacketPoped(sender, e);
        }

        public void PushPacket([ReadOnlyArray] byte[] packet)
        {
            executeLwipTask(() => w.PushPacket(packet));
        }
    }
}
