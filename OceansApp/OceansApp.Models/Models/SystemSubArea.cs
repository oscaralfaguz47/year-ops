using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class SystemSubArea
    {
        [Key]
        public int SystemSubAreaId { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        public int SystemAreaId { get; set; }
        [ForeignKey("SystemAreaId")]
        [ValidateNever]
        public SystemArea SystemArea { get; set; }
    }
}
