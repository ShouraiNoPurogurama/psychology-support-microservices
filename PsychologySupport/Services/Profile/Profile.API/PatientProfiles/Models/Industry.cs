using BuildingBlocks.DDD;

namespace Profile.API.PatientProfiles.Models
{
    public class Industry : Entity<Guid>
    {
        public string IndustryName { get; set; }
        public string Description { get; set; }

        private readonly List<Job> _jobs = [];
        public IReadOnlyCollection<Job> Jobs => _jobs.AsReadOnly();
    }
}
