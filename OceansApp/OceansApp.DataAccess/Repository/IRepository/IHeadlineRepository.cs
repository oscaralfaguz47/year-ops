using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IHeadlineRepository : IRepository<Headline>
    {
        /// <summary>
        /// Posts a new Headline for its (TeamId, WeekStart). Headlines are additive — each
        /// post is its own row (there may be many per Team/Week, the meeting's news round),
        /// so this never overwrites an existing one. Does not save; the caller commits via
        /// <see cref="IUnitOfWork.SaveAsync"/>.
        /// </summary>
        Task PostAsync(Headline obj);

        /// <summary>
        /// Returns every Team's headlines for the given Week (empty when the Week is blank).
        /// </summary>
        Task<IEnumerable<Headline>> GetForWeekAsync(DateOnly weekStart);

        /// <summary>
        /// Returns the Team's headlines for the given (Team, Week), oldest first
        /// (empty when the Week is blank).
        /// </summary>
        Task<IEnumerable<Headline>> GetForTeamWeekAsync(int teamId, DateOnly weekStart);
    }
}
