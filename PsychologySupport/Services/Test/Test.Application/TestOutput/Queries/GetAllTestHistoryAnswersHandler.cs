using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Test.Application.Data;
using Test.Domain.Models;

namespace Test.Application.TestOutput.Queries
{
    public record GetAllTestHistoryAnswersQuery(Guid TestResultId, PaginationRequest PaginationRequest)
        : IQuery<GetAllTestHistoryAnswersResult>;

    public record GetAllTestHistoryAnswersResult(PaginatedResult<TestHistoryAnswer> Answers);

    public class GetAllTestHistoryAnswersHandler
        : IQueryHandler<GetAllTestHistoryAnswersQuery, GetAllTestHistoryAnswersResult>
    {
        private readonly ITestDbContext _context;

        public GetAllTestHistoryAnswersHandler(ITestDbContext context) => _context = context;

        public async Task<GetAllTestHistoryAnswersResult> Handle(
            GetAllTestHistoryAnswersQuery request, CancellationToken cancellationToken)
        {
            var query = _context.TestHistoryAnswers
                .AsNoTracking()
                .Where(t => t.TestResultId == request.TestResultId)
                .OrderBy(t => t.Id); 

            var totalCount = await query.CountAsync(cancellationToken);

            var answers = await query
                .Skip(request.PaginationRequest.PageIndex * request.PaginationRequest.PageSize)
                .Take(request.PaginationRequest.PageSize)
                .ToListAsync(cancellationToken);

            var paginatedResult = new PaginatedResult<TestHistoryAnswer>(
                request.PaginationRequest.PageIndex,
                request.PaginationRequest.PageSize,
                totalCount,
                answers
            );

            return new GetAllTestHistoryAnswersResult(paginatedResult);
        }
    }
}
