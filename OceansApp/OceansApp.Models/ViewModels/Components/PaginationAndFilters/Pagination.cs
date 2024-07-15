
namespace OceansApp.Models.ViewModels.Components
{
    public class Pagination
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalResults { get; set; }
        public bool IsLastPage => (TotalResults - (PageIndex * PageSize)) <= 0;
        public IEnumerable<SelectVM> ItemsPerPageList { get; set; } = new List<SelectVM>
    {
        new SelectVM { Value = "50", Text = "50" },
        new SelectVM { Value = "100", Text = "100" },
        new SelectVM { Value = "200", Text = "200" },
        new SelectVM { Value = "300", Text = "300" }
    };

    }
}
