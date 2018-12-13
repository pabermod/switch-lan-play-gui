using System;
using System.Threading.Tasks;

namespace LanPlayGui.Model
{
    public interface ILanPlayServer
    {
        string Name { get; set; }
        Uri Uri { get; set; }
        ServerStatus Status { get; set; }
        string Version { get; set; }
        long OnlinePeople { get; set; }
    }
}