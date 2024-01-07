using OceansApp.Models.ViewModels;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ISendEmailRepository
    {

        Task<string?> SendEmail(SendEmailVM emailModel);
    }
}
