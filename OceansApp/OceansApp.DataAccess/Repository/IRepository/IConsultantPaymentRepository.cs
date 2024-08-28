using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantPayments;
using OceansApp.Models.ViewModels.Consultants;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantPaymentRepository : IRepository<ConsultantPayment>
    {
        Task<MethodResponse> GetMovementsToPay(ConsultantUserVM consultant, DateTime startDate,
            DateTime endDate);
        Task<MethodResponse> CreatePayment(string userIdCreatedBy,
            CreateUpdateConsultantPaymentVM paymentData, decimal accountPayableAmount);
        Task<MethodResponse> UpdatePayment(string userIdCreatedBy,
            CreateUpdateConsultantPaymentVM paymentData);
        Task<List<GetConsultantPaymentsInPeriodVM>> GetConsultantPaymentsInPeriod(int consultantId, DateTime startDate,
            DateTime endDate);
    }
}
