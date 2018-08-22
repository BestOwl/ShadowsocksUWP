using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using Wintun2socks;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Text;

namespace ShadowsocksBG
{
    internal abstract class TunSocketAdapter : IAdapterSocket, IDisposable
    {
        const int MAX_BUFF_SIZE = 2048;
        protected TcpSocket _socket;
        protected TunInterface _tun;
        protected ConcurrentQueue<IBuffer> sendBuffers = new ConcurrentQueue<IBuffer>();
        private Task sendBufferTask;
        protected EventWaitHandle checkSendBufferHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        public event ReadDataHandler ReadData;
        public event SocketErrorHandler OnError;

        public static void LogData(string prefix, byte[] data)
        {
            /*var sb = new StringBuilder(data.Length * 6 + prefix.Length);
            sb.Append(prefix);
            sb.Append(Encoding.ASCII.GetString(data));
            sb.Append(" ");
            foreach (var by in data)
            {
                sb.AppendFormat("\\x{0:x2} ", by);
            }

            Debug.WriteLine(sb.ToString());*/
        }

        internal TunSocketAdapter(TcpSocket socket, TunInterface tun)
        {
            _socket = socket;
            _tun = tun;

            socket.DataReceived += Socket_DataReceived;
            socket.DataSent += Socket_DataSent;
            socket.SocketError += Socket_SocketError;

            sendBufferTask = Task.Run(() =>
            {
                while (true)
                {
                    _tun.executeLwipTask(() =>
                    {
                        while (sendBuffers.TryPeek(out IBuffer buf))
                        {
                            if (_socket.Send(buf.ToArray()) == 0)
                            {
                                sendBuffers.TryDequeue(out buf);
                            }
                            else
                            {
                                break;
                            }
                        }
                    });
                    checkSendBufferHandle.Reset();
                    checkSendBufferHandle.WaitOne();
                }
            });
        }

        private void checkSendBuffers()
        {
            checkSendBufferHandle.Set();
        }

        protected virtual void Socket_SocketError(TcpSocket sender, int err)
        {
            Close();
            OnError(this, err);
        }

        protected virtual void Socket_DataSent(TcpSocket sender, ushort length)
        {
            checkSendBuffers();
        }

        protected virtual void Socket_DataReceived(TcpSocket sender, byte[] bytes)
        {
            if (bytes == null)
            {
                // Local FIN recved
                Close();
            }
            else
            {
                ReadData(this, bytes);
                checkSendBuffers();
            }
        }

        public virtual void Close()
        {
            _tun.executeLwipTask(() => _socket.Close());
            Debug.WriteLine($"{TcpSocket.ConnectionCount()} connections now");
        }

        public virtual void Reset()
        {
            _tun.executeLwipTask(() => _socket.Abort());
        }

        public virtual void Write(byte[] bytes)
        {
            //for (int i = 0; i < bytes.Length; i += MAX_BUFF_SIZE)
            //{
            //    sendBuffers.Enqueue(bytes.AsBuffer(i, Math.Min(bytes.Length - i, MAX_BUFF_SIZE)));
            //    checkSendBuffers();
            //}
            sendBuffers.Enqueue(bytes.AsBuffer());
            checkSendBuffers();
        }

        public virtual void Dispose()
        {
            Close();
        }
    }
}
