using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;

namespace OceansAppWeb.Areas.AdminCenter.Controllers
{
    [Area("AdminCenter")]
    [EnableCors("AllowSpecificOrigin")]
    [RequireTwoFactorEnabled]
    public class ConsultantBenefitsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ConsultantBenefitsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }

        [Authorize(Policy = "AccessToAllConsultantBenefitsListForSelect")]
        [HttpGet]
        public async Task<IActionResult> GetAllBenefitsListForSelect()
        {
            try
            {
                List<GetDataForSelectVM> benefitsList = new();
                var benefits = _unitOfWork.ConsultantBenefit.GetAll();
                foreach (var benefit in benefits)
                {
                    benefitsList.Add(new GetDataForSelectVM { Value = benefit.BenefitId, Text = benefit.Name });
                }
                return Ok(new
                {
                    Benefits = benefitsList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }
    }
}
