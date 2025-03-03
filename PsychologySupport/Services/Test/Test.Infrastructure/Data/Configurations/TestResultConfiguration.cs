using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Test.Domain.Models;

namespace Test.Infrastructure.Data.Configurations;

public class TestResultConfiguration : IEntityTypeConfiguration<TestResult>
{
    public void Configure(EntityTypeBuilder<TestResult> builder)
    {
        builder.ToTable("TestResults");

        // Chuyển Enum thành string
        builder.Property(tr => tr.SeverityLevel)
            .HasConversion<string>();

        builder.Property(tr => tr.TakenAt)
            .HasColumnType("TIMESTAMPTZ");

        builder.Property(tr => tr.Recommendation)
            .HasColumnType("TEXT");

        builder.HasOne<Domain.Models.Test>()
            .WithMany()
            .HasForeignKey(tr => tr.TestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}