namespace OceansApp.Models.ViewModels.PeopleCulture;

public record TeamMemberVm(
    string Role,
    string Name,
    string Description,
    string Email,
    string Phone,
    string PhotoUrl
);