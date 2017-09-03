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
    }
}