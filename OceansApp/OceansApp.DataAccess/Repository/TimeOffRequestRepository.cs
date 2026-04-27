using Azure.Storage.Queues;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.TimeOff;
using OceansApp.Utility.NotificationTemplates;
using OceansApp.Utility.SharedMethods;

namespace OceansApp.DataAccess.Repository
{
    public class TimeOffRequestRepository : Repository<TimeOffRequest>, ITimeOffRequestRepository
    {
        private ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly Lazy<QueueClient> _queueClient;

        public TimeOffRequestRepository(ApplicationDbContext db, IConfiguration config,
            Lazy<QueueClient> queueClient) : base(db)
        {
            _db = db;
            _config = config;
            _queueClient = queueClient;
        }

        public async Task<TimeOffBalancesVM> GetBalancesAsync(int consultantId)
        {
            var consultant = await _db.CONSULTANT_DETAILS
                .FirstOrDefaultAsync(c => c.ConsultantId == consultantId);

            var result = new TimeOffBalancesVM
            {
                IsPtoEnabled = consultant.IsEligibleForPaidTimeOff,
                PtoAnnualAllowance = consultant.AnnualPaidTimeOffDays ?? 0
            };

            int currentYear = DateTime.UtcNow.Year;

            if (consultant.IsEligibleForPaidTimeOff && consultant.AnnualPaidTimeOffDays.HasValue)
            {
                int annualDays = consultant.AnnualPaidTimeOffDays.Value;
                int ptoAllowance = annualDays;
                int ptoUsedAndPending = await GetUsedDaysAsync(consultantId, "PTO", currentYear);
                result.PtoAvailable = ptoAllowance - ptoUsedAndPending;
            }

            int vtoUsedAndPending = await GetUsedDaysAsync(consultantId, "VTO", currentYear);
            result.VtoAvailable = 1 - vtoUsedAndPending;

            return result;
        }

        public async Task<List<TimeOffCalendarEntryVM>> GetCalendarEntriesAsync(
            int consultantId, DateTime monthStart, DateTime monthEnd)
        {
            return await _db.TIME_OFF_REQUESTS
                .Include(r => r.TransactionStatus)
                .Where(r => r.ConsultantId == consultantId
                    && r.EndDate >= monthStart && r.StartDate <= monthEnd
                    && r.TransactionStatus.Name != "Rejected")
                .Select(r => new TimeOffCalendarEntryVM
                {
                    TimeOffRequestId = r.TimeOffRequestId,
                    TimeOffType = r.TimeOffType,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    Status = r.TransactionStatus.Name
                })
                .ToListAsync();
        }

        public async Task<(List<TimeOffRequestListVM> requests, int totalCount)> GetConsultantRequestsAsync(
            int consultantId, TimeOffPaginationFiltersVM filtersAndPagination)
        {
            var query = _db.TIME_OFF_REQUESTS
                .Include(r => r.TransactionStatus)
                .Include(r => r.ApplicationUserActioned)
                .Where(r => r.ConsultantId == consultantId);

            var filters = filtersAndPagination?.Filters;
            if (filters != null)
            {
                if (filters.TransactionStatusId.HasValue)
                    query = query.Where(r => r.TransactionStatusId == filters.TransactionStatusId.Value);
                if (!string.IsNullOrEmpty(filters.TimeOffType))
                    query = query.Where(r => r.TimeOffType == filters.TimeOffType);
            }

            int totalCount = await query.CountAsync();

            var pagination = filtersAndPagination?.PaginationWithoutFilters?.Pagination;
            int page = pagination?.PageIndex ?? 1;
            int pageSize = (pagination?.PageSize ?? 0) > 0 ? pagination.PageSize : 50;

            var requests = await query
                .OrderByDescending(r => r.CreationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new TimeOffRequestListVM
                {
                    TimeOffRequestId = r.TimeOffRequestId,
                    TimeOffType = r.TimeOffType,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    BusinessDays = r.BusinessDays,
                    Status = r.TransactionStatus.Name,
                    ConsultantName = "",
                    ConsultantId = r.ConsultantId,
                    ManagerName = r.ApplicationUserActioned != null
                        ? r.ApplicationUserActioned.Name + " " + r.ApplicationUserActioned.LastName
                        : null,
                    ActionDate = r.ActionDate,
                    RejectionComment = r.RejectionComment,
                    CreationDate = r.CreationDate
                })
                .ToListAsync();

            return (requests, totalCount);
        }

