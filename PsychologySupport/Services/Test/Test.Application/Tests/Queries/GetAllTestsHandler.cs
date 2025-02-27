using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Test.Application.Data;
using Test.Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Test.Application.Tests.Queries
{
    public record GetAllTestsQuery(PaginationRequest PaginationRequest) : IQuery<GetAllTestsResult>;
    public record GetAllTestsResult(IEnumerable<TestDto> Tests);

    public class GetAllTestsHandler : IQueryHandler<GetAllTestsQuery, GetAllTestsResult>
    {
        private readonly ITestDbContext _context;

        public GetAllTestsHandler(ITestDbContext context)
        {
            _context = context;
        }

        public async Task<GetAllTestsResult> Handle(GetAllTestsQuery request, CancellationToken cancellationToken)
        {
            var pageSize = request.PaginationRequest.PageSize;
            var pageIndex = request.PaginationRequest.PageIndex;

            var tests = await _context.Tests
                .Join(_context.Categories,  
                    test => test.CategoryId,  
                    category => category.Id,  
                    (test, category) => new TestDto(
                        test.Id,
                        category.Description,
                        category.Name
                    ))
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new GetAllTestsResult(tests);
        }
    }
}
