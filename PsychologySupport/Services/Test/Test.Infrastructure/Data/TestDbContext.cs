using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Test.Application.Data;
using Test.Domain.Models;
using Test.Domain.ValueObjects;

namespace Test.Infrastructure.Data;

public class TestDbContext : DbContext, ITestDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Domain.Models.Test> Tests => Set<Domain.Models.Test>();
    public DbSet<TestQuestion> TestQuestions => Set<TestQuestion>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<TestResult> TestResults => Set<TestResult>();
    // public DbSet<TestHistoryAnswer> TestHistoryAnswers => Set<TestHistoryAnswer>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        var scoreConverter = new ValueConverter<Score, int>(
            score => score.Value,
            value => new Score(value)
        );

        builder.Entity<TestResult>()
            .Property(tr => tr.DepressionScore)
            .HasConversion(scoreConverter);

        builder.Entity<TestResult>()
            .Property(tr => tr.AnxietyScore)
            .HasConversion(scoreConverter);

        builder.Entity<TestResult>()
            .Property(tr => tr.StressScore)
            .HasConversion(scoreConverter);


        base.OnModelCreating(builder);
    }
}