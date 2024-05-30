using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;

namespace OceansAppWeb.Areas.AdminCenter.Controllers
{
    [ApiController]
    [Route("AdminCenter/[controller]")]
    [Area("AdminCenter")]
    [EnableCors("AllowSpecificOrigin")]
    [RequireTwoFactorEnabled]
    [Authorize]
    public class ConsultantBenefitsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ConsultantBenefitsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }

        [Authorize(Policy = "AccessToAllConsultantBenefitsListForSelect")]
        [HttpGet("GetAllBenefitsListForSelect")]
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
        
        [Authorize(Policy = "AccessToAllConsultantBenefitsListForSelect")]
        [HttpGet("GetAllBenefitCategoriesListForSelect")]
        public IActionResult GetAllBenefitCategoriesListForSelect(int benefitId)
        {
            try
            {
                List<GetDataForSelectVM> benefitCategoriesList = new();
                var categories = _unitOfWork.ConsultantBenefitCategory.GetAll().Where(x => x.BenefitId == benefitId);
                foreach (var category in categories)
                {
                    benefitCategoriesList.Add(new GetDataForSelectVM { Value = category.BenefitCategoryId, Text = category.Name });
                }
                return Ok(new
                {
                    BenefitCategories = benefitCategoriesList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }
    }
}
