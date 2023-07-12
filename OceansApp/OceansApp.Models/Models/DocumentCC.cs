using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class DocumentCC
    {
        [Key]
        public int DocumentCCId { get; set; }
        [MaxLength(50)]
        public string DocumentNumber { get; set; }
        [MaxLength(3)]
        public string DocumentType { get; set; }
        [MaxLength(249)]
        public string ApplicationDescription { get; set; }
        public DateTime DocumentDate { get; set; }
        public Decimal DocumentAmount { get; set; }
        public Decimal BalanceAmount { get; set; }
        [MaxLength(1)]
        public string Canceled { get; set; }
        [MaxLength(10)]
        public string? IdSeat { get; set; }
        public DateTime? DateLastUpdate { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        public int ClientId { get; set; }
        [ForeignKey("ClientId")]
        [ValidateNever]
        public Client Client { get; set; }
        [MaxLength(8)]
        public string CompanyId { get; set; }

    }
}
