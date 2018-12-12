using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanPlayGui.Model
{
    public class LanPlayServerStatus : ILanPlayServerStatus
    {
        [JsonProperty("online")]
        public long OnlinePeople { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
