using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
            var githubClient = this.CreateGitHubClient();

            var repositories = (await githubClient.GetUserRepositoriesAsync())
                .Where(this.IsTarget);
            
            var result = string.Join(", ", repositories.Select(r => r.name));

            Console.WriteLine(result);
        }

        private bool IsTarget(GitHubRepository repository)
        {
            return _appOption.TargetDate.Date == DateTime.Parse(repository.pushed_at).AddHours(_appOption.TimeDiff).Date;
        }

        private GitHubClient CreateGitHubClient()
        {
            return new GitHubClient(_gitHubOption.Token);
        }
    }
}
