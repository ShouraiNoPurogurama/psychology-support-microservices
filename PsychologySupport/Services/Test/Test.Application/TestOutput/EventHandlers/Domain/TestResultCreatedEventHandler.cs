using MediatR;
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
        var testHistoryAnswers = notification.SelectedOptionIds
            .Select(optionId => TestHistoryAnswer.Create(notification.TestResultId, optionId))
            .ToList();

        await _dbContext.TestHistoryAnswers.AddRangeAsync(testHistoryAnswers, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}