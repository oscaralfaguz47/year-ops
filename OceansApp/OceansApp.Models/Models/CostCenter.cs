using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class CostCenter
    {
        [Key]
        public int CostCenterId { get; set; }
        [MaxLength(25)]
        [Required]
        public string CostCenterCode { get; set; }
        [MaxLength(200)]
        [Required]
        public string Description { get; set; }
        [MaxLength(500)]
        public string? Detail { get; set; }
        [MaxLength(1)]
        [Required]
        public string AcceptData { get; set; }
        [Required]
        public DateTime CreateDate { get; set; }
         [MaxLength(8)]
        public string? CompanyId { get; set; }

    }
}
