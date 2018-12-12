using System;

namespace LanPlayGui.Model.GitHub
{
    public interface IAsset
    {
        Uri BrowserDownloadUrl { get; set; }
        string ContentType { get; set; }
        DateTimeOffset CreatedAt { get; set; }
        long DownloadCount { get; set; }
        long Id { get; set; }
        string Label { get; set; }
        string Name { get; set; }
        string NodeId { get; set; }
        long Size { get; set; }
        string State { get; set; }
        DateTimeOffset UpdatedAt { get; set; }
        IAuthor Uploader { get; set; }
        Uri Url { get; set; }
    }
}