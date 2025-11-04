using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Test.Application.Data;
using Test.Application.Dtos;
using Test.Application.ServiceContracts;
using Test.Domain.Enums;
using Test.Domain.ValueObjects;

namespace Test.Application.Tests.Queries;

public record GetAllTestResultsQuery(
    int PageIndex,
    int PageSize,
    string? Search = "", // TestId
    string? SortBy = "TakenAt", // Sort TakenAt
    string? SortOrder = "asc", // Asc or Desc
    SeverityLevel? SeverityLevel = null // Filter by SeverityLevel
) : IQuery<GetAllTestResultsResult>;

public record GetAllTestResultsResult(PaginatedResult<GetAllTestResultDto> TestResults);

public class GetAllTestResultsHandler
    : IQueryHandler<GetAllTestResultsQuery, GetAllTestResultsResult>
{
    private readonly ITestDbContext _context;
    private readonly ICurrentActorAccessor _currentActorAccessor;

    public GetAllTestResultsHandler(ITestDbContext context, ICurrentActorAccessor currentActorAccessor)
    {
        _context = context;
        _currentActorAccessor = currentActorAccessor;
    }

    public async Task<GetAllTestResultsResult> Handle(
        GetAllTestResultsQuery request, CancellationToken cancellationToken)
    {
        var patientId = _currentActorAccessor.GetRequiredPatientId();
        
        var query = _context.TestResults
            .AsNoTracking()
            .Where(tr => tr.PatientId == patientId);

        // Apply Search by TestId
        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(tr => tr.Id.ToString().Contains(request.Search));
        }

        // Filtering 
        if (request.SeverityLevel.HasValue)
        {
            query = query.Where(tr => tr.SeverityLevel == request.SeverityLevel);
        }

        // Sorting
        if (request.SortBy == "TakenAt")
        {
            query = request.SortOrder == "asc"
                ? query.OrderBy(tr => tr.TakenAt)
                : query.OrderByDescending(tr => tr.TakenAt);
        }


        var dtoList = await query
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .Select(tr => new GetAllTestResultDto(
                tr.Id,
                tr.TestId,
                tr.PatientId,
                tr.TakenAt,
                tr.SeverityLevel,
                tr.DepressionScore,
                tr.AnxietyScore,
                tr.StressScore,
                Recommendation.FromJson(EF.Property<string>(tr, "RecommendationJson"))
            ))
            .ToListAsync(cancellationToken);

        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var paginatedResult = new PaginatedResult<GetAllTestResultDto>(
            request.PageIndex,
            request.PageSize,
            totalCount,
            dtoList
        );

        return new GetAllTestResultsResult(paginatedResult);
    }
}
