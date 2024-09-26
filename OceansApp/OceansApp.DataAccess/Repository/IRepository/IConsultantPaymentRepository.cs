using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.AccountsPayable;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantPayments;
using OceansApp.Models.ViewModels.Consultants;
using OceansApp.Models.ViewModels.PaymentSheets;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantPaymentRepository : IRepository<ConsultantPayment>
    {
        Task<MethodResponse> GetMovementsToPay(ConsultantUserVM consultant, DateTime startDate,
            DateTime endDate);
        Task<MethodResponse> CreatePayment(string userIdCreatedBy,
            CreateUpdateConsultantPaymentVM paymentData, decimal accountPayableAmount, GetListOfMovementsForPaymentVM listOfMovementsForPayment);
        Task<MethodResponse> UpdatePayment(string userIdCreatedBy,
            CreateUpdateConsultantPaymentVM paymentData);
        Task<List<GetConsultantPaymentsInPeriodVM>> GetConsultantPaymentsInPeriod(int consultantId, DateTime startDate,
            DateTime endDate);
        Task<MethodResponse> SetAsAccountPayable(string userIdCreatedBy,
            SetAsAccountPayableVM dataFromModel, decimal accountPayableAmount, GetListOfMovementsForPaymentVM listOfMovementsForPayment,
            string companyId);

        Task<MethodResponse> DeletePayment(int paymentId);
        Task UpdateMovementsStatuses(List<TransactionStatus> transactionStatuses, DateTime startDate, DateTime endDate,
            int consultantId, string newStatus);
        decimal GetConsultantTotalAmountToPay(GetListOfMovementsForPaymentVM? listOfMovements);
        Task<MethodResponse> ApproveAndRejectSubmission(string userIdCreatedBy, ApproveRejectSubmissionVM dataFromUser);
        Task<bool?> AccountPayableIsAccountedAsync(int accountPayableId);
        Task<bool> ExistsPaymentForAccountPayableAsync(int accountPayableId);
    }
}
