using Library.Service.Data;
using Library.Service.Grpc;
using Microsoft.EntityFrameworkCore;

namespace Library.Service;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddScoped<ILibraryReport, LibraryReports>();
       
        builder.Services.AddGrpc();
        var connectionString = builder.Configuration.GetConnectionString("LibraryDb") ??
                               throw new InvalidOperationException("Connection string 'LibraryDb' is missing.");
        builder.Services.AddDbContext<LibraryDbContext>(option => option.UseSqlServer(connectionString));
        builder.Services.AddScoped<LibraryReports>();
        var app = builder.Build();

        await InitializeDatabase(app);
        app.MapGrpcService<LibraryReportsGrpcService>();
       
        
        app.MapGet("/",
            () =>
                "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        await app.RunAsync();
    }

    private static async Task InitializeDatabase(WebApplication app)
    {
        try
        {
            
            await using var scope = app.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            
            var connection = dbContext.Database.GetDbConnection();
            
            app.Logger.LogInformation(
                "Initializing database '{DatabaseName}' on '{DataSource}'",
                connection.Database,
                connection.DataSource);

            await dbContext.Database.MigrateAsync();

            await SampleDataSeeder.SeedAsync(dbContext, CancellationToken.None);
            app.Logger.LogInformation("Database initialization completed.");
        }
        catch (Exception e)
        {
            app.Logger.LogCritical(e, "Database initialization failed.");
            throw;
        }
    }
}

