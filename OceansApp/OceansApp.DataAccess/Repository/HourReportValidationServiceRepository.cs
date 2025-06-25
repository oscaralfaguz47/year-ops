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

        public async Task<(bool isValid, string message)> ValidateMatchingReportsAsync(int movementId, string primaryToolName, string secondToolName, DateTime startDate, DateTime endDate)
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

            bool allPrimaryAreImages = primaryUrls.All(url => IsImageFile(url));
            bool allSecondAreImages = secondUrls.All(url => IsImageFile(url));
            bool allAreImages = allPrimaryAreImages && allSecondAreImages;

            string formattedPrimary;
            string formattedSecond;

            if (allAreImages)
            {
                formattedPrimary = string.Join("\n---\n", primaryUrls);
                formattedSecond = string.Join("\n---\n", secondUrls);
            }
            else
            {
                var primaryTexts = await Task.WhenAll(primaryUrls.Select(_ocrService.ExtractLayoutTextFromFileAsync));
                var secondTexts = await Task.WhenAll(secondUrls.Select(_ocrService.ExtractLayoutTextFromFileAsync));

                formattedPrimary = string.Join("\n---\n", primaryTexts);
                formattedSecond = string.Join("\n---\n", secondTexts);
            }

            var result = await _openAI.CompareReportsAsync(formattedPrimary, formattedSecond, primaryToolName, secondToolName, startDate, endDate, allAreImages);
            var trimmed = result.Trim().Trim('"');

            bool isMatch = trimmed.Contains("Reports match.", StringComparison.OrdinalIgnoreCase);
            return (isMatch, isMatch ? "The uploaded reports appear to match." : $"{trimmed}");
        }
        private bool IsImageFile(string url)
        {
            return url.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                || url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                || url.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                || url.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase)
                || url.EndsWith(".webp", StringComparison.OrdinalIgnoreCase);
        }
    }







}
