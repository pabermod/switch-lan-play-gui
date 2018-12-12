using System;

namespace LanPlayGui.Model.GitHub
{
    public interface IAuthor
    {
        Uri AvatarUrl { get; set; }
        Uri EventsUrl { get; set; }
        Uri FollowersUrl { get; set; }
        Uri FollowingUrl { get; set; }
        Uri GistsUrl { get; set; }
        string GravatarId { get; set; }
        Uri HtmlUrl { get; set; }
        long Id { get; set; }
        string Login { get; set; }
        string NodeId { get; set; }
        Uri OrganizationsUrl { get; set; }
        Uri ReceivedEventsUrl { get; set; }
        Uri ReposUrl { get; set; }
        bool SiteAdmin { get; set; }
        Uri StarredUrl { get; set; }
        Uri SubscriptionsUrl { get; set; }
        string Type { get; set; }
        Uri Url { get; set; }
    }
}