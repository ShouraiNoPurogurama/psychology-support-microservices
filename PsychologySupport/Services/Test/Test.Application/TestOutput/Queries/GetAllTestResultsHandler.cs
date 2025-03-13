using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Test.Application.Data;
using Test.Domain.Enums;
using Test.Domain.Models;

namespace Test.Application.TestOutput.Queries;

public record GetAllTestResultsQuery(
    [FromRoute] Guid PatientId,
    [FromQuery] int PageIndex,
    [FromQuery] int PageSize,
    [FromQuery] string? Search = "", // TestId
    [FromQuery] string? SortBy = "TakenAt", // Sort TakenAt
    [FromQuery] string? SortOrder = "asc", // Asc or Desc
    [FromQuery] SeverityLevel? SeverityLevel = null // Filter by SeverityLevel
) : IQuery<GetAllTestResultsResult>;

public record GetAllTestResultsResult(PaginatedResult<TestResult> TestResults);

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


        var totalCount = await query.CountAsync(cancellationToken);
        var testResults = await query
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var paginatedResult = new PaginatedResult<TestResult>(
            request.PageIndex,
            request.PageSize,
            totalCount,
            testResults
        );

        return new GetAllTestResultsResult(paginatedResult);
    }
}
