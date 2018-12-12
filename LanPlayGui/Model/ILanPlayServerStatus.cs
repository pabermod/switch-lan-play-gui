namespace LanPlayGui.Model
{
    public interface ILanPlayServerStatus
    {
        long OnlinePeople { get; set; }
        string Version { get; set; }
    }
}