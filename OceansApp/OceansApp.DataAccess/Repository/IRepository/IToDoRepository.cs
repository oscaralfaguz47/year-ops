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
    }
}
