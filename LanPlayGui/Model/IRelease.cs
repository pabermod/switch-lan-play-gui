using System;
using System.Collections.Generic;

namespace LanPlayGui.Model
{
    public interface IRelease
    {
        IEnumerable<IAsset> Assets { get; set; }
        Uri AssetsUrl { get; set; }
        IAuthor Author { get; set; }
        object Body { get; set; }
        DateTimeOffset CreatedAt { get; set; }
        bool Draft { get; set; }
        Uri HtmlUrl { get; set; }
        long Id { get; set; }
        object Name { get; set; }
        string NodeId { get; set; }
        bool Prerelease { get; set; }
        DateTimeOffset PublishedAt { get; set; }
        string TagName { get; set; }
        Uri TarballUrl { get; set; }
        string TargetCommitish { get; set; }
        string UploadUrl { get; set; }
        Uri Url { get; set; }
        Uri ZipballUrl { get; set; }
    }
}