using BuildingBlocks.Enums;
using LifeStyles.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection.Emit;
using Emotion = LifeStyles.API.Models.Emotion;

namespace LifeStyles.API.Data;

public class LifeStylesDbContext : DbContext
{
    public LifeStylesDbContext(DbContextOptions<LifeStylesDbContext> options) : base(options)
    {
    }

    public DbSet<PatientPhysicalActivity> PatientPhysicalActivities => Set<PatientPhysicalActivity>();
    public DbSet<EntertainmentActivity> EntertainmentActivities => Set<EntertainmentActivity>();
    public DbSet<TherapeuticActivity> TherapeuticActivities => Set<TherapeuticActivity>();
    public DbSet<TherapeuticType> TherapeuticTypes => Set<TherapeuticType>();
    public DbSet<PatientEntertainmentActivity> PatientEntertainmentActivities => Set<PatientEntertainmentActivity>();
    public DbSet<PatientFoodActivity> PatientFoodActivities => Set<PatientFoodActivity>();
    public DbSet<PatientTherapeuticActivity> PatientTherapeuticActivities => Set<PatientTherapeuticActivity>();
    public DbSet<PhysicalActivity> PhysicalActivities => Set<PhysicalActivity>();
    public DbSet<FoodActivity> FoodActivities => Set<FoodActivity>();
    public DbSet<FoodCategory> FoodCategories => Set<FoodCategory>();
    public DbSet<FoodNutrient> FoodNutrients => Set<FoodNutrient>();
    public DbSet<LifestyleLog> LifestyleLogs { get; set; }
    public DbSet<ImprovementGoal> ImprovementGoals { get; set; }
    public DbSet<PatientImprovementGoal> PatientImprovementGoals { get; set; }
    
    public DbSet<PatientEmotionCheckpoint> PatientEmotionCheckpoints { get; set; }
    
    public DbSet<Emotion> Emotions { get; set; }
    
    public DbSet<EmotionSelection> EmotionSelections { get; set; }

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
        builder.Entity<PatientTherapeuticActivity>()
            .Property(e => e.PreferenceLevel)
            .HasConversion(new EnumToStringConverter<PreferenceLevel>())
            .HasColumnType("VARCHAR(20)");
        builder.Entity<PatientFoodActivity>()
            .Property(e => e.PreferenceLevel)
            .HasConversion(new EnumToStringConverter<PreferenceLevel>())
            .HasColumnType("VARCHAR(20)");

        builder.Entity<PhysicalActivity>()
            .Property(e => e.IntensityLevel)
            .HasConversion(new EnumToStringConverter<IntensityLevel>())
            .HasColumnType("VARCHAR(20)");

        // builder.Entity<PhysicalActivity>()
        //     .Property(e => e.ImpactLevel)
        //     .HasConversion(new EnumToStringConverter<ImpactLevel>())
        //     .HasColumnType("VARCHAR(20)");
        builder.Entity<PhysicalActivity>()
            .Property(e => e.ImpactLevel)
            .HasConversion(t => t.ToString(),
                dbStatus => (ImpactLevel)Enum.Parse(typeof(ImpactLevel), dbStatus))
            .HasColumnType("VARCHAR(20)");

        builder.Entity<FoodActivity>()
            .Property(e => e.IntensityLevel)
            .HasConversion(new EnumToStringConverter<IntensityLevel>())
            .HasColumnType("VARCHAR(20)");

        builder.Entity<FoodActivity>()
            .Property(e => e.MealTime)
            .HasConversion(new EnumToStringConverter<MealTime>())
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

        builder.Entity<PatientTherapeuticActivity>()
            .HasKey(e => new { e.PatientProfileId, e.TherapeuticActivityId });
        builder.Entity<PatientFoodActivity>()
            .HasKey(e => new { e.PatientProfileId, e.FoodActivityId });

        builder.Entity<TherapeuticActivity>()
            .HasOne(e => e.TherapeuticType)
            .WithMany()
            .HasForeignKey(e => e.TherapeuticTypeId);

        builder.Entity<PatientImprovementGoal>()
            .HasKey(p => new { p.PatientProfileId, p.GoalId });

        builder.Entity<PatientImprovementGoal>()
            .HasKey(p => new { p.PatientProfileId, p.GoalId });

        builder.Entity<PatientImprovementGoal>()
            .HasOne(p => p.Goal)
            .WithMany(g => g.PatientImprovementGoals) 
            .HasForeignKey(p => p.GoalId)
            .OnDelete(DeleteBehavior.Cascade); 

        builder.Entity<Emotion>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<EmotionType>(v)
                )
                .HasMaxLength(50);

            entity.Property(e => e.Description)
                .HasMaxLength(200);
            
            entity.Property(e => e.IconUrl)
                .HasMaxLength(255);
        });


        builder.Entity<PatientEmotionCheckpoint>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.LogDate)
                .IsRequired();

            entity.HasMany(c => c.EmotionSelections)
                .WithOne()
                .HasForeignKey(e => e.EmotionCheckpointId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<EmotionSelection>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Intensity);
            
            entity.Property(e => e.Rank);

            entity.HasOne(e => e.Emotion)
                .WithMany(e => e.EmotionSelections)
                .HasForeignKey(e => e.EmotionId)
                .OnDelete(DeleteBehavior.Restrict); //Prevent deleting Emotion if used
            
            entity.HasOne(es => es.EmotionCheckpoint)
                .WithMany(ec => ec.EmotionSelections)
                .HasForeignKey(es => es.EmotionCheckpointId);
        });

        builder.Entity<LifestyleLog>()
            .Property(e => e.SleepHours)
            .HasConversion(new EnumToStringConverter<SleepHoursLevel>())
            .HasColumnType("VARCHAR(20)");

        builder.Entity<LifestyleLog>()
            .Property(e => e.ExerciseFrequency)
            .HasConversion(new EnumToStringConverter<ExerciseFrequency>())
            .HasColumnType("VARCHAR(20)");

        builder.Entity<LifestyleLog>()
            .Property(e => e.AvailableTimePerDay)
            .HasConversion(new EnumToStringConverter<AvailableTimePerDay>())
            .HasColumnType("VARCHAR(20)");


        base.OnModelCreating(builder);
    }
}