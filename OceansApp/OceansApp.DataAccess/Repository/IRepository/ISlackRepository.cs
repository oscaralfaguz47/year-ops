
namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ISlackRepository
    {
        Task SendMessageToChannelAsync(string token, string channel, string message);
        Task SendMessageToUserAsync(string token, string email, string message);
    }
}
