
using AzureFunctionsApp.Models;

namespace AzureFunctionsApp.Repository.IRepository
{
    public interface ISendEmailRepository
    {
        Task<string?> SendEmail(SendEmailVM emailModel);
    }
}
