using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Test.Application.Data;
using Test.Application.Dtos;

namespace Test.Application.TestOutput.Queries;

public record GetAllTestHistoryAnswersQuery(Guid TestResultId)
    : IQuery<GetAllTestHistoryAnswersResult>;

public record GetAllTestHistoryAnswersResult(TestResultOptionsDto Answer);

public class GetAllTestHistoryAnswersHandler
    : IQueryHandler<GetAllTestHistoryAnswersQuery, GetAllTestHistoryAnswersResult>
{
    private readonly ITestDbContext _context;

    public GetAllTestHistoryAnswersHandler(ITestDbContext context)
    {
        _context = context;
    }
    
    public async Task<GetAllTestHistoryAnswersResult> Handle(
        GetAllTestHistoryAnswersQuery request, CancellationToken cancellationToken)
    {
        var testResult = await _context.TestResults
                             .Include(t => t.SelectedOptions)
                             .FirstOrDefaultAsync(t => t.Id == request.TestResultId, cancellationToken)
                         ?? throw new NotFoundException("Test result not found");

        var testQuestionIds = testResult.SelectedOptions
            .Select(o => o.QuestionId)
            .Distinct()
            .ToList();

        var testQuestions = await _context.TestQuestions
            .Where(q => testQuestionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id, cancellationToken); //Store as dictionary for fast lookups

        var selectedOptions = testResult.SelectedOptions
            .Select(option => new SelectedOptionDto(
                testQuestions[option.QuestionId].Adapt<TestQuestionDto>(),
                option.Adapt<QuestionOptionDto>()
            ))
            .ToList();

        return new GetAllTestHistoryAnswersResult(
            new TestResultOptionsDto(testResult.Adapt<TestResultDto>(), selectedOptions));
    }
}