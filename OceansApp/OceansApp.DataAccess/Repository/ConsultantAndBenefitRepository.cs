using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantsAndBenefits;
using System.Text.Json;

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
            var consultantBalance = await _db.CONSULTANTS_AND_BENEFITS
                .Where(cb => cb.ConsultantId == consultantId &&
                             cb.ConsultantBenefit.Name == benefitName)
                .Select(cb => (decimal?)cb.BalanceAmount)
                .FirstOrDefaultAsync();

            if (consultantBalance != null)
                return consultantBalance;

            var defaultAmount = await _db.CONSULTANT_BENEFITS
                .Where(b => b.Name == benefitName)
                .Select(b => (decimal?)b.Amount)
                .FirstOrDefaultAsync();

            return defaultAmount;
        }

        public async Task<(List<GetConsultantsAndBenefitsBalanceAmountVM> results, int totalCount)> GetConsultantsAndBenefitsBalanceAsync(
            ConsultantsAndBenefitsBalancePaginationFiltersVM paginationFilters)
        {
            int pageNumber = paginationFilters.PaginationWithoutFilters.Pagination.PageIndex;
            int pageSize = paginationFilters.PaginationWithoutFilters.Pagination.PageSize;

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 50;

            // Pre-filter benefits to reduce cross-join size
            var benefits = _db.CONSULTANT_BENEFITS.AsQueryable();
            if (paginationFilters.Filters.BenefitId.HasValue)
                benefits = benefits.Where(b => b.BenefitId == paginationFilters.Filters.BenefitId.Value);

            // Pre-filter users by IsActive and search text
            var users = _db.AspNetUsers.AsQueryable();

            if (paginationFilters.Filters.ConsultantIsActive.HasValue)
                users = users.Where(u => u.IsActive == paginationFilters.Filters.ConsultantIsActive.Value);

            if (!string.IsNullOrWhiteSpace(paginationFilters.Filters.SearchText))
            {
                string term = paginationFilters.Filters.SearchText.Trim();

                // Filter by full name (Name + LastName) or partial matches
                users = users.Where(u =>
                    (((u.Name ?? "") + " " + (u.LastName ?? "")).Contains(term)) ||
                    ((u.Name ?? "").Contains(term)) ||
                    ((u.LastName ?? "").Contains(term))
                );
            }

            // Base query: CONSULTANT_DETAILS x BENEFITS (cross join) + left join to CONSULTANTS_AND_BENEFITS
            var query =
                from cd in _db.CONSULTANT_DETAILS
                join u in users on cd.UserId equals u.Id
                from b in benefits                             // Filtered CROSS JOIN
                join cab in _db.CONSULTANTS_AND_BENEFITS
                     on new { cd.ConsultantId, b.BenefitId }
                     equals new { cab.ConsultantId, cab.BenefitId }
                     into cabJoin
                from cab in cabJoin.DefaultIfEmpty()           // LEFT JOIN
                select new
                {
                    ConsultantAndBenefitId = (int?)cab.Id,
                    BenefitId = b.BenefitId,
                    ConsultantName = (u.Name ?? "") + " " + (u.LastName ?? ""),
                    IsActive = u.IsActive,
                    BenefitName = b.Name,
                    AmountBase = b.Amount,
                    BalanceAmount = cab != null ? cab.BalanceAmount : b.Amount
                };

            // Total count before pagination
            var totalCount = await query.CountAsync();

            // Apply ordering and pagination in SQL
            var pageData = await query
                .OrderBy(x => x.ConsultantName)
                .ThenBy(x => x.BenefitName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to ViewModel and propagate total count
            var results = pageData
                .Select(x => new GetConsultantsAndBenefitsBalanceAmountVM
                {
                    ConsultantAndBenefitId = x.ConsultantAndBenefitId,
                    BenefitId = x.BenefitId,
                    ConsultantName = x.ConsultantName,
                    IsActive = x.IsActive,
                    BenefitName = x.BenefitName,
                    AmountBase = x.AmountBase,
                    BalanceAmount = x.BalanceAmount
                })
                .ToList();

            return (results, totalCount);
        }

        public async Task<List<GetConsultantsAndBenefitsBalanceAmountVM>> GetConsultantsWithSavedBenefitsAsync()
        {
            // Load all benefits
            var benefits = _db.CONSULTANT_BENEFITS.AsQueryable();
            var users = _db.AspNetUsers.AsQueryable();

            var results = await (
                from cd in _db.CONSULTANT_DETAILS
                join u in users on cd.UserId equals u.Id

                // CROSS JOIN with all benefits
                from b in benefits

                    // LEFT JOIN with CONSULTANTS_AND_BENEFITS
                join cab in _db.CONSULTANTS_AND_BENEFITS
                     on new { cd.ConsultantId, b.BenefitId }
                     equals new { cab.ConsultantId, cab.BenefitId }
                     into cabJoin
                from cab in cabJoin.DefaultIfEmpty()

                where cab != null

                orderby (u.Name ?? "") + " " + (u.LastName ?? ""), b.Name

                select new GetConsultantsAndBenefitsBalanceAmountVM
                {
                    ConsultantAndBenefitId = cab.Id,
                    BenefitId = b.BenefitId,
                    ConsultantName = (u.Name ?? "") + " " + (u.LastName ?? ""),
                    IsActive = u.IsActive,
                    BenefitName = b.Name,
                    AmountBase = b.Amount,
                    BalanceAmount = cab.BalanceAmount
                }
            ).ToListAsync();

            return results;
        }

        public async Task<MethodResponse> ResetAllConsultantsAndBenefitsBalanceAsync(string userIdCreatedBy, string description, int year)
        {
            var existingReset = await _db.CONSULTANT_BENEFITS_RESETS.FirstOrDefaultAsync(x => x.YearToReset == year);
            if (existingReset != null)
            {
                return MethodResponse.CreateFailureValidationResponse($"A reset has already been implemented for the year {year}. You cannot apply more than 1 to the same year.");
            }
            var existingConsultantsAndBenefits = await GetConsultantsWithSavedBenefitsAsync();

            if (existingConsultantsAndBenefits.Any())
            {
                using (var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
                {
                    try
                    {
                        int count = 0;
                        List<int> consultantAndBenefitIds = new();
                        foreach (var benefitElement in existingConsultantsAndBenefits)
                        {
                            var existingConsultantAndBenefit = await _db.CONSULTANTS_AND_BENEFITS.FirstOrDefaultAsync(x => x.Id == benefitElement.ConsultantAndBenefitId);

                            if (existingConsultantAndBenefit == null)
                                return MethodResponse.CreateFailureNotFoundResponse($"The Consultant and Benefit Id {benefitElement.ConsultantAndBenefitId} was not found.");

                            var benefit = await _db.CONSULTANT_BENEFITS.FirstOrDefaultAsync(x => x.BenefitId == existingConsultantAndBenefit.BenefitId);
                            if (benefit == null)
                                return MethodResponse.CreateFailureNotFoundResponse($"The Benefit Id {existingConsultantAndBenefit.BenefitId} was not found.");

                            if (existingConsultantAndBenefit.BalanceAmount != benefit.Amount)
                            {
                                //Save the history
                                ConsultantAndBenefitHistory historyToCreate = new()
                                {
                                    CreationDate = DateTime.UtcNow,
                                    UserCreatedById = userIdCreatedBy,
                                    ConsultantAndBenefitId = existingConsultantAndBenefit.Id,
                                    OldValue = existingConsultantAndBenefit.BalanceAmount,
                                    NewValue = benefit.Amount,
                                    Notes = $"Renewed annual benefit for ({year})"
                                };
                                await _db.CONSULTANTS_AND_BENEFITS_HISTORY.AddAsync(historyToCreate);

                                //Reset the Balance Amount
                                existingConsultantAndBenefit.BalanceAmount = benefit.Amount;

                                // Add Id to the list of reset records
                                if (benefitElement.ConsultantAndBenefitId.HasValue)
                                {
                                    consultantAndBenefitIds.Add(benefitElement.ConsultantAndBenefitId.Value);
                                }

                                count++;
                            }
                        }
                        // Create the Consultant Benefit Reset record only if something was updated
                        if (count > 0)
                        {
                            // Build the JSON: { "ConsultantAndBenefitIds": [1,3,8,58] }
                            var resetValuesObject = new
                            {
                                ConsultantAndBenefitIds = consultantAndBenefitIds
                            };

                            string arrayResetValuesJson = JsonSerializer.Serialize(resetValuesObject);

                            ConsultantBenefitReset benefitResetToCreate = new()
                            {
                                UserIdCreatedBy = userIdCreatedBy,
                                ResetDate = DateTime.UtcNow,
                                Description = description,
                                ArrayResetValues = arrayResetValuesJson,
                                YearToReset = year
                            };

                            // Asegúrate que estás agregando el reset a su DbSet correcto
                            await _db.CONSULTANT_BENEFITS_RESETS.AddAsync(benefitResetToCreate);
                        }

                        await _db.SaveChangesAsync();

                        await transaction.CommitAsync();
                        return new MethodResponse
                        {
                            Success = true,
                            Message = $"{(count == 0 ? "No updates were executed, all balances are already reset." : count + " registers were reset successfully.")}"
                        };
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
                    }
                }
            }
            else
            {
                return MethodResponse.CreateSuccessResponse("No updates were executed, all balances are already reset.");
            }
        }


    }
}
