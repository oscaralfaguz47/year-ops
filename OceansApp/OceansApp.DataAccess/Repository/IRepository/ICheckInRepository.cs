using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ICheckInRepository : IRepository<CheckIn>
    {
        void Update(CheckIn obj);

        /// <summary>
        /// Inserts the check-in for its (TeamId, WeekStart), or overwrites the existing
        /// row's <see cref="CheckIn.Type"/> and <see cref="CheckIn.Note"/> if one already
        /// exists — guaranteeing exactly one check-in per (Team, Week). Does not save;
        /// the caller commits via <see cref="IUnitOfWork.SaveAsync"/>.
        /// </summary>
        Task UpsertAsync(CheckIn obj);

        /// <summary>
        /// Returns the check-in for the given (Team, Week), or null when the Week is blank.
        /// </summary>
        Task<CheckIn> GetForWeekAsync(int teamId, DateOnly weekStart);
    }
}
