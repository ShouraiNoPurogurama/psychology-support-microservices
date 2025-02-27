using BuildingBlocks.DDD;

namespace Test.Domain.Models
{
    public class Test : AggregateRoot<Guid>
    {
        public Guid CategoryId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; private set; }
        public string CreatedBy { get; private set; }
        public string LastModifiedBy { get; private set; }

        private readonly List<TestQuestion> _questions = new();
        public IReadOnlyCollection<TestQuestion> Questions => _questions.AsReadOnly();

        private Test() { }

        public Test(Guid id, Guid categoryId, string createdBy)
        {
            Id = id;    
            CategoryId = categoryId;
            CreatedAt = DateTime.UtcNow;
            LastModified = DateTime.UtcNow;
            CreatedBy = createdBy;
            LastModifiedBy = createdBy;
        }

        public void Update(string lastModifiedBy)
        {
            LastModified = DateTime.UtcNow;
            LastModifiedBy = lastModifiedBy;
        }
    }
}
