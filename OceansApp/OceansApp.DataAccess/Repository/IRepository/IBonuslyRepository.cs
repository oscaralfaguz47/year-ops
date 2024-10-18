
namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IBonuslyRepository
    {
        Task<string?> GetUserByEmailAsync(string email);
        Task<decimal> GetRedeemableBalanceByUserIdAsync(string userId);
    }
}
