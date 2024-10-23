using Microsoft.Extensions.Configuration;
using OceansApp.DataAccess.Repository.IRepository;
using SlackAPI;

namespace OceansApp.DataAccess.Repository
{
    public class SlackRepository : ISlackRepository
    {
        private readonly IConfiguration _config;
        private string _token;
        public SlackRepository(IConfiguration config)
        {
            _config = config;
            _token = Environment.GetEnvironmentVariable(_config["Slack:TokenAccountingApp"]);
        }
        public async Task SendMessageToChannelAsync(string channelId, string message)
        {
            try
            {
                var client = new SlackTaskClient(_token);

                var response = await client.PostMessageAsync(channelId, message);

                if (!response.ok)
                {
                    throw new Exception("Error al enviar el mensaje al canal de Slack: " + response.error);
                }
            }
            catch (HttpRequestException e)
            {
                throw new Exception("Error de red al intentar enviar mensaje a Slack: " + e.Message);
            }
            catch (Exception e)
            {
                throw new Exception("Error general al intentar enviar mensaje a Slack: " + e.Message);
            }
        }

        public async Task SendMessageToUserAsync(string email, string message)
        {
            var client = new SlackTaskClient(_token);
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

        public async Task<string> GetSlackUserIdByEmailAsync(string email)
        {
            var client = new SlackTaskClient(_token);
            var userResponse = await client.GetUserByEmailAsync(email);

            if (!userResponse.ok)
            {
                throw new Exception("Error finding user in Slack: " + userResponse.error);
            }
            return userResponse.user.id;
        }

    }
}
