﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LanPlayGui.Model.GitHub
{
    public class Release : IRelease
    {
        public Release(IEnumerable<Asset> assets, Author author)
        {
            this.Assets = assets;
            this.Author = author;
        }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("assets_url")]
        public Uri AssetsUrl { get; set; }

        [JsonProperty("upload_url")]
        public string UploadUrl { get; set; }

        [JsonProperty("html_url")]
        public Uri HtmlUrl { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("node_id")]
        public string NodeId { get; set; }

        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        [JsonProperty("target_commitish")]
        public string TargetCommitish { get; set; }

        [JsonProperty("name")]
        public object Name { get; set; }

        [JsonProperty("draft")]
        public bool Draft { get; set; }

        [JsonProperty("author")]
        public IAuthor Author { get; set; }

        [JsonProperty("prerelease")]
        public bool Prerelease { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("published_at")]
        public DateTimeOffset PublishedAt { get; set; }

        [JsonProperty("assets")]
        public IEnumerable<IAsset> Assets { get; set; }

        [JsonProperty("tarball_url")]
        public Uri TarballUrl { get; set; }

        [JsonProperty("zipball_url")]
        public Uri ZipballUrl { get; set; }

        [JsonProperty("body")]
        public object Body { get; set; }
    }
}
