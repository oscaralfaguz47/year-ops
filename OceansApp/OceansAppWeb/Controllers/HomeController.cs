using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Bonusly;
using OceansApp.Models.ViewModels.Dashboard;
using OceansApp.Utility.ConstantData;
using OceansApp.Utility.SharedMethods;
using System.Security.Claims;
using static System.Collections.Specialized.BitVector32;

namespace OceansAppWeb.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    public class HomeController : Controller

    {
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard/Dashboard");
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userConsultant = await _unitOfWork.ApplicationUser.GetUserAndConsultantAsync(userId);

            (int Years, int Months, int Days)? activeTime = userConsultant.UserCategoryName == "External User" ? null : await _unitOfWork.ApplicationUser.GetUserActiveTimeAsync(userId);

            var widgets = _unitOfWork.ApplicationUser.GetWidgetsForUser(userConsultant, User, activeTime);

            var viewModel = new DashboardVM
            {
                Welcome = new WelcomeVM
                {
                    ConsultantName = userConsultant.Name,
                    ActiveTime = activeTime == null ? null : new ActiveTimeVM
                    {
                        Years = activeTime.Value.Years,
                        Months = activeTime.Value.Months,
                        Days = activeTime.Value.Days
                    }
                },
                Widgets = widgets
            };
            return View("Dashboard/Dashboard", viewModel);
        }

        [HttpGet("GetPendingTimesheets")]
        public async Task<IActionResult> GetPendingTimesheets()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var userConsultant = await _unitOfWork.ApplicationUser.GetUserAndConsultantAsync(userId);

                DateTime endDate = DateAndTimes.GetEndOfPeriod(DateTime.UtcNow, userConsultant.PaymentPeriod);

                var pendingProjectsToSubmit = await _unitOfWork.ProjectConsultantPendingSubmission
                    .GetPendingProjectsPendingSubmissionByConsultantAsync(userConsultant.ConsultantId, endDate);

                return Ok(new
                {
                    pendingTimesheets = pendingProjectsToSubmit
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue." });
            }
        }

        [HttpGet("GetLastTimesheetsSubmitted")]
        public async Task<IActionResult> GetLastTimesheetsSubmitted()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var userConsultant = await _unitOfWork.ApplicationUser.GetUserAndConsultantAsync(userId);

                var lastTimeSheetSubmitted = await _unitOfWork.ReportingMyTimeMovementSubmission
                    .GetLastTimesheetSubmittedAsync(userConsultant.ConsultantId);

                return Ok(new
                {
                    lastTimesheetSubmitted = lastTimeSheetSubmitted
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue." });
            }
        }

        [HttpGet("GetBalanceProgramInfo")]
        public async Task<IActionResult> GetBalanceProgramInfo()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var userConsultant = await _unitOfWork.ApplicationUser.GetUserAndConsultantAsync(userId);

                var activeTime = await _unitOfWork.ApplicationUser.GetUserActiveTimeAsync(userId);

                var widgets = _unitOfWork.ApplicationUser.GetWidgetsForUser(userConsultant, User, activeTime);

                //Validate valid access
                if (!widgets.Any(widget => widget.WidgetName == WidgetsCD.Perks &&
                           widget.Sections != null &&
                           widget.Sections.Contains(BenefitsCD.Balance_Program)))
                {
                    var errorResponse = new
                    {
                        error = "Forbidden",
                        message = "You are not allowed to access the GetBalanceProgramInfo method."
                    };
                    return new ObjectResult(errorResponse)
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }

                var balanceAmount = await _unitOfWork.ConsultantAndBenefit
                    .GetBenefitBalanceAmountByConsultantAsync(userConsultant.ConsultantId, "Balance Program");

                var lastRequests = await _unitOfWork.ConsultantReimbursedBenefit
                    .GetLastBenefitRequests(userConsultant.ConsultantId, "Balance Program");

                BenefitBalanceInfoVM balanceProgramInfo = new()
                {
                    BalanceAmount = (decimal)balanceAmount,
                    LastRequests = lastRequests
                };

                return Ok(new
                {
                    balanceProgramInfo = balanceProgramInfo
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue." });
            }
        }

        [HttpGet("GetBonuslyInfo")]
        public async Task<IActionResult> GetBonuslyInfo()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var userConsultant = await _unitOfWork.ApplicationUser.GetUserAndConsultantAsync(userId);

                var activeTime = await _unitOfWork.ApplicationUser.GetUserActiveTimeAsync(userId);

                var widgets = _unitOfWork.ApplicationUser.GetWidgetsForUser(userConsultant, User, activeTime);

                //Validate valid access
                if (!widgets.Any(widget => widget.WidgetName == WidgetsCD.Perks &&
                           widget.Sections != null &&
                           widget.Sections.Contains(BenefitsCD.Bonusly)))
                {
                    var errorResponse = new
                    {
                        error = "Forbidden",
                        message = "You are not allowed to access the GetBonuslyInfo method."
                    };
                    return new ObjectResult(errorResponse)
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
                decimal balanceAmount = 0;
                List<RedemptionsVM> lastRequests = new();
                try
                {
                    string? bonuslyUserId = await _unitOfWork.Bonusly.GetUserByEmailAsync(userConsultant.Email);
                    if (bonuslyUserId == null)
                    {
                        return Ok(new { BonuslyUserExists = false });
                    }
                    balanceAmount = await _unitOfWork.Bonusly
                        .GetRedeemableBalanceByUserIdAsync(bonuslyUserId);

                    lastRequests = await _unitOfWork.Bonusly.GetRedemptionsByUserIdAsync(bonuslyUserId, 2);
                }
                catch (Exception ex)
                {
                    return Ok(new { BonuslyIsDown = true });
                }

                BonuslyBalanceInfoVM bonuslyInfo = new()
                {
                    BalanceAmount = balanceAmount,
                    LastRequests = lastRequests
                };

                return Ok(new
                {
                    BonuslyInfo = bonuslyInfo,
                    BonuslyUserExists = true
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue." });
            }
        }

        [HttpGet("GetOceansChallengeInfo")]
        public async Task<IActionResult> GetOceansChallengeInfo()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var userConsultant = await _unitOfWork.ApplicationUser.GetUserAndConsultantAsync(userId);

                var activeTime = await _unitOfWork.ApplicationUser.GetUserActiveTimeAsync(userId);

                var widgets = _unitOfWork.ApplicationUser.GetWidgetsForUser(userConsultant, User, activeTime);

                //Validate valid access
                if (!widgets.Any(widget => widget.WidgetName == WidgetsCD.Perks &&
                           widget.Sections != null &&
                           widget.Sections.Contains(BenefitsCD.Oceans_Challenge)))
                {
                    var errorResponse = new
                    {
                        error = "Forbidden",
                        message = "You are not allowed to access the GetOceansChallengeInfo method."
                    };
                    return new ObjectResult(errorResponse)
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }

                var balanceAmount = await _unitOfWork.ConsultantAndBenefit
                    .GetBenefitBalanceAmountByConsultantAsync(userConsultant.ConsultantId, "Oceans Challenge");

                var lastRequests = await _unitOfWork.ConsultantReimbursedBenefit
                    .GetLastBenefitRequests(userConsultant.ConsultantId, "Oceans Challenge");

                BenefitBalanceInfoVM balanceProgramInfo = new()
                {
                    BalanceAmount = (decimal)balanceAmount,
                    LastRequests = lastRequests
                };

                return Ok(new
                {
                    oceansChallengeInfo = balanceProgramInfo
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue." });
            }
        }
        public IActionResult Error()
        {
            return View("Error");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
