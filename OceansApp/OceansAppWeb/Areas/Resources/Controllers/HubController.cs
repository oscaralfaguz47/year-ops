using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using OceansApp.Models.ViewModels.PeopleCulture;

namespace OceansAppWeb.Areas.Resources.Controllers;

[Area("Resources")]
[Route("Resources/[controller]")]
[Authorize]
public class HubController(ICompositeViewEngine views) : Controller
{
    // === Main Categories ===
    private static readonly List<ResourceItemVm> Policies =
    [
        new("payment-policy", "Payment Policy", "/icons/Resources/money-bill.svg",
            "https://ripplepeopleandculture.blob.core.windows.net/pc-landing/Payment%20Policies.pdf"),
        new("benefits-policy", "Benefits Policy", "/icons/Resources/star.svg",
            "https://ripplepeopleandculture.blob.core.windows.net/pc-landing/Benefits%20Policies.pdf"),
        new("ethics-compliance", "Ethics & Compliance", "/icons/Resources/scale.svg",
            "https://ripplepeopleandculture.blob.core.windows.net/pc-landing/Compliance%20and%20Ethics%20Policy%20for%20Consultants.pdf"),
        new("syntepro-onboarding", "Syntepro Onboarding", "/icons/Resources/building.svg",
            "https://ripplepeopleandculture.blob.core.windows.net/pc-landing/Onboarding%20GRUPO%20SYNTEPRO.pdf")
    ];

    private static readonly List<ResourceItemVm> PersonalDevelopment =
    [
        new("active-listening", "Active Listening", "/icons/Resources/screen.svg",
            "https://ripplepeopleandculture.blob.core.windows.net/pc-landing/Escucha%20Activa.pdf"),
        new("interviewers-training", "Interviewers Training", "/icons/Resources/presentation.svg",
            "https://ripplepeopleandculture.blob.core.windows.net/pc-landing/Interviewers%20Training.pdf")
    ];

    private static readonly List<ResourceItemVm> Collection =
    [
        new("about-oceans", "About Oceans", "/icons/Resources/magnifying-glass.svg",
            "https://ripplepeopleandculture.blob.core.windows.net/pc-landing/About%20Oceans.pdf")
    ];

    // === Quick Guides ===
    private static readonly List<QuickGuideVm> QuickGuides =
    [
        new("Bonusly", "Authentication", "/icons/Resources/file.svg", "/icons/Resources/bonusly.webp", "#"),
        new("VTracker", "Submissions", "/icons/Resources/play.svg", "/icons/Resources/vtracker.webp", "#"),
        new("Glassdoor", "Reviews", "/icons/Resources/link.svg", "/icons/Resources/glassdoor.webp", "#"),
        new("Oceans", "Website", "/icons/Resources/link.svg", "/icons/Resources/A.webp", "https://oceanscode.com")
    ];

    // === P&C Team ===
    private static readonly List<TeamMemberVm> Team =
    [
        new(
            "People and Culture Associate",
            "María Valeria Mora Guillén",
            "Benefits, activities, voluntary time, company culture, accesses.",
            "valeria.mora@oceanscode.com",
            "+50683776412",
            "https://oceansappfiles.blob.core.windows.net/user-profile-photos/a0b66c0beed8768bb08b60e45cbd089a_455d54d2_Captura de pantalla 2024-11-14 161712.png?sv=2024-08-04&st=2024-11-14T22%3A17%3A26Z&se=2054-11-07T22%3A17%3A26Z&sr=b&sp=rw&sig=QCS2Dse98MjXPb81PugPYFBLw3I%2BixcZ%2FMKo8nOknLg%3D"
        ),
        new(
            "People and Culture Coordinator",
            "Laura Paniagua",
            "Benefits, activities, voluntary time, company culture, accesses.",
            "laura.paniagua@oceanscode.com",
            "+50684184923",
            "https://oceansappfiles.blob.core.windows.net/user-profile-photos/268abd1cdad4bbb804b0e5836d655b79_99933cad_99933cad_c4afdebf00a72fe038dcc4b9ab42c0c8eaa185af6c14d852cad25f7ebdb3524e_Captura_de_pantalla_2024-1.png?sv=2024-08-04&st=2024-11-28T16%3A10%3A00Z&se=2054-11-21T16%3A10%3A00Z&sr=b&sp=rw&sig=oK0VGxudNBcfoqoWQP7%2F5vNTuNO0Ruk%2FQF9bxEqUgwg%3D"
        )
    ];

    [HttpGet]
    public IActionResult Index()
    {
        var vm = new HubIndexVm
        {
            Policies = Policies,
            PersonalDevelopment = PersonalDevelopment,
            Collection = Collection,
            QuickGuides = QuickGuides,
            Team = Team
        };

        return View(vm);
    }
}