        public async Task<(List<TimeOffRequestListVM> requests, int totalCount)> GetManagerRequestsAsync(
            int managerConsultantId, TimeOffPaginationFiltersVM filtersAndPagination)
        {
            var managedProjectIds = await _db.PROJECTS
                .Where(p => p.SuccessManagerId == managerConsultantId && p.IsActive)
                .Select(p => p.ProjectId)
                .ToListAsync();

            var managedConsultantIds = await _db.PROJECTS_CONSULTANTS_ASSIGNED
                .Where(pca => managedProjectIds.Contains(pca.ProjectId))
                .Select(pca => pca.ConsultantId)
                .Distinct()
                .ToListAsync();

            var query = _db.TIME_OFF_REQUESTS
                .Include(r => r.ConsultantDetail).ThenInclude(c => c.ApplicationUser)
                .Include(r => r.TransactionStatus)
                .Include(r => r.ApplicationUserActioned)
                .Where(r => managedConsultantIds.Contains(r.ConsultantId));

            var filters = filtersAndPagination?.Filters;
            if (filters != null)
            {
                if (filters.TransactionStatusId.HasValue)
                    query = query.Where(r => r.TransactionStatusId == filters.TransactionStatusId.Value);
                if (!string.IsNullOrEmpty(filters.TimeOffType))
                    query = query.Where(r => r.TimeOffType == filters.TimeOffType);
                if (!string.IsNullOrEmpty(filters.StatusName))
                    query = query.Where(r => r.TransactionStatus.Name == filters.StatusName);
                if (filters.ConsultantId.HasValue)
                    query = query.Where(r => r.ConsultantId == filters.ConsultantId.Value);
                if (!string.IsNullOrEmpty(filters.SearchText))
                    query = query.Where(r =>
                        r.ConsultantDetail.ApplicationUser.Name.Contains(filters.SearchText) ||
                        r.ConsultantDetail.ApplicationUser.LastName.Contains(filters.SearchText) ||
                        (r.ConsultantDetail.ApplicationUser.Name + " " + r.ConsultantDetail.ApplicationUser.LastName).Contains(filters.SearchText));
                if (filters.ProjectId.HasValue)
                {
                    var consultantsOnProject = await _db.PROJECTS_CONSULTANTS_ASSIGNED
                        .Where(pca => pca.ProjectId == filters.ProjectId.Value)
                        .Select(pca => pca.ConsultantId)
                        .ToListAsync();
                    query = query.Where(r => consultantsOnProject.Contains(r.ConsultantId));
                }
            }

            int totalCount = await query.CountAsync();

            var pagination = filtersAndPagination?.PaginationWithoutFilters?.Pagination;
            int page = pagination?.PageIndex ?? 1;
            int pageSize = (pagination?.PageSize ?? 0) > 0 ? pagination.PageSize : 50;

            var requests = await query
                .OrderByDescending(r => r.CreationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new TimeOffRequestListVM
                {
                    TimeOffRequestId = r.TimeOffRequestId,
                    TimeOffType = r.TimeOffType,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    BusinessDays = r.BusinessDays,
                    Status = r.TransactionStatus.Name,
                    ConsultantName = r.ConsultantDetail.ApplicationUser.Name + " "
                                   + r.ConsultantDetail.ApplicationUser.LastName,
                    ConsultantId = r.ConsultantId,
                    ManagerName = r.ApplicationUserActioned != null
                        ? r.ApplicationUserActioned.Name + " " + r.ApplicationUserActioned.LastName
                        : null,
                    ActionDate = r.ActionDate,
                    RejectionComment = r.RejectionComment,
                    CreationDate = r.CreationDate
                })
                .ToListAsync();

            return (requests, totalCount);
        }

