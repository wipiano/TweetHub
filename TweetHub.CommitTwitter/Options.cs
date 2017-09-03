using System;

namespace TweetHub.CommitTwitter
{
    public class AppOptions
    {
        public DateTime TargetDate { get; set; }

        public int TimeDiff { get; set; }
    }

    public class GitHubOptions
    {
        public string Token { get; set; }

        public string AuthorName { get; set; }
    }

    public class TwitterOptions
    {
        public string ApiKey { get; set; }

        public string ApiKeySecret { get; set; }

        public string AccessToken { get; set; }

        public string AccessTokenSecret { get; set; }
    }
}