using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using System.Data;
using System.Text;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantSeniorityRepository : Repository<ConsultantSeniority>, IConsultantSeniorityRepository
    {
        private ApplicationDbContext _db;
        public ConsultantSeniorityRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<List<SelectVM>> GetSenioritisByRoleAsync(int roleId)
        {
            var connection = _db.Database.GetDbConnection();

            var queryBuilder = new StringBuilder();
            var parameters = new DynamicParameters();

            queryBuilder.AppendLine(@"
               SELECT
                CS.ConsultantSeniorityId AS Value
               ,CS.Name
               FROM CONSULTANT_ROLES_QUALITY_LEVELS RCL
               JOIN CONSULTANT_SENIORITIS CS ON RCL.ConsultantSeniorityId = CS.ConsultantSeniorityId
               WHERE ConsultantRoleId = @roleId
               GROUP BY CS.ConsultantSeniorityId, CS.Name");

            parameters.Add("@roleId", roleId, DbType.String);

            var results = await connection.QueryAsync<SelectVM>(queryBuilder.ToString(), parameters);
            var documents = results.ToList();

            return documents;
        }

        public void Update(ConsultantSeniority obj)
        {
            _db.CONSULTANT_SENIORITIS.Update(obj);
        }

    }
}
