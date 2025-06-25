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

        public async Task<string> CompareReportsAsync(
           string content1,
           string content2,
           string primaryToolName,
           string secondToolName,
           DateTime startDate,
           DateTime endDate,
           bool useImagePrompt = false)
        {
            string formattedStartDate = startDate.ToString("yyyy-MM-dd");
            string formattedEndDate = endDate.ToString("yyyy-MM-dd");

            var prompt = useImagePrompt
                ? $$"""
You are a visual validator of timesheet reports. Carefully analyze the visual content from both Report A and Report B. These are image-based representations of timesheet data.

🧠 Your task:
- Identify and manually sum only the hours reported **between {{formattedStartDate}} and {{formattedEndDate}}**, inclusive.
- If any hours are outside this date range, they must be ignored.
- If the hours differ → ❌ Mismatch.
- If both totals match → ✅ Match.

You MUST detect and interpret all the following formats:
- Time formats like "8 h - 0 m", "4h 5m", or "3.5 hours".
- Time ranges like "8:00am - 4:00pm", and calculate duration.
- Aggregated totals like "Total Hours: 42.75", "Billable Hours: 40.00".
- Daily hours listed by weekday or calendar date (e.g., "Monday 4", "06/10/2025 8.0").
- Horizontal or vertical tables of numbers aligned to days/dates.
- Mixed languages (Spanish or English): "Lunes", "Tue 6/10", "10 de Junio".
- Reports separated by week or ranges (e.g., "06/01/2025 to 06/07/2025").
- You must manually calculate the total hours shown, not rely on visible total summaries.
- Sum all rows (each job, each task) even if they belong to the same day.
- Ignore any day without numeric values (e.g., Sunday with blank).
- Include On-Call Time or similar only if there are hours reported next to it.
- ⚠️ Ignore any hours outside the range of {{formattedStartDate}} to {{formattedEndDate}}.

📅 VALIDATION RULES:
1. If the total hours within the specified date range AND daily distributions match → ✅ It's a match.
2. If totals differ within the date range → ❌ Mismatch.
3. If there’s a mismatch in dates or total within the valid range → ❌ Mismatch.
4. If unsure about alignment or clarity → ❌ Mismatch.

⚠️ DO NOT explain your reasoning. Respond with one of:
> Reports match.
> ❌ Mismatch found: total reported time is X in the '{{primaryToolName}}' report, and Y in the '{{secondToolName}}' report.

--- Report A ({{primaryToolName}}) ---
{{content1}}

--- Report B ({{secondToolName}}) ---
{{content2}}
"""
                : $$"""
You are a highly strict and detail-oriented validator of timesheet reports. Your task is to analyze and compare the total time and its distribution in two reports.

🧠 IMPORTANT: You must verify the **total reported hours** strictly within the date range from {{formattedStartDate}} to {{formattedEndDate}}, inclusive.

You MUST detect and interpret all the following formats:
- Time formats like "8 h - 0 m", "4h 5m", or "3.5 hours".
- Time ranges like "8:00am - 4:00pm", and calculate duration.
- Aggregated totals like "Total Hours: 42.75", "Billable Hours: 40.00".
- Daily hours listed by weekday or calendar date (e.g., "Monday 4", "06/10/2025 8.0").
- Horizontal or vertical tables of numbers aligned to days/dates.
- Mixed languages (Spanish or English): "Lunes", "Tue 6/10", "10 de Junio".
- Reports separated by week or ranges (e.g., "06/01/2025 to 06/07/2025").
- If a day has no reported hours (e.g., Sunday with empty or zero), do NOT consider it part of the total.
- Always sum all rows in the report if multiple rows report hours (e.g., multiple projects per day).
- Do NOT trust any displayed total — always validate by summing each day's hours.
- Include On-Call Time, Flat Rate, or similar only if hours are explicitly reported.
- ⚠️ Ignore any hours outside the date range {{formattedStartDate}} to {{formattedEndDate}}.

📁 MULTIPLE REPORT BLOCKS:
- Each tool's report may contain multiple independent report blocks (e.g., multiple date ranges or biweekly reports).
- These blocks are visually separated by `---`.
- You must identify all blocks within Report A ({{primaryToolName}}) and Report B ({{secondToolName}}).
- Sum the total hours ONLY for dates between {{formattedStartDate}} and {{formattedEndDate}}.
- Do not consider any block partially or entirely outside the range.

📅 VALIDATION RULES:
1. If the total hours AND daily distributions match exactly within the date range → ✅ It's a match.
2. If total hours differ → ❌ Mismatch.
3. If reports belong to different reporting periods → ❌ Mismatch.
4. If total hours (including minutes) match and no date-by-date distribution is present → ✅ Match.
5. Ignore hours outside the specified date range completely.
6. Do not assume hours unless values are explicitly shown.

⚠️ DO NOT explain your reasoning. Respond with one of:
> Reports match.
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