using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;

namespace OceansApp.DataAccess.Repository
{
    public class BankAccountRepository : Repository<BankAccount>, IBankAccountRepository
    {
        private ApplicationDbContext _db;
        public BankAccountRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<bool> AddBankAccount(BankAccount obj)
        {
            var existingBankAccount = await GetFirstOrDefaultAsync(u => u.BankAccountCode == obj.BankAccountCode && u.BankAccountName == obj.BankAccountName && u.CompanyId == obj.CompanyId);
            if (existingBankAccount == null)
            {
                await _db.BANK_ACCOUNTS.AddAsync(obj);
                await _db.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<List<GetDataForSelectVM>> GetBankAccountsWherePaymentMethod(int paymentMethodId)
        {
            try
            {
                var result = await (from pmba in _db.PAYMENT_METHOD_AND_BANK_ACCOUNTS
                                    join ba in _db.BANK_ACCOUNTS on pmba.BankAccountId equals ba.BankAccountId
                                    where pmba.PaymentMethodId == paymentMethodId
                                    orderby pmba.IsDefault descending
                                    select new GetDataForSelectVM
                                    {
                                        Text = ba.BankAccountName,
                                        Value = ba.BankAccountId
                                    }).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
