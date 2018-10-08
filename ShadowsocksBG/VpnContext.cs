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

        internal VpnTrafficFilterAssignment trafficScope = new VpnTrafficFilterAssignment();

        internal bool isInited { get; private set; } = false;

        public void Init()
        {
            var inclusionRoutes = routeScope.Ipv4InclusionRoutes;
            CreateRouteList(inclusionRoutes, new HostName("188.166.248.242"));
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

        /// <summary>
        /// Create route list to route all traffic except ss-remote to VPN interface
        /// </summary>
        /// <param name="list">Route inclusion list</param>
        /// <param name="dst">ss-remote ip address</param>
        /// <remarks>
        /// This is the temporary solution to route all traffic to the VPN interface since ExclusionRoutes dose not work at all
        /// and we cant get any support from microsoft.
        /// </remarks>
        /// <seealso cref="https://social.msdn.microsoft.com/Forums/en-US/eddf881d-d823-4a69-8aa3-df661f5abc0b/routing-issue-with-uwp-vpnplugin?forum=wpdevelop"/>
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
                else if (i == dstIP[0])
                {
                    for (int i2 = 0; i2 <= 255; i2++)
                    {
                        if (i2 == dstIP[1])
                        {
                            for (int i3 = 0; i3 <= 255; i3++)
                            {
                                if (i3 == dstIP[2])
                                {
                                    for (int i4 = 1; i4 < 255; i4++)
                                    {
                                        if (i4 == dstIP[3])
                                        {
                                            continue;
                                        }
                                        list.Add(new VpnRoute(new HostName(string.Format("{0}.{1}.{2}.{3}", i, i2, i3, i4)), 32));
                                    }
                                    continue;
                                }
                                list.Add(new VpnRoute(new HostName(string.Format("{0}.{1}.{2}.0", i, i2, i3)), 24));
                            }
                            continue;
                        }
                        list.Add(new VpnRoute(new HostName(string.Format("{0}.{1}.0.0", i, i2)), 16));
                    }
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
