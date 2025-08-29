using BuildingBlocks.Enums;
using Profile.API.Domains.Pii.Models;

namespace Profile.API.Data.Pii;

public partial class PiiDbContext : DbContext
{
    public PiiDbContext(DbContextOptions<PiiDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AliasOwnerMap> AliasOwnerMaps { get; set; }

    public virtual DbSet<PersonProfile> PersonProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasPostgresExtension("pii", "citext");

        builder.Entity<AliasOwnerMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("alias_owner_map_pkey");

            entity.ToTable("alias_owner_map", "pii");

            entity.HasIndex(e => e.AliasId, "ix_alias_owner_map_alias_id").IsUnique();

            entity.HasIndex(e => e.UserId, "ix_alias_owner_map_user_id").IsUnique();
            
            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.HasOne(e => e.PersonProfile)
                .WithOne(e => e.AliasOwnerMap)
                .HasForeignKey<AliasOwnerMap>(e => e.UserId)
                .IsRequired()
                ;
        });

        builder.Entity<PersonProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("person_profiles_pkey");

            entity.ToTable("person_profiles", "pii");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever();
            entity.Property(e => e.FullName).HasColumnName("full_name");
            entity.Property(e => e.BirthDate);
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
