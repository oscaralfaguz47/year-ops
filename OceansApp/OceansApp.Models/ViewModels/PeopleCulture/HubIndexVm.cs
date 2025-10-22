namespace OceansApp.Models.ViewModels.PeopleCulture;

public class HubIndexVm
{
    public List<ResourceItemVm> Policies { get; set; } = [];
    public List<ResourceItemVm> PersonalDevelopment { get; set; } = [];
    public List<ResourceItemVm> Collection { get; set; } = [];

    public List<QuickGuideVm> QuickGuides { get; set; } = [];

    public List<TeamMemberVm> Team { get; set; } = [];
}