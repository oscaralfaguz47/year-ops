using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Consultants;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantPaymentRepository : IRepository<ConsultantPayment>
    {
        Task<MethodResponse> GetMovementsToPay(ConsultantUserVM consultant, DateTime startDate,
            DateTime endDate);
    }
}