        public async Task<MethodResponse> SubmitRequestAsync(
            string userIdCreatedBy, int consultantId, SubmitTimeOffRequestVM data, string baseUrl)
        {
            var validTypes = new[] { "PTO", "UPTO", "VTO" };
            if (!validTypes.Contains(data.TimeOffType))
                return MethodResponse.CreateFailureNotFoundResponse("Invalid time off type.");

            if (data.StartDate > data.EndDate)
                return MethodResponse.CreateFailureNotFoundResponse("Start date must be before or equal to end date.");
            if (data.StartDate < DateTime.UtcNow.Date)
                return MethodResponse.CreateFailureNotFoundResponse("Cannot request time off in the past.");

            int businessDays = await CalculateBusinessDays(consultantId, data.StartDate.Value, data.EndDate.Value);
            if (businessDays == 0)
                return MethodResponse.CreateFailureNotFoundResponse(
                    "No business days in the selected range (weekends and holidays are excluded).");

            var consultant = await _db.CONSULTANT_DETAILS
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(c => c.ConsultantId == consultantId);

            if (data.TimeOffType == "PTO")
            {
                if (!consultant.IsEligibleForPaidTimeOff)
                    return MethodResponse.CreateFailureNotFoundResponse("You are not eligible for Paid Time Off.");

                var balances = await GetBalancesAsync(consultantId);
                if (businessDays > balances.PtoAvailable)
                    return MethodResponse.CreateFailureNotFoundResponse(
                        $"Insufficient PTO balance. Available: {balances.PtoAvailable} days, Requested: {businessDays} days.");
            }
            else if (data.TimeOffType == "VTO")
            {
                var balances = await GetBalancesAsync(consultantId);
                if (businessDays > balances.VtoAvailable)
                    return MethodResponse.CreateFailureNotFoundResponse(
                        "Insufficient VTO balance. You have already used your voluntary day off this year.");
            }

            var pendingStatus = await _db.TRANSACTION_STATUSES
                .FirstOrDefaultAsync(s => s.Name == "Waiting to be approved");
            if (pendingStatus == null)
                return MethodResponse.CreateFailureNotFoundResponse(
                    "Transaction status 'Waiting to be approved' not found in the database.");

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var request = new TimeOffRequest
                {
                    ConsultantId = consultantId,
                    TimeOffType = data.TimeOffType,
                    StartDate = data.StartDate.Value,
                    EndDate = data.EndDate.Value,
                    BusinessDays = businessDays,
                    TransactionStatusId = pendingStatus.TransactionStatusId,
                    CreationDate = DateTime.UtcNow,
                    UserCreatedBy = userIdCreatedBy
                };

                await _db.TIME_OFF_REQUESTS.AddAsync(request);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                await SendSubmissionEmails(consultant, request, baseUrl);

                return MethodResponse.CreateSuccessResponse(
                    "Your time off request has been submitted successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return MethodResponse.CreateFailureExceptionResponse(ex.Message);
            }
        }

