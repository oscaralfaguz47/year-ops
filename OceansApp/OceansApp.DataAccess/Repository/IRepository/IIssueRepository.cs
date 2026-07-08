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
        /// Converts a <see cref="Headline"/> into a new pre-filled Issue (additive): the
        /// source headline is left intact in its Week, and the new Issue carries an origin
        /// back-reference to it (see <c>ConversionService.FromHeadline</c>). Throws
        /// <see cref="InvalidOperationException"/> if the headline does not exist. Does not save.
        /// </summary>
        Task<Issue> ConvertHeadlineAsync(int headlineId, DateOnly weekStart, DateTimeOffset at);

        /// <summary>
        /// Sets the Review pin override on an Issue. The pin is a Deferred-only override:
        /// the Issue's state as of <paramref name="weekStart"/> must be
        /// <see cref="IssueStatus.Deferred"/>, otherwise the call is rejected
        /// (<see cref="InvalidOperationException"/>). Does not save.
        /// </summary>
        Task SetPinAsync(int issueId, bool pinned, DateOnly weekStart);

        /// <summary>
        /// Deletes every <see cref="IssueHistory"/> row stamped to <paramref name="weekStart"/>
        /// — the Issue side of removing a past Week from the Meeting History (Administer).
        /// The living Issue records themselves are left intact; their state simply re-derives
        /// from whatever rows remain. Does not save; the caller commits via
        /// <see cref="IUnitOfWork.SaveAsync"/>.
        /// </summary>
        Task DeleteHistoryForWeekAsync(DateOnly weekStart);
    }
}
