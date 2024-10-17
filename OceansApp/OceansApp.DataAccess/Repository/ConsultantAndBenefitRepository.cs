using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantAndBenefitRepository : Repository<ConsultantAndBenefit>, IConsultantAndBenefitRepository
    {
        private ApplicationDbContext _db;
        public ConsultantAndBenefitRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<ConsultantAndBenefit> CreateConsultantAndBenefitIfNotExists(int consultantId, ConsultantBenefit benefit)
        {
            ConsultantAndBenefit existingElement = await _db.CONSULTANTS_AND_BENEFITS.FirstOrDefaultAsync(x => x.ConsultantId == consultantId
            && x.BenefitId == benefit.BenefitId);

            if (existingElement == null)
            {
                ConsultantAndBenefit elToCreate = new()
                {
                    ConsultantId = consultantId,
                    BenefitId = benefit.BenefitId,
                    BalanceAmount = benefit.Amount
                };
                await _db.CONSULTANTS_AND_BENEFITS.AddAsync(elToCreate);
                await _db.SaveChangesAsync();
                existingElement = elToCreate;
            }

            return existingElement;
        }

        public async Task<decimal?> GetBenefitBalanceAmountByConsultantAsync(int consultantId, string benefitName)
        {
            decimal? balanceAmount = await _db.CONSULTANTS_AND_BENEFITS
    .Where(cb => cb.ConsultantId == consultantId && cb.ConsultantBenefit.Name == "Balance Program")
    .Select(cb => (decimal?)cb.BalanceAmount) // Cast to nullable to allow null-coalescing
    .FirstOrDefaultAsync()
    ?? _db.CONSULTANT_BENEFITS
    .Where(b => b.Name == benefitName)
    .Select(b => (decimal?)b.Amount)
    .FirstOrDefault();

            return balanceAmount;
        }
    }
}