        public async Task<MethodResponse> ApproveRejectRequestAsync(
            string userIdActionedBy, ApproveRejectTimeOffVM data, string baseUrl)
        {
            var transactionStatus = await _db.TRANSACTION_STATUSES
                .FirstOrDefaultAsync(s => s.Name == data.TransactionStatus);
            if (transactionStatus == null)
                return MethodResponse.CreateFailureNotFoundResponse(
                    $"Transaction status '{data.TransactionStatus}' not found.");

            var request = await _db.TIME_OFF_REQUESTS
                .Include(r => r.ConsultantDetail)
                    .ThenInclude(c => c.ApplicationUser)
                .Include(r => r.TransactionStatus)
                .FirstOrDefaultAsync(r => r.TimeOffRequestId == data.TimeOffRequestId);

            if (request == null)
                return MethodResponse.CreateFailureNotFoundResponse("Time off request not found.");

            if (request.TransactionStatus.Name != "Waiting to be approved")
                return MethodResponse.CreateFailureNotFoundResponse(
                    "This request has already been processed.");

            var managerUser = await _db.AspNetUsers
                .FirstOrDefaultAsync(u => u.Id == userIdActionedBy);
            if (managerUser == null)
                return MethodResponse.CreateFailureNotFoundResponse("Manager user not found.");

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                request.TransactionStatusId = transactionStatus.TransactionStatusId;
                request.UserActionedBy = userIdActionedBy;
                request.ActionDate = DateTime.UtcNow;

                if (data.TransactionStatus == "Rejected")
                {
                    request.RejectionComment = data.RejectionComment;
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // Reload TransactionStatus for email
                request.TransactionStatus = transactionStatus;
                await SendDecisionEmail(request, managerUser, baseUrl);

                return MethodResponse.CreateSuccessResponse(
                    $"The time off request was {data.TransactionStatus.ToLower()} successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return MethodResponse.CreateFailureExceptionResponse(ex.Message);
            }
        }

        public async Task<TimeOffWidgetVM> GetWidgetDataAsync(int consultantId)
        {
            var recentRequests = await _db.TIME_OFF_REQUESTS
                .Include(r => r.TransactionStatus)
                .Where(r => r.ConsultantId == consultantId)
                .OrderByDescending(r => r.CreationDate)
                .Take(4)
                .Select(r => new TimeOffCalendarEntryVM
                {
                    TimeOffRequestId = r.TimeOffRequestId,
                    TimeOffType = r.TimeOffType,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    Status = r.TransactionStatus.Name
                })
                .ToListAsync();

            var statusPending = await _db.TRANSACTION_STATUSES
                .FirstOrDefaultAsync(s => s.Name == "Waiting to be approved");

            int pendingCount = statusPending != null
                ? await _db.TIME_OFF_REQUESTS
                    .CountAsync(r => r.ConsultantId == consultantId
                        && r.TransactionStatusId == statusPending.TransactionStatusId)
                : 0;

            int totalCount = await _db.TIME_OFF_REQUESTS
                .CountAsync(r => r.ConsultantId == consultantId);

            return new TimeOffWidgetVM
            {
                UpcomingApproved = recentRequests,
                TotalCount = totalCount,
                PendingCount = pendingCount
            };
        }

        public async Task<List<TimeOffRequestListVM>> GetTeamWidgetDataAsync(int managerConsultantId)
        {
            var managedProjectIds = await _db.PROJECTS
                .Where(p => p.SuccessManagerId == managerConsultantId && p.IsActive)
                .Select(p => p.ProjectId)
                .ToListAsync();

            var managedConsultantIds = await _db.PROJECTS_CONSULTANTS_ASSIGNED
                .Where(pca => managedProjectIds.Contains(pca.ProjectId))
                .Select(pca => pca.ConsultantId)
                .Distinct()
                .ToListAsync();

            var statusPending = await _db.TRANSACTION_STATUSES
                .FirstOrDefaultAsync(s => s.Name == "Waiting to be approved");
            int pendingStatusId = statusPending?.TransactionStatusId ?? 0;

            var requests = await _db.TIME_OFF_REQUESTS
                .Include(r => r.ConsultantDetail).ThenInclude(c => c.ApplicationUser)
                .Include(r => r.TransactionStatus)
                .Where(r => managedConsultantIds.Contains(r.ConsultantId))
                .OrderByDescending(r => r.TransactionStatusId == pendingStatusId ? 1 : 0)
                .ThenByDescending(r => r.CreationDate)
                .Take(5)
                .Select(r => new TimeOffRequestListVM
                {
                    TimeOffRequestId = r.TimeOffRequestId,
                    TimeOffType = r.TimeOffType,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    BusinessDays = r.BusinessDays,
                    Status = r.TransactionStatus.Name,
                    ConsultantName = r.ConsultantDetail.ApplicationUser.Name + " "
                                   + r.ConsultantDetail.ApplicationUser.LastName,
                    ConsultantId = r.ConsultantId,
                    ActionDate = r.ActionDate,
                    RejectionComment = r.RejectionComment,
                    CreationDate = r.CreationDate
                })
                .ToListAsync();

            return requests;
        }

        public async Task<List<TimeOffRequestListVM>> GetAllConsultantRequestsAsync(int consultantId)
        {
            return await _db.TIME_OFF_REQUESTS
                .Include(r => r.TransactionStatus)
                .Include(r => r.ApplicationUserActioned)
                .Where(r => r.ConsultantId == consultantId)
                .OrderByDescending(r => r.CreationDate)
                .Select(r => new TimeOffRequestListVM
                {
                    TimeOffRequestId = r.TimeOffRequestId,
                    TimeOffType = r.TimeOffType,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    BusinessDays = r.BusinessDays,
                    Status = r.TransactionStatus.Name,
                    ConsultantName = "",
                    ConsultantId = r.ConsultantId,
                    ManagerName = r.ApplicationUserActioned != null
                        ? r.ApplicationUserActioned.Name + " " + r.ApplicationUserActioned.LastName
                        : null,
                    ActionDate = r.ActionDate,
                    RejectionComment = r.RejectionComment,
                    CreationDate = r.CreationDate
                })
                .ToListAsync();
        }

        public async Task<List<DateTime>> GetConsultantHolidayDatesAsync(int consultantId)
        {
            var consultant = await _db.CONSULTANT_DETAILS
                .FirstOrDefaultAsync(c => c.ConsultantId == consultantId);

            if (consultant?.ConsultantHolidayId == null)
                return new List<DateTime>();

            return await _db.CONSULTANT_HOLIDAY_DATES
                .Where(h => h.ConsultantHolidayId == consultant.ConsultantHolidayId)
                .Select(h => h.Date)
                .ToListAsync();
        }

        // ── Private helpers ──


        private async Task<int> GetUsedDaysAsync(int consultantId, string timeOffType, int? calendarYear)
        {
            var statusApproved = await _db.TRANSACTION_STATUSES
                .FirstOrDefaultAsync(s => s.Name == "Approved");
            var statusPending = await _db.TRANSACTION_STATUSES
                .FirstOrDefaultAsync(s => s.Name == "Waiting to be approved");

            var query = _db.TIME_OFF_REQUESTS
                .Where(r => r.ConsultantId == consultantId
                    && r.TimeOffType == timeOffType
                    && (r.TransactionStatusId == statusApproved.TransactionStatusId
                        || r.TransactionStatusId == statusPending.TransactionStatusId));

            if (calendarYear.HasValue)
            {
                query = query.Where(r => r.StartDate.Year == calendarYear.Value);
            }

            return await query.SumAsync(r => r.BusinessDays);
        }

        private async Task<int> CalculateBusinessDays(int consultantId, DateTime startDate, DateTime endDate)
        {
            var holidayDates = await GetConsultantHolidayDatesAsync(consultantId);
            var holidaySet = new HashSet<DateTime>(holidayDates.Select(d => d.Date));

            int count = 0;
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;
                if (holidaySet.Contains(date))
                    continue;
                count++;
            }
            return count;
        }

