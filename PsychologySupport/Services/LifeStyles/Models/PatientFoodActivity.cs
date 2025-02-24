using LifeStyles.API.Data.Common;

namespace LifeStyles.API.Models
{
    public class PatientFoodActivity
    {
        public Guid PatientProfileId { get; set; }
        public Guid FoodActivityId { get; set; }
        public PreferenceLevel PreferenceLevel { get; set; }
    }
}
