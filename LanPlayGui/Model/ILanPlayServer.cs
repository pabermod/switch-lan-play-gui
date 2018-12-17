using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace LanPlayGui.Model
{
    public interface ILanPlayServer : INotifyPropertyChanged
    {
        string Name { get; set; }
        Uri Uri { get; set; }
        ServerStatus Status { get; set; }
        string Version { get; set; }
        long Online { get; set; }
        long Ping { get; set; }
    }
}