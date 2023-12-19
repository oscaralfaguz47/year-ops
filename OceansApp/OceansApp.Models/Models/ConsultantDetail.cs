using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class ConsultantDetail
    {
        [ForeignKey("Id")]
        public string UserId { get; set; }
        public DateTime StartDate { get; set; }
        [MaxLength(4)]
        public string? IdCountry { get; set; }
        [MaxLength(50)]
        public string? Phone2 { get; set; }
        public string? Address { get; set; }
        [MaxLength(249)]
        public string? PersonalEmail { get; set; }
        public string? Location { get; set; }
        [MaxLength(20)]
        public string? ShirtSize { get; set; }



        [ValidateNever]
        public Country Country { get; set; }

        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

    }
}
