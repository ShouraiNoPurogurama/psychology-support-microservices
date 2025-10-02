using BuildingBlocks.DDD;

namespace Test.Domain.Models;

public class Test : AggregateRoot<Guid>
{
    private readonly List<TestQuestion> _questions = new();
    
    private readonly List<TestResult> _testResults = new();

    private Test()
    {
    }

    public Test(Guid id, Guid categoryId, string createdBy)
    {
        Id = id;
        CategoryId = categoryId;
        CreatedAt = DateTimeOffset.UtcNow;
        LastModified = DateTimeOffset.UtcNow;
        CreatedBy = createdBy;
        LastModifiedBy = createdBy;
    }

    public Guid CategoryId { get; private set; }
    public IReadOnlyCollection<TestQuestion> Questions => _questions.AsReadOnly();

    public IReadOnlyCollection<TestResult> TestResults { get; private set; } = new List<TestResult>();
    
    public void Update(string lastModifiedBy)
    {
        LastModified = DateTimeOffset.UtcNow;
        LastModifiedBy = lastModifiedBy;
    }
    
    
}