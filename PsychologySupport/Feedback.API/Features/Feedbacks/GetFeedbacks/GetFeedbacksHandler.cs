using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Feedback.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Blog.API.Features.Feedbacks.GetFeedbacks;

public record GetFeedbacksQuery(PaginationRequest Pagination) : IQuery<GetFeedbacksResult>;

public record GetFeedbacksResult(PaginatedResult<Feedback.API.Models.Feedback> Feedbacks);

public class GetFeedbacksHandler : IQueryHandler<GetFeedbacksQuery, GetFeedbacksResult>
{
    private readonly FeedbackDbContext _dbContext;

    public GetFeedbacksHandler(FeedbackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetFeedbacksResult> Handle(GetFeedbacksQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.Pagination.PageIndex - 1) * request.Pagination.PageSize;

        var totalCount = await _dbContext.Feedbacks.CountAsync(cancellationToken);

        var feedbacks = await _dbContext.Feedbacks
            .Skip(skip)
            .Take(request.Pagination.PageSize)
            .ToListAsync(cancellationToken);

        var paginatedResult = new PaginatedResult<Feedback.API.Models.Feedback>(
            request.Pagination.PageIndex,
            request.Pagination.PageSize,
            totalCount,
            feedbacks
        );

        return new GetFeedbacksResult(paginatedResult);
    }
}
