namespace OceansApp.Models.ViewModels.PeopleCulture;

public record TeamMemberVm
{
    public TeamMemberVm(
        string role,
        string name,
        string description,
        string email,
        string phone,
        string photoUrl,
        List<string>? responsibilities = null)
    {
        Role = role;
        Name = name;
        Description = description;
        Email = email;
        Phone = phone;
        PhotoUrl = photoUrl;
        Responsibilities = responsibilities ?? [];
    }

    public string Role { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public string Email { get; init; }
    public string Phone { get; init; }
    public string PhotoUrl { get; init; }
    public List<string> Responsibilities { get; init; } = [];
}