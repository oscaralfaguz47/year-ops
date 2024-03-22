using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.ConsultantPaymentsDebitsCredits;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantPaymentDebitsCreditsRepository : IRepository<ConsultantPaymentDebitsCredits>
    {
        Task<(List<ConsultantPaymentDebitsCreditsGetAllWithFiltersVM> debitsCredits, int totalCount)> GetAllPaymentsDebitsCreditsWithFiltersAsync(ConsultantPaymentsDebitsCreditsPaginationFiltersVM filtersAndPagination);
    }
}
