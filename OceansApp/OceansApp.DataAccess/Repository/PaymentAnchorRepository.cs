using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.DataAccess.Services;
using OceansApp.Models.ViewModels.PaymentAnchor;

namespace OceansApp.DataAccess.Repository
{
    public class PaymentAnchorRepository : IPaymentAnchorRepository
    {
        private readonly ApplicationDbContext _db;

        public PaymentAnchorRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PaymentAnchorResult?> ResolveAnchorAsync(int consultantId, DateTime periodEnd)
        {
            var history = await (from pca in _db.PROJECTS_CONSULTANTS_ASSIGNED
                                 join h in _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY
                                     on pca.ProjectConsultantAssignedId equals h.ProjectConsultantAssignedId
                                 where pca.ConsultantId == consultantId
                                       && h.ActionDate <= periodEnd.Date
                                 select new PaymentAnchorHistoryRecord
                                 {
                                     ProjectConsultantAssignedId = h.ProjectConsultantAssignedId,
                                     ProjectId = pca.ProjectId,
                                     PositionId = h.PositionId,
                                     HourlySalary = h.HourlySalary,
                                     MonthlySalary = h.MonthlySalary,
                                     ActionDate = h.ActionDate,
                                     Id = h.Id
                                 }).ToListAsync();

            return PaymentAnchorResolver.ResolveFromHistory(history, periodEnd);
        }
    }
}
