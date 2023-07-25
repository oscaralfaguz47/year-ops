
namespace OceansApp.Utility
{
    public class Pagination
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalResults { get; set; }
        public bool IsLastPage => (TotalResults - (PageIndex * PageSize)) <= 0;
      
    }
}
