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

        private static long PingHost(string nameOrAddress)
        {
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                return reply.RoundtripTime;
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

            return 0;
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
                    await UpdateServerStatus(server);
                }));
            }
            await Task.WhenAll(tasks);
        }

        public async Task UpdateServerStatus(ILanPlayServer server)
        {
            Console.WriteLine($"Updating server {server.Name}");
            long ping = PingHost(server.Uri.Host);
            if (ping == 0)
            {
                SetServerToOffline(server);
                return;
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
                        server.Online = status.Online;
                        server.Ping = ping;

                        Console.WriteLine($"Server {server.Name} updated");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request exception to {server.Name}: {ex.Message}");
                SetServerToOffline(server);
                return;
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"Timed out request to {server.Name}");
                SetServerToOffline(server);
                return;
            }
        }

        private static void SetServerToOffline(ILanPlayServer server)
        {
            server.Status = ServerStatus.Offline;
            server.Ping = 0;
            server.Version = "Unknown";
            server.Online = 0;
        }
    }
}
