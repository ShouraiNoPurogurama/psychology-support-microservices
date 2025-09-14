using Profile.API.Enums.Pii;
using Profile.API.Models.Pii;
using Profile.API.ValueObjects.Pii;

namespace Profile.API.Data.Pii;

public partial class PiiDbContext : DbContext
{
    public PiiDbContext(DbContextOptions<PiiDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AliasOwnerMap> AliasOwnerMaps { get; set; }
    public virtual DbSet<PatientOwnerMap> PatientOwnerMaps { get; set; }

    public virtual DbSet<PersonProfile> PersonProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("pii");
        builder.HasPostgresExtension("pii", "citext");

        builder.Entity<AliasOwnerMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("alias_owner_map_pkey");

            entity.ToTable("alias_owner_map", "pii");

            entity.HasIndex(e => e.AliasId, "ix_alias_owner_map_alias_id").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.HasOne(e => e.PersonProfile)
                .WithOne(e => e.AliasOwnerMap)
                .HasForeignKey<AliasOwnerMap>(e => e.SubjectRef)
                .IsRequired();
        });

        builder.Entity<PatientOwnerMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("patient_owner_map_pkey");

            entity.ToTable("patient_owner_map", "pii");

            entity.HasIndex(e => e.PatientProfileId, "ix_patient_owner_map_patient_profile_id").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.HasOne<PersonProfile>()
                .WithOne()
                .HasForeignKey<PatientOwnerMap>(e => e.SubjectRef)
                .IsRequired();
        });

        builder.Entity<PersonProfile>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("subject_ref");

            entity.ToTable("person_profiles", "pii");

            entity.Property(e => e.FullName)
                .HasConversion(e => e.Value,
                    dbValue => PersonName.Of(dbValue));

            entity.Property(e => e.Status)
                .HasConversion(e => e.ToString(),
                    dbStatus => (PersonProfileStatus)Enum.Parse(typeof(PersonProfileStatus), dbStatus))
                .HasSentinel(PersonProfileStatus.Pending)
                .HasDefaultValue(PersonProfileStatus.Pending)
                ;


            entity.Property(e => e.UserId)
                .ValueGeneratedNever();

            entity.HasIndex(e => e.UserId).IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()");

            entity.ComplexProperty(d => d.ContactInfo, contactInfoBuilder =>
            {
                contactInfoBuilder.Property(c => c.Address)
                    .HasColumnName("address");
                contactInfoBuilder.Property(c => c.Email)
                    .HasColumnName("email");
                contactInfoBuilder.Property(c => c.PhoneNumber)
                    .HasColumnName("phone_number");
            });

            entity.Property(e => e.Gender)
                .HasDefaultValue(UserGender.Else)
                .HasConversion(s => s.ToString(),
                    dbStatus => (UserGender)Enum.Parse(typeof(UserGender), dbStatus));
        });


        OnModelCreatingPartial(builder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}