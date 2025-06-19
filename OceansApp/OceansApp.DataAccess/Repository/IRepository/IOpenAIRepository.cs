
namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IOpenAIRepository
    {
        Task<string> CompareReportsAsync(string content1, string content2, string primaryToolName, string secondToolName);
    }

}
