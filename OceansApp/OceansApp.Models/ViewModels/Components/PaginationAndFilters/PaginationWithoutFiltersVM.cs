

namespace OceansApp.Models.ViewModels.Components.PaginationAndFilters
{
    public class PaginationWithoutFiltersVM
    {
        public bool? RequestFromFilters { get; set; }
        public Pagination? Pagination { get; set; }
        public OrderByVM? OrderBy { get; set; }
    }
}
