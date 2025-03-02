using LifeStyles.API.Data.Common;
using LifeStyles.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LifeStyles.API.Data;

public class LifeStylesDbContext : DbContext
{
    public LifeStylesDbContext(DbContextOptions<LifeStylesDbContext> options) : base(options)
    {
        
    }

    public DbSet<PatientPhysicalActivity> PatientPhysicalActivities => Set<PatientPhysicalActivity>();
    public DbSet<EntertainmentActivity> EntertainmentActivities => Set<EntertainmentActivity>();
    public DbSet<TherapeuticActivity> TherapeuticActivities  => Set<TherapeuticActivity>();
    public DbSet<TherapeuticType> TherapeuticTypes => Set<TherapeuticType>();
    public DbSet<PatientEntertainmentActivity> PatientEntertainmentActivities => Set<PatientEntertainmentActivity>();
    public DbSet<PhysicalActivity> PhysicalActivities => Set<PhysicalActivity>();
    public DbSet<FoodActivity> FoodActivities => Set<FoodActivity>();
    public DbSet<FoodCategory> FoodCategories => Set<FoodCategory>();
    public DbSet<FoodNutrient> FoodNutrients => Set<FoodNutrient>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("public");
        
        builder.Entity<PatientPhysicalActivity>()
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


        builder.Entity<PatientEntertainmentActivity>()
            .HasKey(e => new { e.PatientProfileId, e.EntertainmentActivityId });

        builder.Entity<TherapeuticActivity>()
            .HasOne(e => e.TherapeuticType)
            .WithMany()
            .HasForeignKey(e => e.TherapeuticTypeId);

        base.OnModelCreating(builder);
    }
}