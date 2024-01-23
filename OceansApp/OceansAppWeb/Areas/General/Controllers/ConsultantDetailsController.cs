using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;

namespace OceansAppWeb.Areas.General.Controllers
{
    [Area("General")]
    [RequireTwoFactorEnabled]
    public class ConsultantDetailsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ConsultantDetailsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "AccessToClientsPage")]
        [HttpGet]
        public async Task<IActionResult> GetSuccessManagers()
        {
            try
            {
                var users = await _unitOfWork.ConsultantDetail.GetUsersByCategoryAndPositionForSelect("Administrative", "Success Manager");
                return Ok(new
                {
                    SuccessManagers = users
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

    }
}
