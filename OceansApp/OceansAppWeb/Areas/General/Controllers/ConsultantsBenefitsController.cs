using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;

namespace OceansAppWeb.Areas.General.Controllers
{
    [Area("General")]
    [Authorize]
    [Authorize(Policy = "AccessToManageConsultantsBenefitsPage")]
    [RequireTwoFactorEnabled]
    public class ConsultantsBenefitsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ConsultantsBenefitsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
