using BuildingBlocks.DDD;
using Newtonsoft.Json;

namespace Feedback.API.Models
{ 
    public class AIRecommendation : Entity<Guid>
    {
        public Guid TestResultId { get; set; }

        public Guid ScheduleId { get; set; }

        public string Recommendation { get; set; } 

        // Convert JSON to Object
        public RecommendationDetails? GetRecommendation()
        {
            return JsonConvert.DeserializeObject<RecommendationDetails>(Recommendation);
        }

        // Convert Object to JSON
        public void SetRecommendation(RecommendationDetails data)
        {
            Recommendation = JsonConvert.SerializeObject(data);
        }
    }
}
