using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.ConsultantRolesQualityLevels;
using System.Data;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantRoleQualityLevelRepository : Repository<ConsultantRolesQualityLevels>, IConsultantRoleQualityLevelRepository
    {
        private ApplicationDbContext _db;
        public ConsultantRoleQualityLevelRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public IEnumerable<GetConsultantRolesQualityLevelsVM> GetConsultantRoleQualityLevelsList()
        {
            var sqlQuery = @"
            SELECT 
                CRQ.ConsultantRoleId
                ,CRQ.ConsultantQualityLevelId
                ,CRQ.ConsultantSeniorityId
                ,CR.Name AS RoleName
                ,CQL.Name AS QualityLevelName
                ,CS.Name AS SeniorityName
                ,CRQ.ClientRateMaximumAmount
                ,CRQ.ConsultantMaximumAmount
                ,CRQ.UpdatedDate
                ,U.Name AS UpdatedByName
            FROM [dbo].[CONSULTANT_ROLES_QUALITY_LEVELS] CRQ
            JOIN CONSULTANT_QUALITY_LEVELS CQL ON CRQ.ConsultantQualityLevelId = CQL.ConsultantQualityLevelId
            JOIN CONSULTANT_ROLES CR ON CRQ.ConsultantRoleId = CR.ConsultantRoleId
            LEFT JOIN Users U ON CRQ.UpdatedBy = U.Id
            LEFT JOIN CONSULTANT_SENIORITIS CS ON CRQ.ConsultantSeniorityId = CS.ConsultantSeniorityId
            ORDER BY CR.NAME";

            using (var connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();

                var result = connection.Query<GetConsultantRolesQualityLevelsVM>(sqlQuery).ToList();

                return result;
            }
        }




        public void Update(ConsultantRolesQualityLevels obj)
        {
            _db.CONSULTANT_ROLES_QUALITY_LEVELS.Update(obj);
        }

    }
}
