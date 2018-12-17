using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanPlayGui.Model
{
    public class LanPlayServerStatus
    {
        [JsonProperty("online")]
        public long Online { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        public override string ToString()
        {
            return $"{Online} - {Version}";
        }
    }
}
