using OceansApp.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using OceansApp.Models.Models;
using OceansApp.DataAccess.Data;

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
                .FromSqlRaw($"SELECT * FROM ACCOUNTING_ACCOUNT WHERE IdAccountingAccount LIKE '5%'").ToList();
            return accountingAccountsList;
        }
        IEnumerable<AccountingAccount> IAccountingAccountRepository.GetExpensesAccountingAccounts()
        {
            IEnumerable<AccountingAccount>? accountingAccountsList = _db.ACCOUNTING_ACCOUNT
                .FromSqlRaw($"SELECT * FROM ACCOUNTING_ACCOUNT WHERE IdAccountingAccount LIKE '6%'").ToList();
            return accountingAccountsList;
        }
        public IEnumerable<AccountingAccount> GetReturnsAndDiscountsAccountingAccounts()
        {
            IEnumerable<AccountingAccount>? accountingAccountsList = _db.ACCOUNTING_ACCOUNT
               .FromSqlRaw($"SELECT * FROM ACCOUNTING_ACCOUNT WHERE IdAccountingAccount LIKE '4-02-01%' OR IdAccountingAccount LIKE '4-03-01%'").ToList();
            return accountingAccountsList;
        }

        public bool UpdateIfExistAddIfNot(AccountingAccount obj)
        {
            var existingAccountingAccount = GetFirstOrDefault(u => u.AccountingAccountCode == obj.AccountingAccountCode && u.CompanyId == obj.CompanyId);

            if (existingAccountingAccount == null)
            {
                _db.ACCOUNTING_ACCOUNT.Add(obj);
                _db.SaveChanges();
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

        public DateTime GetLatestUpdateDate()
        {
            var latestDate = _db.ACCOUNTING_ACCOUNT.OrderByDescending(x => x.DateLastUpdate).FirstOrDefault();
            if (latestDate == null)
            {
                return DateTime.Now;
            }
            else
            {
                return latestDate.DateLastUpdate;
            }

        }
    }
}
