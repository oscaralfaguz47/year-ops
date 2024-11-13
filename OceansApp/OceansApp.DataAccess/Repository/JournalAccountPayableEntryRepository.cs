using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.JournalAccountsPayable;

namespace OceansApp.DataAccess.Repository
{
    public class JournalAccountPayableEntryRepository : Repository<JournalAccountPayableEntry>, IJournalAccountPayableEntryRepository
    {
        private ApplicationDbContext _db;
        public JournalAccountPayableEntryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<List<JournalAccountPayableEntriesToExportVM>> GetJournalAccountPayableEntries(int journalId)
        {
            var result = await (from je in _db.JOURNAL_ACCOUNTS_PAYABLE_ENTRIES
                                join cc in _db.COST_CENTER on je.CostCenterId equals cc.CostCenterId
                                join aa in _db.ACCOUNTING_ACCOUNT on je.AccountingAccountId equals aa.AccountingAccountId
                                where je.JournalId == journalId
                                select new JournalAccountPayableEntriesToExportVM
                                {
                                    Nit = "ND",
                                    CostCenter = cc.CostCenterCode,
                                    AccountingAccount = aa.AccountingAccountCode,
                                    Source = "AccountPayableId:" + je.AccountPayableId,
                                    Reference = je.Reference,
                                    Debit = je.Debit,
                                    Credit = je.Credit
                                }).ToListAsync();
            return result;
        }

    }
}
