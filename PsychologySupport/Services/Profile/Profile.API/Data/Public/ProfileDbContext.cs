using BuildingBlocks.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Profile.API.Domains.DoctorProfiles.Models;
using Profile.API.Domains.MentalDisorders.Models;
using Profile.API.Domains.PatientProfiles.Enum;
using Profile.API.Domains.PatientProfiles.Models;

namespace Profile.API.Data.Public;

public class ProfileDbContext : DbContext
{
    public ProfileDbContext(DbContextOptions<ProfileDbContext> options) : base(options)
    {
    }

    public DbSet<PatientProfile> PatientProfiles => Set<PatientProfile>();
    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
    public DbSet<MedicalHistory> MedicalHistories => Set<MedicalHistory>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<MentalDisorder> MentalDisorders => Set<MentalDisorder>();
    public DbSet<SpecificMentalDisorder> SpecificMentalDisorders => Set<SpecificMentalDisorder>();
    public DbSet<PhysicalSymptom> PhysicalSymptoms => Set<PhysicalSymptom>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<Industry> Industries => Set<Industry>();
    public DbSet<Job> Jobs => Set<Job>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("public");

        builder.Entity<PatientProfile>(typeBuilder =>
        {
            typeBuilder.Property(d => d.PersonalityTraits)
                .HasConversion(t => t.ToString(),
                    dbStatus => (PersonalityTrait)Enum.Parse(typeof(PersonalityTrait), dbStatus));

            typeBuilder.HasOne(p => p.MedicalHistory)
                .WithOne()
                .HasForeignKey<MedicalHistory>(m => m.PatientId);
        });


        builder.Entity<DoctorProfile>(typeBuilder =>
        {
            typeBuilder.Property(d => d.Gender)
                .HasConversion(t => t.ToString(),
                    dbStatus => (UserGender)Enum.Parse(typeof(UserGender), dbStatus))
                .HasColumnName("gender");

            typeBuilder.ComplexProperty(d => d.ContactInfo, contactInfoBuilder =>
            {
                contactInfoBuilder.Property(c => c.Address)
                    .HasColumnName("address");
                contactInfoBuilder.Property(c => c.Email)
                    .HasColumnName("email");
                contactInfoBuilder.Property(c => c.PhoneNumber)
                    .HasColumnName("phone_number");
            });
        });

        builder.Entity<MedicalRecord>()
            .Property(e => e.Status)
            .HasConversion(new EnumToStringConverter<MedicalRecordStatus>())
            .HasColumnType("VARCHAR(20)");
        base.OnModelCreating(builder);

        builder.Entity<Job>()
          .Property(e => e.EducationLevel)
          .HasConversion(new EnumToStringConverter<EducationLevel>())
          .HasColumnType("VARCHAR(30)");
        base.OnModelCreating(builder);
    }
}