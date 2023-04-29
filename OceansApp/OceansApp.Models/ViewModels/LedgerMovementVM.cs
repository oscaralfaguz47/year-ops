using OceansApp.Models.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OceansApp.Models.ViewModels
{
    public class LedgerMovementVM
    {
        public LedgerMovement LedgerMovement { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> AccountingAccountsList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> CostsCenterList { get; set; }
    }
}
