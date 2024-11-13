
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IBankAccountRepository : IRepository<BankAccount> 
    {
        Task<bool> AddBankAccount(BankAccount obj);
        Task<List<GetDataForSelectVM>> GetBankAccountsWherePaymentMethod(int paymentMethodId);
    }
}
