using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.ReportingMyTime;
using OceansApp.Utility.SharedMethods.InputValidations;
using System.Security.Claims;

namespace OceansAppWeb.Areas.TrackingTool.Controllers
{
    [Area("TrackingTool")]
    [Authorize]
    [Authorize(Policy = "BasicAccessToReportingMyTime")]
    [RequireTwoFactorEnabled]
    public class ReportingMyTimeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ReportingMyTimeController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> CreateUpdateTimeEntryClientNoTrackingTool([FromBody] CreateUpdateMovementClientNoTrackingToolVM reportMovementData)
        //{
        //    try
        //    {
        //        if (reportMovementData == null)
        //        {
        //            return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
        //        }
        //        ValidateInputs validateInputs = new();

        //        validateInputs.ValidateNonRequiredFieldIntType("MovementId", "MovementId", reportMovementData.MovementId, ModelState);
        //        validateInputs.ValidateRequiredFieldIntType("ProjectId", "Project", reportMovementData.ProjectId, ModelState);
        //        validateInputs.ValidateRequiredFieldNumberValue("Quantity", "Quantity", reportMovementData.Quantity, ModelState);
        //        validateInputs.ValidateNoNegativeNumber("Quantity", "Quantity", reportMovementData.Quantity, ModelState);
        //        validateInputs.ValidateNumberLessOrEqualThanZero("Quantity", "Quantity", reportMovementData.Quantity, ModelState);
        //        validateInputs.ValidateDateValidFormat("ActionDate", "Action Date", reportMovementData.ActionDate, ModelState);
        //        validateInputs.ValidateRequiredFieldAnyValue("ActionDate", "Action Date", reportMovementData.ActionDate, ModelState);
        //        validateInputs.ValidateNotRequiredAndStringLength("Notes", "Notes", reportMovementData.Notes, 200, ModelState);

        //        if (ModelState.IsValid)
        //        {
        //            var claimsIdentity = (ClaimsIdentity)User.Identity;
        //            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        //            var timeZone = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, _config["Config:TimeZone"]);
        //            var resultMessage = "";
        //            var userActionedBy = claim.Value;

        //            //IF IS NOT ID THEN CREATE IT
        //            if (reportMovementData.InterviewId == null)
        //            {
        //                var res = await _unitOfWork.Interview.CreateInterview(userActionedBy, timeZone, reportMovementData);

        //                if (res.Success)
        //                {
        //                    resultMessage = res.Message;
        //                }
        //                else
        //                {
        //                    if (res.MessageType != "Validation Error")
        //                    {
        //                        return BadRequest(new { MessageType = res.MessageType, error = res.Message, result = "ErrorSaving", detail = $"The interview could not be saved." });
        //                    }
        //                    else
        //                    {
        //                        return BadRequest(new
        //                        {
        //                            MessageType = res.MessageType,
        //                            errors = new[] { res.Message }
        //                        });
        //                    }

        //                }
        //            }
        //            else
        //            {
        //                //IF IS ID THEN UPDATE THE DEBIT/CREDIT
        //                var res = await _unitOfWork.Interview.UpdateInterview(userActionedBy, timeZone, reportMovementData);
        //                if (res.Success)
        //                {
        //                    resultMessage = res.Message;
        //                }
        //                else
        //                {
        //                    if (res.MessageType != "Validation Error")
        //                    {
        //                        return BadRequest(new { error = res.Message, MessageType = res.MessageType, result = "ErrorSaving", detail = "The interview could not be updated." });
        //                    }
        //                    else
        //                    {
        //                        return BadRequest(new
        //                        {
        //                            MessageType = res.MessageType,
        //                            errors = new[] { res.Message }
        //                        });
        //                    }

        //                }
        //            }
        //            return Ok(new
        //            {
        //                success = true,
        //                message = resultMessage
        //            });
        //        }
        //        else
        //        {
        //            var errors = ModelState.Values.SelectMany(v => v.Errors)
        //                                          .Select(e => e.ErrorMessage)
        //                                          .ToList();
        //            return BadRequest(new { MessageType = "Validation Error", message = "Validation Error", result = "error", errors = errors });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { MessageType = "Exception Error", error = $"There was an error saving the changes. More details: " + ex.Message, detail = ex.Message });
        //    }
        //}
    }
}
