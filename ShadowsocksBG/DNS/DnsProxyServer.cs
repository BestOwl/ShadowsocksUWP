using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DNS.Server;
using DNS.Protocol;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using DNS.Client;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Networking;
using DNS.Client.RequestResolver;
using DNS.Protocol.ResourceRecords;

namespace ShadowsocksBG.DNS
{
    public sealed class DnsProxyServer : IDisposable
    {
        private DnsClient client = new DnsClient("218.2.2.2");
        private static Dictionary<int, string> lookupTable = new Dictionary<int, string>();
        private static Dictionary<string, int> rlookupTable = new Dictionary<string, int>();

        public void Dispose()
        {
            lookupTable.Clear();
        }

        async private Task<IList<byte>> Query(
            byte[] payload)
        {
            var req = Request.FromArray(payload);
            Debug.WriteLine("DNS request: " + req.Questions[0].Name);
            //var res = await clireq.Resolve();
            Response res = new Response();
            byte[] ip = new byte[4] { 172, 17, 0, 0 };
            res.Id = req.Id;
            foreach (var q in req.Questions)
            {
                res.Questions.Add(q);
            }

            lock (lookupTable)
            {
                lock (rlookupTable)
                {
                    string n = req.Questions[0].Name.ToString();
                    if (rlookupTable.ContainsKey(n))
                    {
                        ip[3] = (byte)rlookupTable[n];
                        ResourceRecord answer = ResourceRecord.FromQuestion(req.Questions[0], ip);
                        res.AnswerRecords.Add(answer);
                    }
                    else
                    {
                        int i = lookupTable.Count();
                        lookupTable[i] = n;
                        rlookupTable[n] = i;
                        ip[3] = (byte)i;
                        ResourceRecord answer = ResourceRecord.FromQuestion(req.Questions[0], ip);
                        res.AnswerRecords.Add(answer);
                    }
                }
            }
            Debug.WriteLine("DNS request done: " + req.Questions[0].Name);
            //Debug.WriteLine(req);
            return res.ToArray();
        }

        public static string Lookup(byte ip)
        {
            return lookupTable[ip];
        }

        public IAsyncOperation<IList<byte>> QueryAsync(
            [ReadOnlyArray]
            byte[] payload)
        {
            return Query(payload).AsAsyncOperation();
        }
    }
}
