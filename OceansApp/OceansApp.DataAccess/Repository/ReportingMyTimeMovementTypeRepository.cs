using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;


namespace OceansApp.DataAccess.Repository
{
    public class ReportingMyTimeMovementTypeRepository : Repository<ReportingMyTimeMovementType>, IReportingMyTimeMovementTypeRepository
    {
        private ApplicationDbContext _db;
        public ReportingMyTimeMovementTypeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        
    }
}
