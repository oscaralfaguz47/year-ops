using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Consultants;
using System.Data;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantDetailRepository : Repository<ConsultantDetail>, IConsultantDetailRepository
    {
        private ApplicationDbContext _db;
        public ConsultantDetailRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<List<GetUsersSelectVM>> GetUsersByCategoryAndPositionForSelect(string userCategory, string userPosition)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@UserCategory", userCategory, DbType.String);
            parameters.Add("@UserPosition", userPosition, DbType.String);

            var results = await connection.QueryAsync<GetUsersSelectVM>("GetUsersByCategoryAndPosition", parameters, commandType: CommandType.StoredProcedure);

            var users = results.ToList();

            return (users);
        }
        public async Task<int> GetNumOfUsersByCategoryConsultantIdAndPosition(string userCategory, string userPosition, int consultantId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserCategory", userCategory, DbType.String);
            parameters.Add("@UserPosition", userPosition, DbType.String);
            parameters.Add("@ConsultantId", consultantId, DbType.Int32);

            var result = await connection.ExecuteScalarAsync<int>("GetNumOfUsersByCategoryConsultantIdAndPosition", parameters, commandType: CommandType.StoredProcedure);
            return result;
        }

        public async Task<List<GetConsultantsBySearchTextVM>> GetConsultantsBySearchText(string searchText)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@SearchText", searchText, DbType.String);

            var result = await connection.QueryAsync<GetConsultantsBySearchTextVM>("SP_CONSULTANT_DETAILS_SearchConsultantsBySearchText", parameters, commandType: CommandType.StoredProcedure);
            return result.ToList();
        }

        public void Update(ConsultantDetail obj)
        {
            _db.CONSULTANT_DETAILS.Update(obj);
        }

    }
}
