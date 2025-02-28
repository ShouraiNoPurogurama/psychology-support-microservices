using Microsoft.EntityFrameworkCore;
using Test.Domain.Models;

namespace Test.Application.Data;

public interface ITestDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Domain.Models.Test> Tests { get; }
    DbSet<TestQuestion> TestQuestions { get; }
    DbSet<QuestionOption> QuestionOptions { get; }
    DbSet<TestResult> TestResults { get; }
    DbSet<TestHistoryAnswer> TestHistoryAnswers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}