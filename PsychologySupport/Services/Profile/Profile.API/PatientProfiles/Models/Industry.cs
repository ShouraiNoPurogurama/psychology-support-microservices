using BuildingBlocks.DDD;
using Newtonsoft.Json;

namespace Profile.API.PatientProfiles.Models
{
    public class Industry : AuditableEntity<Guid>
    {
        public string IndustryName { get; set; }
        public string Description { get; set; }

        
        private readonly List<Job> _jobs = [];
        [JsonIgnore]
        public IReadOnlyCollection<Job> Jobs => _jobs.AsReadOnly();
    }
}
