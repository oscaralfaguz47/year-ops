using Microsoft.Extensions.Configuration;
using OceansApp.DataAccess.Repository.IRepository;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
namespace OceansApp.DataAccess.Repository
{
    public class OpenAIRepository : IOpenAIRepository
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient = new();

        public OpenAIRepository(IConfiguration config)
        {
            _apiKey = config["OpenAIApiKey"];
        }

        public async Task<string> CompareReportsAsync(string content1, string content2)
        {
            var prompt = $"""
        Compare the following two timesheet reports. Determine if the reported hours per day and per week match. If they don’t, explain the differences.

        --- Report A ---
        {content1}

        --- Report B ---
        {content2}
        """;

            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[]
                {
                new { role = "user", content = prompt }
            }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            using var json = JsonDocument.Parse(responseContent);
            return json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
    }


}