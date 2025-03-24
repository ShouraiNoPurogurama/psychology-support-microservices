using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Test.Application.Data;
using Test.Application.Dtos;
using Test.Domain.Models;

namespace Test.Application.Tests.Queries;

public record GetAllTestQuestionsQuery(Guid TestId, PaginationRequest PaginationRequest) : IQuery<GetAllTestQuestionsResult>;

public record GetAllTestQuestionsResult(PaginatedResult<TestQuestionDto> TestQuestions);

public class GetAllTestQuestionsHandler : IQueryHandler<GetAllTestQuestionsQuery, GetAllTestQuestionsResult>
{
    private readonly ITestDbContext _context;

    public GetAllTestQuestionsHandler(ITestDbContext context)
    {
        _context = context;
    }

    public async Task<GetAllTestQuestionsResult> Handle(GetAllTestQuestionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TestQuestions.AsNoTracking()
            .Where(q => q.TestId == request.TestId)
            .OrderBy(q => q.Order)
            .ProjectToType<TestQuestionDto>();

        var totalCount = await query.CountAsync(cancellationToken);

        var testQuestions = await query
            .Skip(request.PaginationRequest.PageIndex * request.PaginationRequest.PageSize)
            .Take(request.PaginationRequest.PageSize)
            .ToListAsync(cancellationToken);

        var paginatedResult = new PaginatedResult<TestQuestionDto>(
            request.PaginationRequest.PageIndex,
            request.PaginationRequest.PageSize,
            totalCount,
            testQuestions
        );

        return new GetAllTestQuestionsResult(paginatedResult);
    }
}