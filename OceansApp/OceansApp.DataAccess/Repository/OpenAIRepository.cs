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

        public async Task<string> CompareReportsAsync(string content1, string content2, string primaryToolName, string secondToolName)
        {
            var prompt = $$"""
You are a highly strict and detail-oriented validator of timesheet reports. Your task is to analyze and compare the total time and its distribution in two reports.

🧠 IMPORTANT: You must verify the **total reported hours**. If **the total differs**, it is considered a mismatch.

You MUST detect and interpret all the following formats:
- Time formats like "8 h - 0 m", "4h 5m", or "3.5 hours".
- Time ranges like "8:00am - 4:00pm", and calculate duration.
- Aggregated totals like "Total Hours: 42.75", "Billable Hours: 40.00".
- Daily hours listed by weekday or calendar date (e.g., "Monday 4", "06/10/2025 8.0").
- Horizontal or vertical tables of numbers aligned to days/dates.
- Mixed languages (Spanish or English): "Lunes", "Tue 6/10", "10 de Junio".
- Reports separated by week or ranges (e.g., "06/01/2025 to 06/07/2025").

📁 MULTIPLE REPORT BLOCKS:
- Each tool's report may contain **multiple independent report blocks** (e.g., multiple date ranges or biweekly reports).
- These blocks are visually separated by `---`.
- You must identify **all blocks** within Report A ({{primaryToolName}}) and Report B ({{secondToolName}}).
- For each tool, **sum the total hours across ALL its blocks**, even if they cover different periods.
- You must also verify if the combined **date ranges** are the same between both tools.
- For example: "01 - 15 Jun 2025" and "16 - 30 Jun 2025" together form a single reporting period of June.
- Make sure to review **every block** inside each report and sum all durations accurately before comparing.

📅 VALIDATION RULES:
1. If the total hours AND daily distributions match exactly → ✅ It's a match.
2. If total hours match, you must only declare mismatch in day/date distribution if there is a **real difference in values per day or date**. Do not assume mismatch based on layout or structure.
3. If total hours differ → ❌ Mismatch.
4. If reports belong to **different reporting periods**, this is also a ❌ Mismatch.
5. If total hours (including minutes) match exactly, and no date-by-date distribution is present in either report → ✅ This is a match.

⚠️ DO NOT explain your reasoning. Respond with **one of the following only**:

If both reports are exactly equal in time and structure:
> Reports match.

If the totals differ:
> ❌ Mismatch found: total reported time is X in the '{{primaryToolName}}' report, and Y in the '{{secondToolName}}' report.

--- Report A ({{primaryToolName}}) ---
{{content1}}

--- Report B ({{secondToolName}}) ---
{{content2}}
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