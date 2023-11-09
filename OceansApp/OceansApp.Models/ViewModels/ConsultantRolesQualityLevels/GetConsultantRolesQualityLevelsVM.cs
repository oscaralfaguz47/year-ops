

namespace OceansApp.Models.ViewModels.ConsultantRolesQualityLevels
{
    public class GetConsultantRolesQualityLevelsVM
    {
        public int ConsultantRoleId { get; set; }
        public int ConsultantQualityLevelId { get; set; }
        public int? ConsultantSeniorityId { get; set; }
        public string? RoleName { get; set; }
        public string? QualityLevelName { get; set; }
        public string? SeniorityName { get; set; }
        public decimal ConsultantMaximumAmount { get; set; }
        public decimal ClientRateMaximumAmount { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string? UpdatedByName { get; set; }
    }
}
