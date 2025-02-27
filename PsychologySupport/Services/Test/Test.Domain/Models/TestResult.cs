using BuildingBlocks.DDD;
using Test.Domain.Enums;
using Test.Domain.ValueObjects;

namespace Test.Domain.Models
{
    public class TestResult : AggregateRoot<Guid>
    {
        public Guid PatientId { get; private set; }
        public Guid TestId { get; private set; }
        public DateTime TakenAt { get; private set; }
        public Score DepressionScore { get; private set; }
        public Score AnxietyScore { get; private set; }
        public Score StressScore { get; private set; }
        public SeverityLevel SeverityLevel { get; private set; }
        public string Recommendation { get; private set; }

        private readonly List<TestHistoryAnswer> _historyAnswers = [];
        public IReadOnlyCollection<TestHistoryAnswer> HistoryAnswers => _historyAnswers.AsReadOnly();

        private TestResult() { }

        public TestResult(Guid id, Guid patientId, Guid testId, Score depressionScore,
            Score anxietyScore, Score stressScore, SeverityLevel severityLevel, string recommendation)
        {
            Id = id;
            PatientId = patientId;
            TestId = testId;
            TakenAt = DateTime.UtcNow;
            DepressionScore = depressionScore;
            AnxietyScore = anxietyScore;
            StressScore = stressScore;
            SeverityLevel = severityLevel;
            Recommendation = recommendation;
        }

        public static TestResult Create(Guid patientId, Guid testId, Score depressionScore,
                                        Score anxietyScore, Score stressScore, SeverityLevel severityLevel, 
                                        string recommendation, List<Guid> selectedOptionIds)
        {
            var testResult = new TestResult(Guid.NewGuid(), patientId, testId, depressionScore, anxietyScore, stressScore, severityLevel, recommendation);

            foreach (var optionId in selectedOptionIds)
            {
                var historyAnswer = TestHistoryAnswer.Create(testResult.Id, optionId);
                testResult._historyAnswers.Add(historyAnswer);
            }

            return testResult;
        }

    }
}
