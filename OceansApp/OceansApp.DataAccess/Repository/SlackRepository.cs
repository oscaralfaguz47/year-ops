using OceansApp.DataAccess.Repository.IRepository;
using SlackAPI;

namespace OceansApp.DataAccess.Repository
{
    public class SlackRepository : ISlackRepository
    {
        public async Task SendMessageToChannelAsync(string token, string channelId, string message)
        {
            var client = new SlackTaskClient(token);

            var response = await client.PostMessageAsync(channelId, message);

            if (!response.ok)
            {
                throw new Exception("Error al enviar el mensaje al canal de Slack: " + response.error);
            }
        }

        public async Task SendMessageToUserAsync(string token, string email, string message)
        {
            var client = new SlackTaskClient(token);
            var userResponse = await client.GetUserByEmailAsync(email);

            if (!userResponse.ok)
            {
                throw new Exception("Error al encontrar el usuario en Slack: " + userResponse.error);
            }

            var messageResponse = await client.PostMessageAsync(userResponse.user.id, message);

            if (!messageResponse.ok)
            {
                throw new Exception("Error al enviar el mensaje al usuario de Slack: " + messageResponse.error);
            }
        }

        public async Task<string> GetSlackUserIdByEmailAsync(string token, string email)
        {
            var client = new SlackTaskClient(token);
            var userResponse = await client.GetUserByEmailAsync(email);

            if (!userResponse.ok)
            {
                throw new Exception("Error al encontrar el usuario en Slack: " + userResponse.error);
            }
            return userResponse.user.id;
        }
    }
}
