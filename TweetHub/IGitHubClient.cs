using System.Collections.Generic;
using System.Threading.Tasks;
using TweetHub.Models;

namespace TweetHub
{
    public interface IGitHubClient
    {
        Task<IEnumerable<GitHubRepository>> GetUserRepositoriesAsync();
    }
}