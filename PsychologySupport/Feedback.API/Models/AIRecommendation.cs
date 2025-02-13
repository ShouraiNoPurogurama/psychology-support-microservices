using Newtonsoft.Json;

namespace Feedback.API.Models
{ 
    public class AIRecommendation
    {
        public Guid Id { get; set; }

        public Guid TestResultId { get; set; }

        public Guid ScheduleId { get; set; }

        public string Recommendation { get; set; } 
        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; } 

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
