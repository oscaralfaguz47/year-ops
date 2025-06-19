
namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IOcrServiceRepository
    {
        Task<string> ExtractLayoutTextFromFileAsync(string fileUrl);
    }
}
