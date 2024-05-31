using OceansApp.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using OceansApp.Models.Models;
using OceansApp.DataAccess.Data;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.AccountingAccounts;
using System.Linq;

namespace OceansApp.DataAccess.Repository
{
    public class AccountingAccountRepository : Repository<AccountingAccount>, IAccountingAccountRepository
    {
        private ApplicationDbContext _db;
        public AccountingAccountRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        IEnumerable<AccountingAccount> IAccountingAccountRepository.GetCostOfSalesAccountingAccounts()
        {
            IEnumerable<AccountingAccount>? accountingAccountsList = _db.ACCOUNTING_ACCOUNT
                .FromSqlRaw($"SELECT * FROM ACCOUNTING_ACCOUNT WHERE AccountingAccountCode LIKE '5%'").ToList();
            return accountingAccountsList;
        }
        IEnumerable<AccountingAccount> IAccountingAccountRepository.GetExpensesAccountingAccounts()
        {
            IEnumerable<AccountingAccount>? accountingAccountsList = _db.ACCOUNTING_ACCOUNT
                .FromSqlRaw($"SELECT * FROM ACCOUNTING_ACCOUNT WHERE AccountingAccountCode LIKE '6%'").ToList();
            return accountingAccountsList;
        }
        public IEnumerable<AccountingAccount> GetReturnsAndDiscountsAccountingAccounts()
        {
            IEnumerable<AccountingAccount>? accountingAccountsList = _db.ACCOUNTING_ACCOUNT
               .FromSqlRaw($"SELECT * FROM ACCOUNTING_ACCOUNT WHERE AccountingAccountCode LIKE '4-02-01%' OR AccountingAccountCode LIKE '4-03-01%'").ToList();
            return accountingAccountsList;
        }

        public async Task<bool> UpdateIfExistAddIfNot(AccountingAccount obj)
        {
            var existingAccountingAccount = await GetFirstOrDefaultAsync(u => u.AccountingAccountCode == obj.AccountingAccountCode && u.CompanyId == obj.CompanyId);

            if (existingAccountingAccount == null)
            {
               await _db.ACCOUNTING_ACCOUNT.AddAsync(obj);
               await _db.SaveChangesAsync();
                return true;
            }
            else
            {
                if (existingAccountingAccount.DateLastUpdate != obj.DateLastUpdate)
                {
                    existingAccountingAccount.Description = obj.Description;
                    existingAccountingAccount.DescriptionIFRS = obj.DescriptionIFRS;
                    existingAccountingAccount.AccountingAccountType = obj.AccountingAccountType;
                    existingAccountingAccount.DetailedType = obj.DetailedType;
                    existingAccountingAccount.Balance = obj.Balance;
                    existingAccountingAccount.AcceptData = obj.AcceptData;
                    existingAccountingAccount.UseCostCenter = obj.UseCostCenter;
                    existingAccountingAccount.UseThird = obj.UseThird;
                    existingAccountingAccount.DateLastUpdate = obj.DateLastUpdate;
                    return true;
                }
                return false;
            }
        }
        public void Update(AccountingAccount obj)
        {
            _db.ACCOUNTING_ACCOUNT.Update(obj);
        }

        public async Task<DateTime> GetLatestUpdateDate()
        {
            var latestDate = await _db.ACCOUNTING_ACCOUNT.OrderByDescending(x => x.DateLastUpdate).FirstOrDefaultAsync();
            if (latestDate == null)
            {
                return DateTime.Now;
            }
            else
            {
                return latestDate.DateLastUpdate;
            }

        }
        public async Task<List<GetAccountingAccountsForListVM>> GetAccountingAccountsWhereCostCenterIdAsync(int costCenterId)
        {
            var results = await _db.COSTS_CENTERS_ACCOUNTING_ACCOUNTS.Where(x => x.CostCenterId == costCenterId)
                .Join(_db.ACCOUNTING_ACCOUNT, 
                ccaa => ccaa.AccountingAccountId, aa => aa.AccountingAccountId, (ccaa, aa)=> new
                {
                    CCAA = ccaa,
                    AA = aa
                }).OrderBy(x => x.AA.AccountingAccountCode).ToListAsync();
            var listToReturn = new List<GetAccountingAccountsForListVM>();
            foreach (var accountingAccount in results)
            {
                var selectVM = new GetAccountingAccountsForListVM
                {
                    AccountingAccountId = accountingAccount.CCAA.AccountingAccountId,
                    Description = accountingAccount.AA.Description,
                    AccountingAccountCode = accountingAccount.AA.AccountingAccountCode,
                    AcceptData = accountingAccount.AA.AcceptData
                };
                listToReturn.Add(selectVM);
            }
            return listToReturn;
        }
    }
}
