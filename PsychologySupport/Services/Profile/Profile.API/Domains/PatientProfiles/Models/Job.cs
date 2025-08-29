using BuildingBlocks.DDD;
using BuildingBlocks.Enums;

namespace Profile.API.Domains.PatientProfiles.Models
{
    public class Job : AuditableEntity<Guid>
    {
        public Guid IndustryId { get; set; }
        public string JobTitle { get; set; }
        public EducationLevel EducationLevel { get; set; }
        public Industry Industry { get; set; }
    }
}
