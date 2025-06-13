using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OceansApp.Models.ViewModels.OpenAI
{
    public class OpenAIOptions
    {
        public string ApiKey { get; set; }
        public string Model { get; set; } = "gpt-4o";
    }
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAIOptions _options;

        public OpenAIService(IOptions<OpenAIOptions> options)
        {
            _httpClient = new HttpClient();
            _options = options.Value;
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

            var body = new
            {
                model = _options.Model,
                messages = new[]
                {
                new { role = "user", content = prompt }
            }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
    }
}
