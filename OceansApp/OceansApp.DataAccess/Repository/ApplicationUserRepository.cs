using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels;
using OceansApp.Models.ViewModels.Account;
using OceansApp.Models.ViewModels.ApplicationUser;
using OceansApp.Models.ViewModels.Dashboard;
using OceansApp.Utility.ConstantData;
using OceansApp.Utility.ConstantData.Claims;
using OceansApp.Utility.SharedMethods.Blobs;
using System.Linq.Expressions;
using System.Security.Claims;

namespace OceansApp.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _db;
        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<ProfileVM> GetUserProfileDataAsync(string userId)
        {
            var result = await (from u in _db.AspNetUsers
                                join ib in _db.IMAGE_BLOBS on u.Id equals ib.EntityId
                                into ibGroup
                                from ib in ibGroup.DefaultIfEmpty()
                                where u.Id == userId && ib.ContainerName == "user-profile-photos" && ib.EntityType == "UserProfile"
                                select new ProfileVM
                                {
                                    Id = u.Id,
                                    Name = u.Name,
                                    LastName = u.LastName,
                                    Email = u.Email,
                                    Ocupation = u.Occupation,
                                    PhoneNumber = u.PhoneNumber,
                                    ProfileUrl = ib.BlobUrl
                                }).FirstOrDefaultAsync();
            return result;
        }
        public async Task<bool> AnyAsync(Expression<Func<ApplicationUser, bool>> predicate)
        {
            return await _db.AspNetUsers.AnyAsync(predicate);
        }

        public async Task<List<GetUserIdVM>> GetUsersWhereRoleId(string roleId)
        {
            var userRoles = await _db.UserRoles.Where(x => x.RoleId == roleId).ToListAsync();
            List<GetUserIdVM> usersList = new List<GetUserIdVM>();
            foreach (var user in userRoles)
            {
                GetUserIdVM userToAdd = new GetUserIdVM()
                {
                    UserId = user.UserId
                };
                usersList.Add(userToAdd);
            }
            return usersList;
        }

        public async Task<UserAndConsultantVM> GetUserAndConsultantAsync(string userId)
        {
            var result = await (from u in _db.AspNetUsers
                                join cd in _db.CONSULTANT_DETAILS on u.Id equals cd.UserId
                                into cdGroup
                                from cd in cdGroup.DefaultIfEmpty()
                                join uc in _db.UserCategories on u.UserCategoryId equals uc.UserCategoryId
                                where u.Id == userId
                                select new UserAndConsultantVM
                                {
                                    UserId = u.Id,
                                    ConsultantId = cd.ConsultantId,
                                    Name = u.Name,
                                    LastName = u.LastName,
                                    ConsultantHolidayId = cd.ConsultantHolidayId,
                                    StartDate = cd.StartDate,
                                    WorkingModel = cd.WorkingModel,
                                    UserCategoryName = uc.Name,
                                    PaymentPeriod = (int)cd.PaymentPeriod,
                                    Email = u.Email
                                }).FirstOrDefaultAsync();

            if (result == null) throw new InvalidOperationException("The user does not exist.");

            return result;
        }
        public List<WidgetVM> GetWidgetsForUser(UserAndConsultantVM userAndConsultant, ClaimsPrincipal userClaims,
            (int Years, int Months, int Days)? activeTime)
        {
            try
            {
                List<WidgetVM> listToReturn = new();

                //Access to Report time in tracking tool
                if (userClaims.IsAuthorizedForReportTimeInTrackingTool() && userAndConsultant.UserCategoryName != "External User")
                {
                    WidgetVM timeSheetsW = new() { WidgetName = WidgetsCD.TimeSheets };
                    listToReturn.Add(timeSheetsW);
                }
                //Access to Benefits
                if (userAndConsultant.UserCategoryName != "External User" && activeTime != null)
                {
                    // Full time
                    if (userAndConsultant.WorkingModel == 1)
                    {
                        var sections = new List<string>();

                        // Dictionary of conditions and their respective sections
                        var perksByCondition = new Dictionary<Func<(int Years, int Months, int Days), bool>, List<string>>
    {
        { activeTime => activeTime.Months >= 1 || activeTime.Years >= 1 || activeTime.Days >= 0, new List<string> { BenefitsCD.Bonusly, BenefitsCD.Oceans_Challenge, BenefitsCD.VTO } },
        { activeTime => activeTime.Months >= 4 || activeTime.Years >= 1, new List<string> { BenefitsCD.Balance_Program } }
    };

                        // Evaluate each condition and accumulate the corresponding sections
                        foreach (var condition in perksByCondition)
                        {
                            if (condition.Key(((int Years, int Months, int Days))activeTime))
                            {
                                sections.AddRange(condition.Value);
                            }
                        }

                        // Add a single widget with all the corresponding sections sorted numerically
                        if (sections.Any())
                        {
                            listToReturn.Add(new WidgetVM { WidgetName = WidgetsCD.Perks, Sections = sections.Distinct().OrderBy(s => int.Parse(s.Substring(0, 1))).ToList() });
                        }
                    }

                    // Part time
                    if (userAndConsultant.WorkingModel == 2)
                    {
                        var sections = new List<string>();

                        // Dictionary of conditions and their respective sections
                        var perksByCondition = new Dictionary<Func<(int Years, int Months, int Days), bool>, List<string>>
    {
        { activeTime => activeTime.Months >= 1 || activeTime.Years >= 1 || activeTime.Days >= 0, new List<string> { BenefitsCD.Bonusly, BenefitsCD.Oceans_Challenge, BenefitsCD.VTO } }
    };

                        // Evaluate each condition and accumulate the corresponding sections
                        foreach (var condition in perksByCondition)
                        {
                            if (condition.Key(((int Years, int Months, int Days))activeTime))
                            {
                                sections.AddRange(condition.Value);
                            }
                        }

                        // Add a single widget with all the corresponding sections sorted numerically
                        if (sections.Any())
                        {
                            listToReturn.Add(new WidgetVM { WidgetName = WidgetsCD.Perks, Sections = sections.Distinct().OrderBy(s => int.Parse(s.Substring(0, 1))).ToList() });
                        }
                    }

                    // Hourly
                    if (userAndConsultant.WorkingModel == 3)
                    {
                        listToReturn.Add(new WidgetVM { WidgetName = WidgetsCD.Perks, Sections = new List<string> { BenefitsCD.Bonusly } });
                    }


                }
                //General Consultant and Admin Team
                if (userAndConsultant.UserCategoryName != "External User")
                {
                    WidgetVM generalConsultantAndAdminW = new() { WidgetName = WidgetsCD.GeneralConsultantAndAdmin };
                    listToReturn.Add(generalConsultantAndAdminW);
                }

                return listToReturn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<(int Years, int Months, int Days)> GetUserActiveTimeAsync(string userId)
        {
            var query = from history in _db.UsersActiveHistory
                        join subQuery in
                            (from h in _db.UsersActiveHistory
                             where h.UserId == userId
                             group h by h.ActionDate.Date into g
                             select g.OrderByDescending(x => x.HistoryId).FirstOrDefault().HistoryId)
                        on history.HistoryId equals subQuery
                        orderby history.ActionDate
                        select history;

            var activeHistoryMovements = await query.ToListAsync();
            var calculatedTime = CalculateExactActiveTime(activeHistoryMovements);

            return calculatedTime;
        }
        private (int Years, int Months, int Days) CalculateExactActiveTime(List<ApplicationUserActiveHistory> activeHistoryMovements)
        {
            int totalYears = 0;
            int totalMonths = 0;
            int totalDays = 0;

            DateTime? activationDate = null;

            foreach (var record in activeHistoryMovements)
            {
                if (record.IsActive)
                {
                    if (activationDate == null)
                    {
                        activationDate = record.ActionDate;
                    }
                }
                else
                {
                    if (activationDate != null)
                    {
                        AddExactActivePeriod(activationDate.Value, record.ActionDate, ref totalYears, ref totalMonths, ref totalDays);
                        activationDate = null;
                    }
                }
            }

            if (activationDate != null)
            {
                AddExactActivePeriod(activationDate.Value, DateTime.Now, ref totalYears, ref totalMonths, ref totalDays);
            }

            return (totalYears, totalMonths, totalDays);
        }

        private void AddExactActivePeriod(DateTime start, DateTime end, ref int totalYears, ref int totalMonths, ref int totalDays)
        {
            int years = 0, months = 0, days = 0;

            while (start.AddYears(1) <= end)
            {
                years++;
                start = start.AddYears(1);
            }

            while (start.AddMonths(1) <= end)
            {
                months++;
                start = start.AddMonths(1);
            }

            days = (end - start).Days;

            totalYears += years;
            totalMonths += months;
            totalDays += days;

            if (totalDays >= DateTime.DaysInMonth(end.Year, (start.Month % 12) + 1))
            {
                totalMonths += totalDays / DateTime.DaysInMonth(end.Year, (start.Month % 12) + 1);
                totalDays %= DateTime.DaysInMonth(end.Year, (start.Month % 12) + 1);
            }

            if (totalMonths >= 12)
            {
                totalYears += totalMonths / 12;
                totalMonths %= 12;
            }
        }

        public async Task<ImageBlob> VerifyIfUploadedFileAsync(IFormFile file, string entityId, string containerName, string entityType)
        {
            CalculateContentHash calculateHash = new CalculateContentHash();

            string fileNameWithHass = $"{await calculateHash.CalculateContentHashAsync((IFormFile)file)}_{file.FileName}";

            var existingFile = await _db.IMAGE_BLOBS.FirstOrDefaultAsync(x => x.BlobName == fileNameWithHass
            && x.EntityId == entityId && x.ContainerName == containerName && x.EntityType == entityType);

            return existingFile;
        }
    }
}
