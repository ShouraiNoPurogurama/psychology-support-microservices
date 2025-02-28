using BuildingBlocks.DDD;

namespace Test.Domain.Models;

public class TestHistoryAnswer : Entity<Guid>
{
    private TestHistoryAnswer()
    {
    }

    public TestHistoryAnswer(Guid id, Guid testResultId, Guid selectedOptionId)
    {
        Id = id;
        TestResultId = testResultId;
        SelectedOptionId = selectedOptionId;
    }

    public Guid TestResultId { get; private set; }
    public Guid SelectedOptionId { get; private set; }

    public static TestHistoryAnswer Create(Guid testResultId, Guid selectedOptionId)
    {
        return new TestHistoryAnswer(Guid.NewGuid(), testResultId, selectedOptionId);
    }
}