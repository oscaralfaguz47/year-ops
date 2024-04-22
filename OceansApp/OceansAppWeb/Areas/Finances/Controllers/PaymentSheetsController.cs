using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;

namespace OceansAppWeb.Areas.Finances.Controllers
{
    [Area("Finances")]
    [Authorize]
    [Authorize(Policy = "AccessToManageTheBasicsOfPaymentSheets")]
    [RequireTwoFactorEnabled]
    public class PaymentSheetsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public PaymentSheetsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
