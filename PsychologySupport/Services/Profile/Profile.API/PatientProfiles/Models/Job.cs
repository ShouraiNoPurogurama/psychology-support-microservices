using BuildingBlocks.DDD;
using BuildingBlocks.Enums;

namespace Profile.API.PatientProfiles.Models
{
    public class Job : Entity<Guid>
    {
        public Guid IndustryId { get; set; }
        public string JobTitle { get; set; }
        public EducationLevel EducationLevel { get; set; }
        public Industry Industry { get; set; }
    }
}
