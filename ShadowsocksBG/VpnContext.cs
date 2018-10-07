using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tun2SocksWinRT;
using Windows.Networking;
using Windows.Networking.Vpn;

namespace ShadowsocksBG
{
    internal class VpnContext
    {
        internal Task TunTask;
        internal Tun2Socks Tun;

        internal VpnDomainNameAssignment domainAssignment = new VpnDomainNameAssignment();

        internal VpnRouteAssignment routeScope = new VpnRouteAssignment()
        {
            ExcludeLocalSubnets = true
        };

        internal bool isInited { get; private set; } = false;

        public void Init()
        {
            var inclusionRoutes = routeScope.Ipv4InclusionRoutes;
            // qzworld.net
            //inclusionRoutes.Add(new VpnRoute(new HostName("188.166.248.242"), 32));
            // DNS server
            //inclusionRoutes.Add(new VpnRoute(new HostName("1.1.1.1"), 32));
            // main CIDR
            //inclusionRoutes.Add(new VpnRoute(new HostName("172.17.0.0"), 16));

            //var assignment = new VpnDomainNameAssignment();
            //var dnsServers = new[]
            //{
            //    // DNS servers
            //    new HostName("1.1.1.1")
            //};
            //assignment.DomainNameList.Add(new VpnDomainNameInfo(".", VpnDomainNameType.Suffix, dnsServers, new HostName[] { }));

            isInited = true;
        }

        private void CreateRouteList(IList<VpnRoute> list, HostName dst)
        {
            int[] dstIP = new int[4];
            string[] dstHost = dst.RawName.Split('.');
            if (dstHost.Length != 4)
            {
                return;
            }
            for(int i = 0; i < 4; i++)
            {
                int.TryParse(dstHost[i], out dstIP[i]);
            }

            for (int i = 0; i < 240; i++) // exclude 240.0.0.0/4
            {
                if (i == 10)
                {
                    continue;
                }
                else if (i == 127)
                {
                    continue;
                }
                else if (i == dstIP[1])
                {
                    continue;
                }

                list.Add(new VpnRoute(new HostName(string.Format("{0}.0.0.0", i)), 8));
            }
        }

        public void InitTun2Socks(string tunServiceName, string vlanAddr, string vlanNetmask, int mtu, string socksServerAddr, string cryptoMethod, string socksServerPasswd)
        {
            Tun = new Tun2Socks();
            TunTask = Task.Run(() =>
            {
                Tun.Init(tunServiceName, vlanAddr, vlanNetmask, mtu, socksServerAddr, cryptoMethod, socksServerPasswd);
            });
        }
    }
}
