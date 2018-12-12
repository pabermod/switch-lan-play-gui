using LanPlayGui.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanPlayGui.Service
{
    public class LanPlayServerService
    {
        private const string serverListFileName = "serverlist.txt";

        public IEnumerable<ILanPlayServer> Servers { get; set; }

        public LanPlayServerService()
        {
            Servers = new List<LanPlayServer>();
        }

        public async Task InitializeAsync()
        {
            IList<LanPlayServer> serverList = new List<LanPlayServer>();
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
    }
}
