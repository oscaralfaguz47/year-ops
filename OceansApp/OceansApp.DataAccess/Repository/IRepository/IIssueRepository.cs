using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IIssueRepository : IRepository<Issue>
    {
        /// <summary>
        /// Raises a new Issue (Living entity), stamping it with its origin Week and
        /// appending the initial <see cref="IssueStatus.Open"/> status row to its
        /// history. Does not save; the caller commits via
        /// <see cref="IUnitOfWork.SaveAsync"/>.
        /// </summary>
        Task RaiseAsync(Issue issue, DateTimeOffset at);

        /// <summary>
        /// Appends a status-change row for the Issue (Open -> Deferred -> Solved),
        /// week-stamped with <paramref name="weekStart"/>. One row per change — the
        /// Issue's state is derived from the latest such row. Does not save.
        /// </summary>
        Task TransitionAsync(int issueId, IssueStatus status, DateOnly weekStart, DateTimeOffset at);

        /// <summary>
        /// Appends an IDS comment row for the Issue, week-stamped with
        /// <paramref name="weekStart"/>. Does not change the Issue's state. Does not save.
        /// </summary>
        Task CommentAsync(int issueId, string comment, DateOnly weekStart, DateTimeOffset at);

        /// <summary>
        /// Returns the Team's Issues with their full status/comment history loaded,
        /// ready for stateAsOf computation.
        /// </summary>
        Task<IEnumerable<Issue>> GetForTeamAsync(int teamId);

        /// <summary>
        /// Sets the Review pin override on an Issue. The pin is a Deferred-only override:
        /// the Issue's state as of <paramref name="weekStart"/> must be
        /// <see cref="IssueStatus.Deferred"/>, otherwise the call is rejected
        /// (<see cref="InvalidOperationException"/>). Does not save.
        /// </summary>
        Task SetPinAsync(int issueId, bool pinned, DateOnly weekStart);
    }
}
