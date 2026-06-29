namespace OceansApp.Models.Models
{
    /// <summary>
    /// What a single <see cref="IssueHistory"/> row records: either a status change
    /// (which moves the Issue through Open -> Deferred -> Solved) or an IDS comment.
    /// </summary>
    public enum IssueChangeType
    {
        Status = 0,
        Comment = 1
    }
}
