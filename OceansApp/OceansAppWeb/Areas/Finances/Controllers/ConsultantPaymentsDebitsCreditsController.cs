using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;

namespace OceansAppWeb.Areas.Finances.Controllers
{
    [Area("Finances")]
    [Authorize]
    [Authorize(Policy = "AccessToManageConsultantPaymentsDebitsAndCredits")]
    [RequireTwoFactorEnabled]
    public class ConsultantPaymentsDebitsCreditsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ConsultantPaymentsDebitsCreditsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
