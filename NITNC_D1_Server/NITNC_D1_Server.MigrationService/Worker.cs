using System.Diagnostics;
using NITNC_D1_Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using OpenTelemetry.Trace;

namespace NITNC_D1_Server.MigrationService;
public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = s_activitySource.StartActivity("Migrating database", ActivityKind.Client);
        var count = 0;
retry:
        try
        {
            Console.WriteLine("Trying migrate: "+count);
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await EnsureDatabaseAsync(dbContext, cancellationToken);
            await dbContext.Database.MigrateAsync(cancellationToken);
            //await RunMigrationAsync(dbContext, cancellationToken);
            //await SeedDataAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);
            Console.WriteLine(ex.ToString());
            if (count >= 2)
            {
                throw;
                
            }
            Thread.Sleep(10000);
            count++;
            goto retry;
        }

        hostApplicationLifetime.StopApplication();
    }

    private static async Task EnsureDatabaseAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Create the database if it does not exist.
            // Do this first so there is then a database to start a transaction against.
            if (!await dbCreator.ExistsAsync(cancellationToken))
            {
                await dbCreator.CreateAsync(cancellationToken);
            }
        });
    }

    private static async Task RunMigrationAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Run migration in a transaction to avoid partial migration if it fails.
            //await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await dbContext.Database.MigrateAsync(cancellationToken);
            //await transaction.CommitAsync(cancellationToken);
        });
    }

    private static async Task SeedDataAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Seed the database
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            //await dbContext.Tickets.AddAsync(firstTicket, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }
}