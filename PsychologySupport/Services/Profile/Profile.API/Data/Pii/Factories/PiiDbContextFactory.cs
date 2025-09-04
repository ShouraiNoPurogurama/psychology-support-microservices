namespace Profile.API.Data.Pii.Factories;

public class PiiDbContextFactory : DesignTimeDbContextFactoryBase<PiiDbContext>
{
    protected override string MigrationConnKey => "MigrationConnectionStrings:PiiProfileDb";

    protected override PiiDbContext CreateNewInstance(DbContextOptions<PiiDbContext> options)
        => new PiiDbContext(options);
}