using LanPlayGui.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace LanPlayGui.Service
{
    public class LanPlayServerService
    {
        private const string serverListFileName = "serverlist.txt";

        private static HttpClient httpClient = new HttpClient();

        private static Uri GetInfoUri(ILanPlayServer server)
        {
            return new Uri(server.Uri, "info");
        }

        private static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingable;
        }

        public IEnumerable<ILanPlayServer> Servers { get; set; }

        public LanPlayServerService()
        {
            Servers = new List<ILanPlayServer>();

            httpClient.Timeout = new TimeSpan(0, 0, 5);
        }

        public async Task InitializeAsync()
        {
            IList<ILanPlayServer> serverList = new List<ILanPlayServer>();
            using (TextReader reader = File.OpenText(serverListFileName))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (!line.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                        line = "http://" + line;
                    serverList.Add(new LanPlayServer(new Uri(line)));
                }
            }
            Servers = serverList;
        }

        public async void UpdateServersStatus()
        {
            IList<Task> tasks = new List<Task>();
            foreach (var server in Servers)
            {
                tasks.Add(Task.Run(async () =>
                {
                    Console.WriteLine($"Sending Ping to {server.Name}");
                    if (!PingHost(server.Uri.Host))
                    {
                        server.Status = ServerStatus.Offline;
                    }
                    try
                    {
                        Console.WriteLine($"Sending request to {server.Name}");
                        using (HttpResponseMessage response = await httpClient.GetAsync(GetInfoUri(server)))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                string resultString = await response.Content.ReadAsStringAsync();
                                var status = JsonConvert.DeserializeObject<LanPlayServerStatus>(resultString);

                                server.Status = ServerStatus.Online;
                                server.Version = status.Version;
                                server.OnlinePeople = status.OnlinePeople;
                            }
                        }
                    }
                    catch (HttpRequestException)
                    {
                        Console.WriteLine($"Request exception to {server.Name}");
                        server.Status = ServerStatus.Offline;
                        return;
                    }
                    catch (TaskCanceledException)
                    {
                        Console.WriteLine($"Timed out request to {server.Name}");
                        server.Status = ServerStatus.Offline;
                        return;
                    }
                }));
            }
            await Task.WhenAll(tasks);
        }
    }
}
