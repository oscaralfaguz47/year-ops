namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IHourReportValidationServiceRepository
    {
        Task<(bool isValid, string message)> ValidateMatchingReportsAsync(int movementId, string primaryToolName, string secondToolName, DateTime startDate, DateTime endDate);
    }
}
