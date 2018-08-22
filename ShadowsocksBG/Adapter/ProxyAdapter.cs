using Wintun2socks;

namespace ShadowsocksBG
{
    internal abstract class ProxyAdapter : TunSocketAdapter
    {
        public ProxyAdapter(TcpSocket socket, TunInterface tun) : base(socket, tun)
        {
            ReadData += ProxyAdapter_ReadData;
            OnError += ProxyAdapter_OnError;
        }

        protected virtual void RemoteReceived(byte[] e)
        {
            Write(e);
        }

        protected abstract void SendToRemote(byte[] e);
        protected abstract void DisconnectRemote();

        private void ProxyAdapter_ReadData(object sender, byte[] e)
        {
            SendToRemote(e);
        }

        private void ProxyAdapter_OnError(object sender, int err)
        {
            Close();
            DisconnectRemote();
        }

    }
}
