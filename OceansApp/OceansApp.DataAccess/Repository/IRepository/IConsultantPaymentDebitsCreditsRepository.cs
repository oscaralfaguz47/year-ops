using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantPaymentsDebitsCredits;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantPaymentDebitsCreditsRepository : IRepository<ConsultantPaymentDebitsCredits>
    {
        Task<(List<ConsultantPaymentDebitsCreditsGetAllWithFiltersVM> debitsCredits, int totalCount)> GetAllPaymentsDebitsCreditsWithFiltersAsync(ConsultantPaymentsDebitsCreditsPaginationFiltersVM filtersAndPagination);
        Task<MethodResponse> CreateDebitCredit(string userIdCreatedBy,
            CreateUpdateConsultantPaymentDebitCreditVM debitCreditData);
        Task<MethodResponse> UpdateDebitCredit(string userActionedBy,
            CreateUpdateConsultantPaymentDebitCreditVM debitCreditData);
        Task<MethodResponse> RejectDebitCredit(string userActionedBy, int consultantPaymentDebitsCreditsId);
        Task<CreateUpdateConsultantPaymentDebitCreditVM> GetDebitCreditDataById(int consultantPaymentDebitsCreditsId);
    }
}
