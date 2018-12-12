using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LanPlayGui.Model
{
    public class LanPlayServer : ILanPlayServer
    {
        private static HttpClient httpClient = new HttpClient();

        public LanPlayServer(Uri uri)
        {
            Uri = uri;
        }

        public Uri Uri { get; set; }

        public ILanPlayServerStatus Status { get; set; }

        public Task UpdateStatus()
        {
            return Task.CompletedTask;
        }
    }
}
