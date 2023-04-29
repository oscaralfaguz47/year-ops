using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class AccountingAccount
    {
        [Key]
        [MaxLength(25)]
        public string? IdAccountingAccount { get; set; }
        [Required]
        [MaxLength(400)]
        public string? Description { get; set; }
        [Required]
        [MaxLength(1)]
        public string? AccountingAccountType { get; set; }
        [Required]
        [MaxLength(1)]
        public string? DetailedType { get; set; }
        [Required]
        [MaxLength(1)]
        public string? Balance { get; set; }
        [Required]
        [MaxLength(1)]
        public string? AcceptData { get; set; }
        [Required]
        [MaxLength(1)]
        public string? UseCostCenter { get; set; }
        [Required]
        [MaxLength(1)]
        public string? UseThird { get; set; }
        [Required]
        public DateTime DateLastUpdate { get; set; }
        [Required]
        public DateTime DateHour { get; set; }

    }
}
