using BuildingBlocks.DDD;

namespace Profile.API.Models.Public
{
    public class Job : AuditableEntity<Guid>
    {
        public Guid IndustryId { get; set; }
        public string JobTitle { get; set; }
        public EducationLevel EducationLevel { get; set; }
        public Industry Industry { get; set; }
    }
}
