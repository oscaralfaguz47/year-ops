namespace OceansApp.Models.ViewModels.PeopleCulture;

public record ResourceItemVm(
    string Slug,
    string Title,
    string IconPath,
    string? TemplatePath = null
);