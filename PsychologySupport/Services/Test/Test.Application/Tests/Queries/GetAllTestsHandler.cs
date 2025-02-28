using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Test.Application.Data;
using Test.Application.Dtos;

namespace Test.Application.Tests.Queries;

public record GetAllTestsQuery(PaginationRequest PaginationRequest)
    : IQuery<GetAllTestsResult>;

public record GetAllTestsResult(PaginatedResult<TestDto> Tests);

public class GetAllTestsHandler : IQueryHandler<GetAllTestsQuery, GetAllTestsResult>
{
    private readonly ITestDbContext _context;

    public GetAllTestsHandler(ITestDbContext context)
    {
        _context = context;
    }

    public async Task<GetAllTestsResult> Handle(
        GetAllTestsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PaginationRequest.PageSize;
        var pageIndex = Math.Max(0, request.PaginationRequest.PageIndex - 1);

        var query = _context.Tests
            .AsNoTracking()
            .Join(_context.Categories,
                test => test.CategoryId,
                category => category.Id,
                (test, category) => new TestDto(
                    test.Id,
                    category.Description,
                    category.Name
                ));

        var totalCount = await query.CountAsync(cancellationToken);

        var tests = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var paginatedResult = new PaginatedResult<TestDto>(
            pageIndex + 1,
            pageSize,
            totalCount,
            tests
        );

        return new GetAllTestsResult(paginatedResult);
    }
}