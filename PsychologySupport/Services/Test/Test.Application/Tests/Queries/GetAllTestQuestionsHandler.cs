using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Test.Application.Data;
using Test.Domain.Models;

namespace Test.Application.Tests.Queries
{
    public record GetAllTestQuestionsQuery(Guid TestId) : IQuery<IEnumerable<TestQuestion>>;
    public class GetAllTestQuestionsHandler : IQueryHandler<GetAllTestQuestionsQuery, IEnumerable<TestQuestion>>
    {
        private readonly ITestDbContext _context;

        public GetAllTestQuestionsHandler(ITestDbContext context) => _context = context;

        public async Task<IEnumerable<TestQuestion>> Handle(GetAllTestQuestionsQuery request, CancellationToken cancellationToken)
        {
            return await _context.TestQuestions.AsNoTracking()
                .Where(q => q.TestId == request.TestId)
                .OrderBy(q => q.Order)
                .ToListAsync(cancellationToken);
        }
    }

}
