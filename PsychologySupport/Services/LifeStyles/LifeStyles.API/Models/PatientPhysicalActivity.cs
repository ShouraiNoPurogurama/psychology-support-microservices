using LifeStyles.API.Data.Common;

namespace LifeStyles.API.Models
{
    public class PatientPhysicalActivity
    {
        public Guid PatientProfileId { get; set; }
        public Guid PhysicalActivityId { get; set; }
        public PreferenceLevel PreferenceLevel { get; set; }
    }
}
