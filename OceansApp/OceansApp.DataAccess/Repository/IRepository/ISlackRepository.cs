
namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ISlackRepository
    {
        Task SendMessageToChannelAsync(string channelId, string message);
        Task SendMessageToUserAsync(string email, string message);
        Task<string> GetSlackUserIdByEmailAsync(string email);
    }
}
