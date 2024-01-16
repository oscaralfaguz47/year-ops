
namespace OceansApp.Models.ViewModels.Components
{
    public class PaginationFilters
    {
        public object SetPaginationAndFilters(object paginationFiltersFromModel, object paginationFiltersToApply)
        {

            if (paginationFiltersFromModel.GetType().GetProperty("Pagination").GetValue(paginationFiltersFromModel) != null && paginationFiltersFromModel.GetType().GetProperty("Filters").GetValue(paginationFiltersFromModel) != null)
            {
                var paginationFromModel = paginationFiltersFromModel.GetType().GetProperty("Pagination").GetValue(paginationFiltersFromModel);
                var orderByValueFromModel = paginationFiltersFromModel.GetType().GetProperty("OrderBy").GetValue(paginationFiltersFromModel); ;
                var filtersFromModel = paginationFiltersFromModel.GetType().GetProperty("Filters").GetValue(paginationFiltersFromModel);


                var pageSizeValueFromModel = paginationFromModel.GetType().GetProperty("PageSize").GetValue(paginationFromModel);
                var pageIndexValueFromModel = paginationFromModel.GetType().GetProperty("PageIndex").GetValue(paginationFromModel);
                var paginationToApply = paginationFiltersToApply.GetType().GetProperty("Pagination").GetValue(paginationFiltersToApply);
                var pageSizePropertyToApply = paginationToApply.GetType().GetProperty("PageSize");
                var pageIndexPropertyToApply = paginationToApply.GetType().GetProperty("PageIndex");

                var filtersToApply = paginationFiltersToApply.GetType().GetProperty("Filters");
                var RequestFromFiltersValueFromModel = (bool)paginationFiltersFromModel.GetType().GetProperty("RequestFromFilters").GetValue(paginationFiltersFromModel);
                var orderByPropertyToApply = paginationFiltersToApply.GetType().GetProperty("OrderBy");

                int numAppliedFilters = 0;
                foreach (var prop in filtersFromModel.GetType().GetProperties())
                {
                    var value = prop.GetValue(filtersFromModel, null);
                    if (value is not null and not "")
                    {
                        numAppliedFilters++;
                    }
                }
                if ((int)pageSizeValueFromModel != 0)
                {
                    pageSizePropertyToApply.SetValue(paginationToApply, pageSizeValueFromModel);
                }

                if (numAppliedFilters > 0)
                {
                    filtersToApply.SetValue(paginationFiltersToApply, filtersFromModel);
                    if (RequestFromFiltersValueFromModel == false)
                    {
                        pageIndexPropertyToApply.SetValue(paginationToApply, pageIndexValueFromModel);
                    }
                }
                else
                {
                    pageIndexPropertyToApply.SetValue(paginationToApply, pageIndexValueFromModel);
                }
                if (orderByValueFromModel != null)
                {
                    orderByPropertyToApply.SetValue(paginationFiltersToApply, orderByValueFromModel);
                }
            }
            return paginationFiltersToApply;
        }
    }
}
