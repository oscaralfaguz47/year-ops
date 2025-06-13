using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;

namespace OceansApp.DataAccess.Repository
{
    public class HourReportValidationServiceRepository : IHourReportValidationServiceRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IOcrServiceRepository _ocrService;
        private readonly IOpenAIRepository _openAI;

        public HourReportValidationServiceRepository(ApplicationDbContext db, IOcrServiceRepository ocrService, IOpenAIRepository openAI)
        {
            _db = db;
            _ocrService = ocrService;
            _openAI = openAI;
        }

        public async Task<(bool isValid, string message)> ValidateMatchingReportsAsync(int movementId, string primaryToolName, string secondToolName)
        {
            var primaryUrls = await _db.REPORTING_MY_TIME_MOVEMENT_BLOBS
                .Where(x => x.MovementId == movementId && x.PrimaryReportTrackingToolName.Trim() == primaryToolName.Trim())
                .Select(x => x.BlobUrl)
                .ToListAsync();

            var secondUrls = await _db.REPORTING_MY_TIME_MOVEMENT_BLOBS
                .Where(x => x.MovementId == movementId && x.SecondReportTrackingToolName.Trim() == secondToolName.Trim())
                .Select(x => x.BlobUrl)
                .ToListAsync();

            if (!primaryUrls.Any() || !secondUrls.Any())
                return (false, "One or both report sources are missing for validation.");

            var primaryTextTasks = primaryUrls.Select(url => _ocrService.ExtractTextFromFileAsync(url));
            var secondTextTasks = secondUrls.Select(url => _ocrService.ExtractTextFromFileAsync(url));

            var primaryTexts = await Task.WhenAll(primaryTextTasks);
            var secondTexts = await Task.WhenAll(secondTextTasks);

            // Concatenate all extracted texts
            var combinedPrimaryText = string.Join("\n\n", primaryTexts);
            var combinedSecondText = string.Join("\n\n", secondTexts);

            // Send combined text to OpenAI
            var result = await _openAI.CompareReportsAsync(combinedPrimaryText, combinedSecondText);


            if (result.Contains("match", StringComparison.OrdinalIgnoreCase) &&
                !result.Contains("difference", StringComparison.OrdinalIgnoreCase))
            {
                return (true, "The uploaded reports appear to match.");
            }
            else
            {
                return (false, $"Mismatch detected between reports: {result}");
            }
        }
    }
}
