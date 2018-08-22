namespace ShadowsocksBG
{
    internal delegate void ReadDataHandler(object sender, byte[] e);
    internal delegate void SocketErrorHandler(object sender, int err);
    internal interface IAdapterSocket
    {
        void Write(byte[] bytes);
        event ReadDataHandler ReadData;
        event SocketErrorHandler OnError;
        void Close();
        void Reset();
    }
}
