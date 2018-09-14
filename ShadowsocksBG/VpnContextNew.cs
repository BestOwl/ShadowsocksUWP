using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tun2SocksWinRT;

namespace ShadowsocksBG
{
    internal class VpnContextNew
    {
        public Task TunTask;
        public Tun2Socks Tun;

        public void InitTun2Socks(string tunServiceName, string vlanAddr, string vlanNetmask, int mtu, string socksServerAddr, string socksServerPasswd)
        {
            Tun = new Tun2Socks();
            TunTask = Task.Run(() =>
            {
                Tun.Init(tunServiceName, vlanAddr, vlanNetmask, mtu, socksServerAddr, socksServerPasswd);
            });
        }
    }
}
