
using Microsoft.AspNetCore.Identity;

namespace OceansApp.DataAccess.DbInitializer
{
    public interface IDbInitializer
    {
        Task InitializeAsync();
    }
}
