namespace Profile.API.Data.Public.Factories;

public class PublicDbContextFactory : DesignTimeDbContextFactoryBase<ProfileDbContext>
{
    protected override string MigrationConnKey => "MigrationConnectionStrings:PublicProfileDb";

    protected override ProfileDbContext CreateNewInstance(DbContextOptions<ProfileDbContext> options)
        => new ProfileDbContext(options);
}