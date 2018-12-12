using System;
using System.Threading.Tasks;

namespace LanPlayGui.Model
{
    public interface ILanPlayServer
    {
        ILanPlayServerStatus Status { get; set; }
        Uri Uri { get; set; }
    }
}