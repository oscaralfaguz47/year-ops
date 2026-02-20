using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.PeopleCulture;
using System.Security.Claims;

namespace OceansAppWeb.Areas.Resources.Controllers;

[Area("Resources")]
[Route("Resources/[controller]")]
[Authorize]
public class HubController: Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public HubController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    // === Main Categories ===
    private static readonly List<ResourceItemVm> Policies =
    [
        new("payment-policy", "Payment Policy", "/icons/Resources/money-bill.svg",
            "https://ripplepeopleandculture.blob.core.windows.net/pc-landing/Oceans%20Code%20Payrroll%20Policies-2_compressed.pdf"),
        new("benefits-policy", "Benefits Policy", "/icons/Resources/star.svg",
            "https://ripplepeopleandculture.blob.core.windows.net/pc-landing/Oceans%20Benefits%20and%20Perks%20Policies%20V2.pdf"),
        new("ethics-compliance", "Ethics & Compliance", "/icons/Resources/scale.svg",
            "https://ripplepeopleandculture.blob.core.windows.net/pc-landing/Compliance%20and%20Ethics%20Policy%20for%20Consultants.pdf"),
        new("syntepro-onboarding", "Syntepro Onboarding", "/icons/Resources/building.svg",
             "https://ripplepeopleandculture.blob.core.windows.net/pc-landing/Manual%20de%20inducci%C3%B3n%20Syntepro.pdf")
    ];

    private static readonly List<ResourceItemVm> PersonalDevelopment =
    [
        new("active-listening", "Active Listening", "/icons/Resources/screen.svg",
            "https://ripplepeopleandculture.blob.core.windows.net/pc-landing/Escucha%20Activa.pdf"),
        new("interviewers-training", "Interviewers Training", "/icons/Resources/presentation.svg",
            "https://ripplepeopleandculture.blob.core.windows.net/pc-landing/Interviewers%20Training.pdf"),
        new("oceans-onboarding", "Oceans Onboarding", "/icons/Resources/rocket.svg",
            "https://ripplepeopleandculture.blob.core.windows.net/pc-landing/2025%20Onboarding%20PPT-P&C%20-%20updated.pdf")
    ];

    private static readonly List<ResourceItemVm> Collection =
    [
        new("about-oceans", "About Oceans", "/icons/Resources/magnifying-glass.svg",
            "https://ripplepeopleandculture.blob.core.windows.net/pc-landing/About%20Oceans%20one%20pager%20(1).pdf"),
        new("life-at-oceans", "Life at Oceans", "/icons/Resources/play-teal.svg",
            "https://ripplepeopleandculture.blob.core.windows.net/pc-landing/Video%20Photos.mp4")
    ];

    // === Quick Guides ===
    private static readonly List<QuickGuideVm> QuickGuides =
    [
        new("Bonusly", "Authentication", "/icons/Resources/file.svg", "/icons/Resources/bonusly.webp",
            "https://app.bonus.ly/"),
        new("VTracker", "Submissions", "/icons/Resources/play.svg", "/icons/Resources/vtracker.webp",
            "https://vacationtracker.io/"),
        new("Glassdoor", "Reviews", "/icons/Resources/link.svg", "/icons/Resources/glassdoor.webp",
            "https://www.glassdoor.com/surveys/employer/create?i=5073834&j=true&y=&c=PAGE_INFOSITE_BOTTOM&rt=/Overview/Working-at-Oceans-Code-Experts-EI_IE5073834.11,30.htm"),
        new("Oceans", "Website", "/icons/Resources/link.svg", "/icons/Resources/A.webp", "https://oceanscode.com"),
        new("Ripple", "by Oceans", "/icons/Resources/file.svg", "/img/ripple-logo-simple.webp",
            "https://ripplepeopleandculture.blob.core.windows.net/pc-landing/One%20Pager%20Tracking%20Tool_compressed.pdf"),
        new("Benefit", "Reimbursement", "/icons/Resources/link.svg", "/icons/Resources/refund.svg",
            "https://app.fillout.com/t/9QGFtqwy6yus")
    ];

    // === P&C Team ===
    private static readonly List<TeamMemberVm> Team =
    [
        new(
            "People & Culture Associate",
            "Valeria Mora Guillén",
            "Benefits, activities, voluntary time",
            "valeria.mora@oceanscode.com",
            "+50683776412",
            "https://oceansappfiles.blob.core.windows.net/user-profile-photos/a0b66c0beed8768bb08b60e45cbd089a_455d54d2_Captura de pantalla 2024-11-14 161712.png?sv=2024-08-04&st=2024-11-14T22%3A17%3A26Z&se=2054-11-07T22%3A17%3A26Z&sr=b&sp=rw&sig=QCS2Dse98MjXPb81PugPYFBLw3I%2BixcZ%2FMKo8nOknLg%3D",
            [
                "Onboarding & Consultant Support: Assist with general onboarding, payment onboarding, platform access, candidate documentation, and consultant support.",
                "Internships",
                "Benefits, VTO and Reimbursments",
                "Operational Management: Handle purchases, reimbursements, event and internship logistics, inventory control, and vendor coordination.",
                "Engagement Activities: Manage gifts (birthdays, anniversaries, holidays), internal communications, and Bonusly platform administration.",
                "Administrative Tasks: Maintain developer databases, consultant photos, P&C follow-up summaries, and the activities calendar.",
                "Marketing & Travel Support: Manage quotes, flight bookings, and marketing material requests (photos, images, designs).",
                "Culture & Engagement Projects: Get Togethers, Team Building, Virtual Coffee, Coworking activities."
            ]
        ),
        new(
            "People & Culture Lead",
            "Laura Paniagua Villegas",
            "Benefits, activities, voluntary time",
            "laura.paniagua@oceanscode.com",
            "+50684184923",
            "https://oceansappfiles.blob.core.windows.net/user-profile-photos/268abd1cdad4bbb804b0e5836d655b79_99933cad_99933cad_c4afdebf00a72fe038dcc4b9ab42c0c8eaa185af6c14d852cad25f7ebdb3524e_Captura_de_pantalla_2024-1.png?sv=2024-08-04&st=2024-11-28T16%3A10%3A00Z&se=2054-11-21T16%3A10%3A00Z&sr=b&sp=rw&sig=oK0VGxudNBcfoqoWQP7%2F5vNTuNO0Ruk%2FQF9bxEqUgwg%3D",
            [
                "Contract & Legal Management: Oversee MSA, NDA, CLs, new hires, renewals, vendor contracts, and consultant offboarding letters.",
                "Talent Processes: Manage general onboarding, consultant exits, background checks, HRider, and candidate communications.",
                "Operational Support: Provide support on payments, conflict resolution, contracts, events, benefits, platform access, and admin team needs.",
                "Budget Management: Lead negotiations, manage assets, ensure budget compliance, and own event-related contracts.",
                "Finance & Compliance: Support finance processes (W8/W9, payments, credits/debits), handle Syntepro reports, and ensure organizational compliance.",
                "Engagement & Internal Communications: Coordinate recognition programs, welcome and organizational change emails, social events, and committee follow-ups.",
                "Compliance & Ethics",
                "Culture & Engagement Projects: Get Togethers, Team Building, Virtual Coffee, Coworking activities."
            ]
        )
    ];

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var userInfo = await _unitOfWork.ApplicationUser
            .QueryAsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                CategoryName = u.ApplicationUserCategory.Name
            })
            .FirstOrDefaultAsync(ct);

        if (userInfo is null)
            return NotFound();

        // Filter by Title
        var filteredPolicies = userInfo.CategoryName.Equals("Consultant", StringComparison.OrdinalIgnoreCase)
            ? Policies
                .Where(p => !string.Equals(p.Title, "Syntepro Onboarding", StringComparison.OrdinalIgnoreCase))
                .ToList()
            : Policies.ToList();

        var vm = new HubIndexVm
        {
            Policies = filteredPolicies,
            PersonalDevelopment = PersonalDevelopment,
            Collection = Collection,
            QuickGuides = QuickGuides,
            Team = Team
        };

        return View(vm);
    }
}