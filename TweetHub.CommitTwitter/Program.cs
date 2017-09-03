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
            (AppOptions app, GitHubOptions github) = BuildOptions(args);
            await new Executor(app, github).ExecuteAsync();
            Console.ReadKey();
        }

        private static (AppOptions, GitHubOptions) BuildOptions(string[] args)
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

            return (appOption, gitHubOption);
        }
    }

    internal class Executor
    {
        private readonly AppOptions _appOption;
        private readonly GitHubOptions _gitHubOption;

        internal Executor(AppOptions appOption, GitHubOptions gitHubOption)
        {
            _appOption = appOption;
            _gitHubOption = gitHubOption;
        }

        internal async Task ExecuteAsync()
        {
            var repositories = await this.GetUserRepositoriesAsync();
            var commitsDic = await this.GetCommitCountsAsync(repositories);

            var header = $"{_gitHubOption.AuthorName}'s commit {_appOption.TargetDate:yy/MM/dd}";
            
            var result = string.Join(Environment.NewLine, commitsDic.Select(p => $"{p.Key}: {p.Value}"));

            Console.WriteLine($@"{header}

total: {commitsDic.Sum(p => p.Value)} commits
{result}
https://github.com/{_gitHubOption.AuthorName}");
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

        private GitHubClient CreateGitHubClient()
        {
            return new GitHubClient(_gitHubOption.Token);
        }
    }
}
