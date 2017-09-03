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
            await new Executor(args).ExecuteAsync();
            Console.ReadKey();
        }
    }

    internal class Executor
    {
        private readonly AppOptions _appOption;
        private readonly GitHubOptions _gitHubOption;

        internal Executor(string[] args)
        {
            // build options
            IConfigurationRoot root = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json")
                .AddCommandLine(args)
                .Build();

            _appOption = new AppOptions();
            root.GetSection("App").Bind(_appOption);

            _gitHubOption = new GitHubOptions();
            root.GetSection("GitHub").Bind(_gitHubOption);

            Console.WriteLine(JsonConvert.SerializeObject(_appOption, Formatting.Indented));
            Console.WriteLine(JsonConvert.SerializeObject(_gitHubOption, Formatting.Indented));
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
