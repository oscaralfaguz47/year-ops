using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.ProjectsConsultantsPeriodsDisabledTrakings;


namespace OceansApp.DataAccess.Repository
{
    public class ProjectConsultantPeriodDisabledTrackingRepository : Repository<ProjectConsultantPeriodDisabledTracking>, IProjectConsultantPeriodDisabledTrackingRepository
    {
        private ApplicationDbContext _db;
        public ProjectConsultantPeriodDisabledTrackingRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<List<GetProjectsConsultantsPeriodsDisabledTrakingsVM>> GetRemovedProjectsInPeriodAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var results = await (from pdt in _db.PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS
                                    join cd in _db.CONSULTANT_DETAILS on pdt.ConsultantId equals cd.ConsultantId
                                    join cu in _db.AspNetUsers on cd.UserId equals cu.Id
                                    join p in _db.PROJECTS on pdt.ProjectId equals p.ProjectId
                                    join uc in _db.AspNetUsers on pdt.CreatedBy equals uc.Id
                                    where pdt.StartPeriodDate == startDate && pdt.EndPeriodDate == endDate
                                    orderby cu.Name, cu.LastName
                                    select new GetProjectsConsultantsPeriodsDisabledTrakingsVM
                                    {
                                        Id = pdt.Id,
                                        ConsultantName = cu.Name + " " + cu.LastName,
                                        ProjectName = p.Name,
                                        RemovedDate = pdt.CreationDate,
                                        UserRemovedBy = uc.Name + " " + uc.LastName
                                    }).ToListAsync();

                return results;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