        private async Task SendSubmissionEmails(ConsultantDetail consultant, TimeOffRequest request, string baseUrl)
        {
            var emailTemplates = new EmailTemplates();
            string dateRange = FormatDateRange(request.StartDate, request.EndDate);

            // Email 1: To consultant (confirmation)
            try
            {
                var confirmBody = emailTemplates.TimeOffRequestSubmittedBody(
                    consultant.ApplicationUser.Name, GetTimeOffTypeLabel(request.TimeOffType), dateRange, request.BusinessDays);
                var confirmEmail = new SendEmailVM
                {
                    Subject = "TIME OFF REQUEST SUBMITTED - RIPPLE BY OCEANS",
                    SharedEmailFrom = _config["SharedMailboxEmailRippleApp"],
                    EmailTo = consultant.ApplicationUser.Email.Trim(),
                    Body = emailTemplates.EmailTemplate("TIME OFF REQUEST SUBMITTED", confirmBody)
                };
                string msg = JsonConvert.SerializeObject(confirmEmail);
                await _queueClient.Value.SendMessageAsync(StringsMethods.Base64Encode(msg));
            }
            catch (Exception) { }

            // Email 2: To all distinct Success Managers
            try
            {
                var managerEmails = await GetDistinctSuccessManagerEmails(consultant.ConsultantId);
                string approvalUrl = $"{baseUrl}/General/TimeOffApprovals?consultantId={consultant.ConsultantId}";

                var approvalBody = emailTemplates.TimeOffApprovalRequestBody(
                    approvalUrl,
                    consultant.ApplicationUser.Name + " " + consultant.ApplicationUser.LastName,
                    GetTimeOffTypeLabel(request.TimeOffType), dateRange, request.BusinessDays);
                var templateEmail = emailTemplates.EmailTemplate("TIME OFF REQUEST TO REVIEW", approvalBody);

                foreach (var managerEmail in managerEmails)
                {
                    var approvalEmail = new SendEmailVM
                    {
                        Subject = "TIME OFF REQUEST TO REVIEW - RIPPLE BY OCEANS",
                        SharedEmailFrom = _config["SharedMailboxEmailRippleApp"],
                        EmailTo = managerEmail.Trim(),
                        Body = templateEmail
                    };
                    string msg = JsonConvert.SerializeObject(approvalEmail);
                    await _queueClient.Value.SendMessageAsync(StringsMethods.Base64Encode(msg));
                }
            }
            catch (Exception) { }
        }

