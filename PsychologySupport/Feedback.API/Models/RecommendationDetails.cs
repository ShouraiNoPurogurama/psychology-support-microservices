namespace Feedback.API.Models
{
    public class RecommendationDetails
    {
        public DetailedScores Scores { get; set; } = new DetailedScores();
        public ClassificationLevels Classification { get; set; } = new ClassificationLevels();
        public string OverallAssessment { get; set; } = string.Empty;
    }

    public class DetailedScores
    {
        public int AnxietyScore { get; set; }
        public int DepressionScore { get; set; }
        public int StressScore { get; set; }
    }

    public class ClassificationLevels
    {
        public string Anxiety { get; set; } = string.Empty;
        public string Depression { get; set; } = string.Empty;
        public string Stress { get; set; } = string.Empty;
    }

}
