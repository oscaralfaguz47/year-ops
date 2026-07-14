namespace OceansApp.Models.Models
{
    /// <summary>
    /// What a single <see cref="ToDoHistory"/> row records: either a status change
    /// (which moves the ToDo through Open -> Blocked -> Done) or a comment. Mirrors
    /// <see cref="IssueChangeType"/>.
    /// </summary>
    public enum ToDoChangeType
    {
        Status = 0,
        Comment = 1
    }
}
