
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ConsultantAndPosition
    {
        [Required]
        public int ConsultantId { get; set; }
        [Required]
        public int ConsultantPositionId { get; set; }


    }
}
