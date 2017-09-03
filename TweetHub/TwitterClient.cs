using System.Threading.Tasks;
using CoreTweet;

namespace TweetHub
{
    public class TwitterClient : ITwitterClient
    {
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly string _accessToken;
        private readonly string _accessTokenSecret;

        public TwitterClient(string apiKey, string apiSecret, string accessToken, string accessTokenSecret)
        {
            _apiKey = apiKey;
            _apiSecret = apiSecret;
            _accessToken = accessToken;
            _accessTokenSecret = accessTokenSecret;
        }

        public async Task TweetAsync(string text)
        {
            var tokens = Tokens.Create(_apiKey, _apiSecret, _accessToken, _accessTokenSecret);
            await tokens.Statuses.UpdateAsync(text);
        }
    }
}