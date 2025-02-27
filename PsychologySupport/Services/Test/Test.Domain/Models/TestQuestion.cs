using BuildingBlocks.DDD;

namespace Test.Domain.Models
{
    public class TestQuestion : Entity<Guid>
    {
        public int Order { get; private set; }
        public string Content { get; private set; }
        public Guid TestId { get; private set; }

        private readonly List<QuestionOption> _options = new();
        public IReadOnlyCollection<QuestionOption> Options => _options.AsReadOnly();

        private TestQuestion() { }

        public TestQuestion(Guid id, int order, string content, Guid testId)
        {
            Id = id;
            Order = order;
            Content = content;
            TestId = testId;
        }
    }

}
