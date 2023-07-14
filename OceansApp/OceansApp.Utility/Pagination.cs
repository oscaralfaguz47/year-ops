
namespace OceansApp.Utility
{
    public class Pagination
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalResults { get; set; }
        public List<int> PageSizeOptions { get; set; }
        public int SelectedPageSize { get; set; }
    }
}
