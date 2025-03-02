using Auth.API.Models;
using BuildingBlocks.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.API.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Gender)
            .HasDefaultValue(UserGender.Else)
            .HasConversion(ug => ug.ToString(),
                dbStatus => (UserGender)Enum.Parse(typeof(UserGender), dbStatus));
    }
}