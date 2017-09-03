using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TweetHub.Models;

namespace TweetHub.CommitTwitter
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            (AppOptions app, GitHubOptions github, TwitterOptions twitter) = BuildOptions(args);
            await new Executor(app, github, twitter).ExecuteAsync();
            Console.ReadKey();
        }

        private static (AppOptions, GitHubOptions, TwitterOptions) BuildOptions(string[] args)
        {
            // build options
            IConfigurationRoot root = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json")
                .AddCommandLine(args)
                .Build();

            var appOption = new AppOptions();
            root.GetSection("App").Bind(appOption);

            var gitHubOption = new GitHubOptions();
            root.GetSection("GitHub").Bind(gitHubOption);

            var twitterOption = new TwitterOptions();
            root.GetSection("Twitter").Bind(twitterOption);

            return (appOption, gitHubOption, twitterOption);
        }
    }

    internal class Executor
    {
        private readonly AppOptions _appOption;
        private readonly GitHubOptions _gitHubOption;
        private readonly TwitterOptions _twitterOption;

        internal Executor(AppOptions appOption, GitHubOptions gitHubOption, TwitterOptions twitterOption)
        {
            _appOption = appOption;
            _gitHubOption = gitHubOption;
            _twitterOption = twitterOption;
        }

        internal async Task ExecuteAsync()
        {
            var repositories = await this.GetUserRepositoriesAsync();
            var commitsDic = await this.GetCommitCountsAsync(repositories);

            var header = $"{_gitHubOption.AuthorName}'s commit {_appOption.TargetDate:yy/MM/dd}";
            
            var result = string.Join(Environment.NewLine, commitsDic.Select(p => $"{p.Key}: {p.Value}"));

            var text = $@"{header}

total: {commitsDic.Sum(p => p.Value)} commits
{result}
https://github.com/{_gitHubOption.AuthorName}";

            Console.WriteLine(text);

            await this.CreateTwitterClient().TweetAsync(text);
        }

        private async Task<IEnumerable<GitHubRepository>> GetUserRepositoriesAsync()
        {
            bool IsTarget(GitHubRepository repository)
                => _appOption.TargetDate.Date <= DateTime.Parse(repository.pushed_at).AddHours(_appOption.TimeDiff).Date;

            var client = this.CreateGitHubClient();
            return (await client.GetUserRepositoriesAsync())
                .Where(IsTarget);
        }

        private async Task<List<KeyValuePair<string, int>>> GetCommitCountsAsync(IEnumerable<GitHubRepository> repos)
        {
            var client = this.CreateGitHubClient();
            DateTime begin = _appOption.TargetDate.Date.AddHours(-_appOption.TimeDiff);
            DateTime end = begin.AddDays(1);
            string author = _gitHubOption.AuthorName;

            async Task<KeyValuePair<string, int>> GetInner(GitHubRepository r)
            {
                int count = await client.GetCommitCountAsync(r.full_name, begin, end, author);
                return new KeyValuePair<string, int>(r.name, count);
            }

            var tasks = repos.Select(async r => await GetInner(r)).ToList();
            await Task.WhenAll(tasks);

            return tasks.Select(t => t.Result).OrderByDescending(p => p.Value).ToList();
        }

        private IGitHubClient CreateGitHubClient()
        {
            return new GitHubClient(_gitHubOption.Token);
        }

        private ITwitterClient CreateTwitterClient()
        {
            return new TwitterClient(_twitterOption.ApiKey, _twitterOption.ApiKeySecret, _twitterOption.AccessToken, _twitterOption.AccessTokenSecret);
        }
    }
}
