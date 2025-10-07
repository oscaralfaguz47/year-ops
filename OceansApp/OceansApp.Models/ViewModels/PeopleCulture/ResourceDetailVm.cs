namespace OceansApp.Models.ViewModels.PeopleCulture;

public class ResourceDetailVm
{
    public string Category { get; init; } = string.Empty;

    public ResourceItemVm Item { get; init; } = null!;

    public List<ResourceItemVm> Peers { get; init; } = [];

    public string? TemplatePath { get; init; }

    public bool TemplateExists { get; set; }
}