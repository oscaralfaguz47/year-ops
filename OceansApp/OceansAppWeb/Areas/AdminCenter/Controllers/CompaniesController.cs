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
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    [Authorize]
    public class CompaniesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompaniesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize(Policy = "AccessToAllCompaniesList")]
        [HttpGet("GetAllCompaniesListForSelect")]
        public async Task<IActionResult> GetAllCompaniesListForSelect()
        {
            try
            {
                List<SelectVM> companiesList = new();
                var companies = await _unitOfWork.Company.GetAllAsync();
                foreach (var company in companies)
                {
                    companiesList.Add(new SelectVM { Value = company.CompanyId, Text = company.Name });
                }
                return Ok(new
                {
                    Companies = companiesList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }
        [Authorize(Policy = "AccessToAllCompaniesList")]
        [HttpGet("GetCompanyIdByClient")]
        public async Task<IActionResult> GetCompanyIdByClient(int clientId)
        {
            try
            {
                var client = await _unitOfWork.Client.GetFirstOrDefaultAsync(x => x.ClientId == clientId);

                if (client == null)
                {
                    return BadRequest(new { error = "The Client was not found."});
                }
                return Ok(new
                {
                    CompanyId = client.CompanyId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

    }
}
