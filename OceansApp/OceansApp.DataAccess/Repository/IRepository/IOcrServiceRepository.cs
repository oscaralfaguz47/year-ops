
namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IOcrServiceRepository
    {
        Task<string> ExtractTextFromFileAsync(string fileUrl);
    }
}
