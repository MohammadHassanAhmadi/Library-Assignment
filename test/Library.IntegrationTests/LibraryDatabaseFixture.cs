using Library.Service.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Library.IntegrationTests;

public sealed class LibraryDatabaseFixture : IAsyncLifetime
{
    public string ConnectionString { get; } = new ConfigurationBuilder().AddJsonFile("appsettings.json", false)
                                                  .AddEnvironmentVariables()
                                                  .Build()
                                                  .GetConnectionString("LibraryDbTest") ??
                                              throw new InvalidOperationException(
                                                  "Connection string 'LibraryDbTest' is missing.");


    public async Task InitializeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
        await SampleDataSeeder.SeedAsync(dbContext);
    }

    public async Task DisposeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
    }

    public LibraryDbContext CreateDbContext()
    {
        return new LibraryDbContext(
            new DbContextOptionsBuilder<LibraryDbContext>()
                .UseSqlServer(ConnectionString).Options);
    }
}

[CollectionDefinition(Name)]
public sealed class LibraryDatabaseCollection : ICollectionFixture<LibraryDatabaseFixture>
{
    public const string Name = "LibraryDatabase";
}