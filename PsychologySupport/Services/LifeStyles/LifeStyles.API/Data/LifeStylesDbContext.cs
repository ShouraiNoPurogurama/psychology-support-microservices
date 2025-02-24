using LifeStyles.API.Data.Common;
using LifeStyles.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LifeStyles.API.Data;

public class LifeStylesDbContext : DbContext
{
    public LifeStylesDbContext(DbContextOptions<LifeStylesDbContext> options) : base(options) { }

    public DbSet<PatientPhysicalActivity> PatientPhysicalActivities { get; set; }
    public DbSet<EntertainmentActivity> EntertainmentActivities { get; set; }
    public DbSet<TherapeuticActivity> TherapeuticActivities { get; set; }
    public DbSet<TherapeuticType> TherapeuticTypes { get; set; }
    public DbSet<PatientFoodActivity> PatientFoodActivities { get; set; }
    public DbSet<PatientEntertainmentActivity> PatientEntertainmentActivities { get; set; }
    public DbSet<PhysicalActivity> PhysicalActivities { get; set; }
    public DbSet<FoodActivity> FoodActivities { get; set; }
    public DbSet<FoodCategory> FoodCategories { get; set; }
    public DbSet<FoodNutrient> FoodNutrients { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("public");

     
        builder.Entity<PatientPhysicalActivity>()
            .Property(e => e.PreferenceLevel)
            .HasConversion(new EnumToStringConverter<PreferenceLevel>())
            .HasColumnType("VARCHAR(20)");

        builder.Entity<PatientFoodActivity>()
            .Property(e => e.PreferenceLevel)
            .HasConversion(new EnumToStringConverter<PreferenceLevel>())
            .HasColumnType("VARCHAR(20)");

        builder.Entity<PatientEntertainmentActivity>()
            .Property(e => e.PreferenceLevel)
            .HasConversion(new EnumToStringConverter<PreferenceLevel>())
            .HasColumnType("VARCHAR(20)");

        builder.Entity<PhysicalActivity>()
            .Property(e => e.IntensityLevel)
            .HasConversion(new EnumToStringConverter<IntensityLevel>())
            .HasColumnType("VARCHAR(20)");

        builder.Entity<FoodActivity>()
            .Property(e => e.IntensityLevel)
            .HasConversion(new EnumToStringConverter<IntensityLevel>())
            .HasColumnType("VARCHAR(20)");

        builder.Entity<EntertainmentActivity>()
            .Property(e => e.IntensityLevel)
            .HasConversion(new EnumToStringConverter<IntensityLevel>())
            .HasColumnType("VARCHAR(20)");

        builder.Entity<EntertainmentActivity>()
            .Property(e => e.ImpactLevel)
            .HasConversion(new EnumToStringConverter<ImpactLevel>())
            .HasColumnType("VARCHAR(20)");

        builder.Entity<TherapeuticActivity>()
            .Property(e => e.IntensityLevel)
            .HasConversion(new EnumToStringConverter<IntensityLevel>())
            .HasColumnType("VARCHAR(20)");

        builder.Entity<TherapeuticActivity>()
            .Property(e => e.ImpactLevel)
            .HasConversion(new EnumToStringConverter<ImpactLevel>())
            .HasColumnType("VARCHAR(20)");

        // Configure relationships (e.g., many-to-many, one-to-many)
        builder.Entity<PatientPhysicalActivity>()
            .HasKey(e => new { e.PatientProfileId, e.PhysicalActivityId });

        builder.Entity<PatientFoodActivity>()
            .HasKey(e => new { e.PatientProfileId, e.FoodActivityId });

        builder.Entity<PatientEntertainmentActivity>()
            .HasKey(e => new { e.PatientProfileId, e.EntertainmentActivityId });

        builder.Entity<TherapeuticActivity>()
            .HasOne(e => e.TherapeuticType)
            .WithMany()
            .HasForeignKey(e => e.TherapeuticTypeId);

        /* // Configure FoodActivity's foreign keys
    builder.Entity<FoodActivity>()
        .HasOne(e => e.FoodCategories)
        .WithMany()
        .HasForeignKey(e => e.Id);

    builder.Entity<FoodActivity>()
        .HasOne(e => e.FoodNutrients)
        .WithMany()
        .HasForeignKey(e => e.Id);*/

        // Add additional configurations for other entities if necessary
        base.OnModelCreating(builder);
    }
}