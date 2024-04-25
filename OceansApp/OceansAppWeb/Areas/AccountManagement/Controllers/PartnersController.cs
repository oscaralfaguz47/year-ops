using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;

namespace OceansAppWeb.Areas.AccountManagement.Controllers
{
    [Area("AccountManagement")]
    [RequireTwoFactorEnabled]
    [Authorize]
    public class PartnersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public PartnersController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }

        [Authorize(Policy = "AccessToListOfPartners")]
        [HttpGet]
        public async Task<IActionResult> GetAllPartnersListForSelect()
        {
            try
            {
                List<GetDataForSelectVM> partnersList = new();
                var partners = await _unitOfWork.Partner.GetAllAsync().ConfigureAwait(false);
                foreach (var partner in partners)
                {
                    partnersList.Add(new GetDataForSelectVM { Value = partner.PartnerId, Text = partner.Name });
                }
                return Ok(new
                {
                    Partners = partnersList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }
    }
}
