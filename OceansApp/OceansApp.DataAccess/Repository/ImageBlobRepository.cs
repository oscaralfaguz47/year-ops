using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;


namespace OceansApp.DataAccess.Repository
{
    public class ImageBlobRepository : Repository<ImageBlob>, IImageBlobRepository
    {
        private ApplicationDbContext _db;
        public ImageBlobRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


    }
}
