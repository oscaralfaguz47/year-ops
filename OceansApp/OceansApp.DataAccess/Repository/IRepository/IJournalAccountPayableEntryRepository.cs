
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.JournalAccountsPayable;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IJournalAccountPayableEntryRepository : IRepository<JournalAccountPayableEntry>
    {
        Task<List<JournalAccountPayableEntriesToExportVM>> GetJournalAccountPayableEntries(int journalId);
    }
}
