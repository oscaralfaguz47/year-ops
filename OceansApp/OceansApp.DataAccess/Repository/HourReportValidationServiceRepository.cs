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

            var primaryTexts = await Task.WhenAll(primaryUrls.Select(_ocrService.ExtractLayoutTextFromFileAsync));
            var secondTexts = await Task.WhenAll(secondUrls.Select(_ocrService.ExtractLayoutTextFromFileAsync));

            var formattedPrimary = string.Join("\n---\n", primaryTexts);
            var formattedSecond = string.Join("\n---\n", secondTexts);

            var result = await _openAI.CompareReportsAsync(formattedPrimary, formattedSecond, primaryToolName, secondToolName);
            var trimmed = result.Trim().Trim('"');

            bool isMatch = trimmed.Contains("Reports match.", StringComparison.OrdinalIgnoreCase);
            return (isMatch, isMatch ? "The uploaded reports appear to match." : $"{trimmed}");
        }


    }






}
