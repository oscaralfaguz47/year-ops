
using OceansApp.Models.ViewModels.Components.PaginationAndFilters;

namespace OceansApp.Models.ViewModels.Components
{
    public class PaginationFiltersBehavior
    {
        public PaginationWithoutFiltersVM SetPagination(PaginationWithoutFiltersVM? paginationFromModel, int numAppliedFilters)
        {
            PaginationWithoutFiltersVM paginationToApply = new();
            paginationToApply.Pagination = new Pagination();
            paginationToApply.OrderBy = new OrderByVM();
            if (paginationFromModel != null)
            {
                if (paginationFromModel.Pagination != null)
                {
                    if (paginationFromModel.Pagination.PageSize != 0)
                    {
                        paginationToApply.Pagination.PageSize = paginationFromModel.Pagination.PageSize;
                    }
                    if (numAppliedFilters > 0)
                    {
                        if (paginationFromModel.RequestFromFilters == false)
                        {
                            paginationToApply.Pagination.PageIndex = paginationFromModel.Pagination.PageIndex;
                        }
                    }
                    else
                    {
                        paginationToApply.Pagination.PageIndex = paginationFromModel.Pagination.PageIndex;
                    }
                    if (paginationFromModel.OrderBy != null)
                    {
                        paginationToApply.OrderBy = paginationFromModel.OrderBy;
                    }
                }
            }
            return paginationToApply;
        }
    }
}
