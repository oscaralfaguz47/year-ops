using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.JournalAccountsPayable;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IJournalAccountPayableRepository : IRepository<JournalAccountPayable>
    {
        Task<(List<JournalAccountsPayableGetAllWithFiltersVM> journalAccountsPayable, int totalCount)> GetAllJournalAccountsPayableWithFiltersAsync(JournalAccountsPayablePaginationFiltersVM filtersAndPagination);
    }
}
