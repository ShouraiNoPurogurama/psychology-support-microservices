using Profile.API.DoctorProfiles.Models;
using Profile.API.MentalDisorders.Models;
using Profile.API.PatientProfiles.Models;

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

        builder.Entity<PatientProfile>(typeBuilder =>
        {
             typeBuilder.Property(d => d.Gender)
                .HasConversion<string>()  
                .HasMaxLength(10)
                .HasColumnName("Gender");

            typeBuilder.HasOne(p => p.MedicalHistory)
                .WithOne()
                .HasForeignKey<MedicalHistory>(m => m.PatientId);

            typeBuilder.ComplexProperty(p => p.ContactInfo, contactInfoBuilder =>
            {
                contactInfoBuilder.Property(c => c.Address)
                    .HasColumnName("Address");
                contactInfoBuilder.Property(c => c.Email)
                    .HasColumnName("Email");
                contactInfoBuilder.Property(c => c.PhoneNumber)
                    .HasColumnName("PhoneNumber");
            });
        });
            
        
        builder.Entity<DoctorProfile>(typeBuilder =>
        {
            typeBuilder.Property(d => d.Gender)
                .HasConversion<string>() 
                .HasMaxLength(10)
                .HasColumnName("Gender");

            typeBuilder.ComplexProperty(d => d.ContactInfo, contactInfoBuilder =>
            {
                contactInfoBuilder.Property(c => c.Address)
                    .HasColumnName("Address");
                contactInfoBuilder.Property(c => c.Email)
                    .HasColumnName("Email");
                contactInfoBuilder.Property(c => c.PhoneNumber)
                    .HasColumnName("PhoneNumber");
            });
        });
        base.OnModelCreating(builder);
    }
}