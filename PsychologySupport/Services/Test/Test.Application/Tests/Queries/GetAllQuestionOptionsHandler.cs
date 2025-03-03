using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Test.Application.Data;
using Test.Domain.Models;

namespace Test.Application.Tests.Queries;

public record GetAllQuestionOptionsQuery(Guid QuestionId, PaginationRequest PaginationRequest)
    : IQuery<GetAllQuestionOptionsResult>;

public record GetAllQuestionOptionsResult(PaginatedResult<QuestionOption> QuestionOptions);

public class GetAllQuestionOptionsHandler
    : IQueryHandler<GetAllQuestionOptionsQuery, GetAllQuestionOptionsResult>
{
    private readonly ITestDbContext _context;

    public GetAllQuestionOptionsHandler(ITestDbContext context)
    {
        _context = context;
    }

    public async Task<GetAllQuestionOptionsResult> Handle(
        GetAllQuestionOptionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.QuestionOptions
            .AsNoTracking()
            .Where(o => o.QuestionId == request.QuestionId)
            .OrderBy(o => o.Id);

        var totalCount = await query.CountAsync(cancellationToken);

        var questionOptions = await query
            .Skip(request.PaginationRequest.PageIndex * request.PaginationRequest.PageSize)
            .Take(request.PaginationRequest.PageSize)
            .ToListAsync(cancellationToken);

        var paginatedResult = new PaginatedResult<QuestionOption>(
            request.PaginationRequest.PageIndex,
            request.PaginationRequest.PageSize,
            totalCount,
            questionOptions
        );

        return new GetAllQuestionOptionsResult(paginatedResult);
    }
}