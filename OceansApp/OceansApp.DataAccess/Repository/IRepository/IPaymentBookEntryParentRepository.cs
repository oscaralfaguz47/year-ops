using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.PaymentBookEntries;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IPaymentBookEntryParentRepository : IRepository<PaymentBookEntryParent> 
    {
        Task<(List<BookEntriesGetAllWithFiltersVM> bookEntries, int totalCount)> GetAllBookEntriesWithFiltersAsync(BookEntriesPaginationFiltersVM filtersAndPagination);
    }
}
