using System.Threading.Tasks;

namespace TweetHub
{
    public interface ITwitterClient
    {
        Task TweetAsync(string text);
    }
}