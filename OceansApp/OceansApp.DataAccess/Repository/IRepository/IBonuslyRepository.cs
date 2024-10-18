
using OceansApp.Models.ViewModels.Bonusly;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IBonuslyRepository
    {
        Task<string?> GetUserByEmailAsync(string email);
        Task<decimal> GetRedeemableBalanceByUserIdAsync(string userId);
        Task<List<RedemptionsVM>> GetRedemptionsByUserIdAsync(string userId, int limit);
    }
}
