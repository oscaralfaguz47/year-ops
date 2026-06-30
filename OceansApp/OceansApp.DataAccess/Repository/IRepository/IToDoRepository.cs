using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IToDoRepository : IRepository<ToDo>
    {
        /// <summary>
        /// Raises a new To-Do (Living entity), stamping it with its origin Week and
        /// appending the initial <see cref="ToDoStatus.Open"/> status row to its history.
        /// Does not save; the caller commits via <see cref="IUnitOfWork.SaveAsync"/>.
        /// </summary>
        Task RaiseAsync(ToDo toDo, DateTimeOffset at);

        /// <summary>
        /// Appends a status-change row for the To-Do (Open -> Blocked -> Done), week-stamped
        /// with <paramref name="weekStart"/>. One row per change — the To-Do's state is
        /// derived from the latest such row. Does not save.
        /// </summary>
        Task TransitionAsync(int toDoId, ToDoStatus status, DateOnly weekStart, DateTimeOffset at);

        /// <summary>
        /// Appends a comment row for the To-Do, week-stamped with <paramref name="weekStart"/>.
        /// Does not change the To-Do's state. Does not save.
        /// </summary>
        Task CommentAsync(int toDoId, string comment, DateOnly weekStart, DateTimeOffset at);

        /// <summary>
        /// Returns the Team's To-Dos with their full status/comment history loaded, ready for
        /// stateAsOf computation.
        /// </summary>
        Task<IEnumerable<ToDo>> GetForTeamAsync(int teamId);

        /// <summary>
        /// Converts an <see cref="Issue"/> into a new pre-filled To-Do (additive): the source
        /// Issue is left intact (its history untouched), and the new To-Do carries an origin
        /// back-reference to it (see <c>ConversionService.FromIssue</c>). The To-Do's required
        /// <paramref name="ownerId"/> and <paramref name="dueDate"/> are supplied by the caller.
        /// Raised via <see cref="RaiseAsync"/> in <paramref name="weekStart"/>. Throws
        /// <see cref="InvalidOperationException"/> if the Issue does not exist. Does not save.
        /// </summary>
        Task<ToDo> ConvertIssueAsync(int issueId, string ownerId, DateOnly dueDate, DateOnly weekStart, DateTimeOffset at);

        /// <summary>
        /// Deletes every <see cref="ToDoHistory"/> row stamped to <paramref name="weekStart"/>
        /// — the To-Do side of removing a past Week from the Meeting History (Administer).
        /// The living To-Do records themselves are left intact; their state simply re-derives
        /// from whatever rows remain. Does not save; the caller commits via
        /// <see cref="IUnitOfWork.SaveAsync"/>.
        /// </summary>
        Task DeleteHistoryForWeekAsync(DateOnly weekStart);
    }
}
