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
        public LanPlayServer(Uri uri)
        {
            Uri = uri;
        }

        public string Name
        {
            get
            {
                return string.IsNullOrEmpty(name) ? Uri.Host : name;
            }
            set
            {
                name = value;
            }
        }

        private string name;

        public Uri Uri { get; set; }

        public ServerStatus Status { get; set; }

        public string Version { get; set; }

        public long OnlinePeople { get; set; }

        public override string ToString()
        {
            return Uri.Host;
        }
    }
}
