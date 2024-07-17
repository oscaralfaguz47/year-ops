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
    public class TransactionStatusesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public TransactionStatusesController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }

        [Authorize(Policy = "AccessToAllTransactionStatusesList")]
        [HttpGet("GetAllTransactionStatusesList")]
        public async Task<IActionResult> GetAllTransactionStatusesList()
        {
            try
            {
                List<GetDataForSelectVM> transactionStatusesList = new();
                var statuses = await _unitOfWork.TransactionStatus.GetAllAsync(
                    orderBy: q => q.OrderBy(x => x.Name)
                    );
                foreach (var status in statuses)
                {
                    transactionStatusesList.Add(new GetDataForSelectVM { Value = status.TransactionStatusId, Text = status.Name });
                }
                return Ok(new
                {
                    Statuses = transactionStatusesList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }
    }
}
