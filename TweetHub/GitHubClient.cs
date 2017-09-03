using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TweetHub.Models;

namespace TweetHub
{
    public class GitHubClient : IGitHubClient
    {
        private const string ApiHost = "api.github.com";

        private static readonly HttpClient Client = new HttpClient();

        private readonly string _token;

        public GitHubClient(string token)
        {
            _token = token;
        }

        public async Task<IEnumerable<GitHubRepository>> GetUserRepositoriesAsync()
        {
            var url = $"https://api.github.com/user/repos";

            string result;

            using (var client = this.CreateWebClient(url))
            {
                result = await client.DownloadStringTaskAsync($"{url}");
            }

            Console.WriteLine(result);

            return JsonConvert.DeserializeObject<List<GitHubRepository>>(result);
        }

        private WebClient CreateWebClient(string url)
        {
            var client = new WebClient()
            {
                BaseAddress = url,
                CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore),
            };

            client.Headers[HttpRequestHeader.Authorization] = $"token {_token}";
            client.Headers[HttpRequestHeader.Accept] =
                "text/html,application/json,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            client.Headers[HttpRequestHeader.AcceptEncoding] = "br";
            client.Headers[HttpRequestHeader.AcceptLanguage] = "ja,en-US;q=0.8,en;q=0.6";
            client.Headers[HttpRequestHeader.UserAgent] =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";

            client.Encoding = Encoding.UTF8;

            return client;
        }
    }
}