        private async Task<List<string>> GetDistinctSuccessManagerEmails(int consultantId)
        {
            var managerConsultantIds = await _db.PROJECTS_CONSULTANTS_ASSIGNED
                .Where(pca => pca.ConsultantId == consultantId)
                .Join(_db.PROJECTS,
                    pca => pca.ProjectId,
                    p => p.ProjectId,
                    (pca, p) => p)
                .Where(p => p.IsActive)
                .Select(p => p.SuccessManagerId)
                .Distinct()
                .ToListAsync();

            var emails = await _db.CONSULTANT_DETAILS
                .Where(cd => managerConsultantIds.Contains(cd.ConsultantId))
                .Join(_db.AspNetUsers,
                    cd => cd.UserId,
                    u => u.Id,
                    (cd, u) => u.Email)
                .Distinct()
                .ToListAsync();

            return emails;
        }

        private async Task SendDecisionEmail(TimeOffRequest request, ApplicationUser managerUser, string baseUrl)
        {
            try
            {
                var emailTemplates = new EmailTemplates();
                string dateRange = FormatDateRange(request.StartDate, request.EndDate);
                string status = request.TransactionStatus?.Name ?? "Unknown";
                string consultantName = request.ConsultantDetail.ApplicationUser.Name;
                string managerName = managerUser.Name + " " + managerUser.LastName;

                string buttonUrl = $"{baseUrl}/General/TimeOff";
                var body = emailTemplates.TimeOffDecisionBody(
                    buttonUrl, consultantName, GetTimeOffTypeLabel(request.TimeOffType), dateRange,
                    request.BusinessDays, status, managerName, request.RejectionComment);

                string titlePrefix = status == "Approved"
                    ? "YOUR TIME OFF REQUEST WAS APPROVED"
                    : "YOUR TIME OFF REQUEST WAS REJECTED";

                var email = new SendEmailVM
                {
                    Subject = $"{titlePrefix} - RIPPLE BY OCEANS",
                    SharedEmailFrom = _config["SharedMailboxEmailRippleApp"],
                    EmailTo = request.ConsultantDetail.ApplicationUser.Email.Trim(),
                    Body = emailTemplates.EmailTemplate(titlePrefix, body)
                };
                string msg = JsonConvert.SerializeObject(email);
                await _queueClient.Value.SendMessageAsync(StringsMethods.Base64Encode(msg));
            }
            catch (Exception) { }
        }

        private static string GetTimeOffTypeLabel(string timeOffType)
        {
            return timeOffType switch
            {
                "PTO" => "Paid Time Off",
                "UPTO" => "Unpaid Time Off",
                "VTO" => "Voluntary Time Off",
                _ => timeOffType
            };
        }

        private static string FormatDateRange(DateTime start, DateTime end)
        {
            string s = start.ToString("MMM d", System.Globalization.CultureInfo.InvariantCulture);
            string e = end.ToString("MMM d", System.Globalization.CultureInfo.InvariantCulture);
            return $"{s} - {e}";
        }
    }
}
