
namespace OceansApp.Models.ViewModels.Consultants
{
    public class ConsultantsFiltersGetAllVM
    {
        public string? SearchText { get; set; }
        public string? CountryId { get; set; }
        public bool? IsTwoFactorEnabled { get; set; }
        public bool? EmailConfirmed { get; set; }
        public bool? IsActive { get; set; }
        public int? UserCategoryId { get; set; }

        public string? ProjectIds { get; set; }
        public string? PositionIds { get; set; }
    }
}
