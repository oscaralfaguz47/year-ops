
namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ISlackRepository
    {
        Task SendMessageToChannelAsync(string token, string channelId, string message);
        Task SendMessageToUserAsync(string token, string email, string message);
        Task<string> GetSlackUserIdByEmailAsync(string token, string email);
    }
}
