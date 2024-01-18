
namespace OceansApp.Models.ViewModels.Clients
{
    public class ClientsGetAllForListVM
    {
        public ClientsPaginationFiltersVM PaginationFilters { get; set; }
        public List<ClientsGetAllWithFiltersVM>? ClientsList { get; set; }
    }
}
