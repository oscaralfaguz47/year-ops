using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Utility.SharedMethods.InputValidations;
using System.Security.Claims;
using System.Threading;

namespace OceansAppWeb.Areas.TrackingTool.Controllers
{
    [ApiController]
    [Route("TrackingTool/[controller]")]
    [Area("TrackingTool")]
    [Authorize]
    [Authorize(Policy = "AccessToGeneralReports")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    public class GeneralReportsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public GeneralReportsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetOptionsForFilters")]
        public async Task<IActionResult> GetOptionsForFilters()
        {
            try
            {
                var clientsTask = await _unitOfWork.Client.GetAllClientsWithActiveInactiveAsync();
                var projectsTask = await _unitOfWork.Project.GetAllProjectsWithActiveInactiveAsync();
                var consultantsTask = await _unitOfWork.ConsultantDetail.GetAllConsultantsWithActiveInactiveAsync();

                var data = new
                {
                    clients = clientsTask,
                    projects = projectsTask,
                    consultants = consultantsTask
                };

                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return BadRequest(new
                {
                    error = "There was an error fetching the filter options.",
                    success = false,
                    detail = ex.Message
                });
            }
        }

        [HttpGet("GetGeneralReport")]
        public async Task<IActionResult> GetGeneralReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int? movementType,
            [FromQuery] IEnumerable<int>? clients,
            [FromQuery] IEnumerable<int>? projects, [FromQuery] IEnumerable<int>? consultants )
        {
            try
            {
                ValidateInputs validateInputs = new();
                //Validate Filter inputs
                validateInputs.ValidateDateValidFormat("StartDate", "Start Date", startDate, ModelState);
                validateInputs.ValidateDateValidFormat("EndDate", "End Date", endDate, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("StartDate", "Start Date", startDate, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("EndDate", "End Date", endDate, ModelState);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();
                    return BadRequest(new { MessageType = "Validation Error", message = "Validation Error", result = "error", errors = errors });
                }
                

                var totalResults = await _unitOfWork.ReportingMyTimeMovement.GetGlobalMovementsWithFiltersAsync((DateTime)startDate, (DateTime)endDate, movementType,
                    projects, clients, consultants);

                var data = new { movementsList = totalResults };
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"There was an error fetching project movements.", success = false, detail = ex.Message });
            }
        }


    }
}
