using BuildingBlocks.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Test.Application.Data;
using Test.Domain.Events;
using Test.Domain.Models;

namespace Test.Application.TestOutput.EventHandlers.Domain;

public class TestResultCreatedEventHandler : INotificationHandler<TestResultCreatedEvent>
{
    private readonly ITestDbContext _dbContext;

    public TestResultCreatedEventHandler(ITestDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(TestResultCreatedEvent notification, CancellationToken cancellationToken)
    {
        // TestResult testResult = await _dbContext.TestResults
        //                             .FindAsync([notification.TestResultId], cancellationToken)
        //                         ?? throw new NotFoundException("Test Result", notification.TestResultId);
        //
        // List<QuestionOption> testResultOptions = await _dbContext.QuestionOptions
        //     .Where(o => notification.SelectedOptionIds.Contains(o.Id))
        //     .ToListAsync(cancellationToken);
        //
        // testResult.AddSelectedOptions(testResultOptions);
        //
        // await _dbContext.SaveChangesAsync(cancellationToken);
    }
}