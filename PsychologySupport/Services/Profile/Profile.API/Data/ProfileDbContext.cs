using Microsoft.EntityFrameworkCore;
using Profile.API.Models;

namespace Profile.API.Data;

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


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("public");

        builder.Entity<PatientProfile>()
            .HasOne(p => p.MedicalHistory)
            .WithOne(m => m.PatientProfile)
            .HasForeignKey<MedicalHistory>(m => m.PatientId);
        
        base.OnModelCreating(builder);
    }
}