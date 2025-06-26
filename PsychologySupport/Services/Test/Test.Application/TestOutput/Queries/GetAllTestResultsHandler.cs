using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Test.Application.Data;
using Test.Application.Dtos;
using Test.Domain.Enums;
using Test.Domain.Models;
using Test.Domain.ValueObjects;

namespace Test.Application.TestOutput.Queries;

public record GetAllTestResultsQuery(
    Guid PatientId,
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

    public GetAllTestResultsHandler(ITestDbContext context)
    {
        _context = context;
    }

    public async Task<GetAllTestResultsResult> Handle(
        GetAllTestResultsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TestResults
            .AsNoTracking()
            .Where(tr => tr.PatientId == request.PatientId);

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
