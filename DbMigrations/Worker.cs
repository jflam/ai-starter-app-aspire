using Data;
using Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace DbMigrations;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> logger;
    private readonly IServiceProvider serviceProvider;
    private readonly IHostEnvironment hostEnvironment;
    private readonly IHostApplicationLifetime hostApplicationLifetime;
    private readonly ActivitySource _activitySource;

    public Worker(
        ILogger<Worker> logger,
        IServiceProvider serviceProvider,
        IHostEnvironment hostEnvironment,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
        this.hostEnvironment = hostEnvironment;
        this.hostApplicationLifetime = hostApplicationLifetime;
        this._activitySource = new ActivitySource(hostEnvironment.ApplicationName);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity(hostEnvironment.ApplicationName, ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PetBnBDbContext>();

            await EnsureDatabaseAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private async Task EnsureDatabaseAsync(PetBnBDbContext dbContext, CancellationToken cancellationToken)
    {
        var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            bool? dbExists = null;
            try { dbExists = await dbCreator.ExistsAsync(cancellationToken); }
            catch (Exception exception) {
                logger.LogError("Error during ExistsAsync {0}", exception);
            }

            if(dbExists.HasValue && !dbExists.Value)
            {
                try
                {
                    await dbCreator.CreateAsync(cancellationToken);
                    dbExists = await dbCreator.ExistsAsync(cancellationToken);
                }
                catch (Exception exception)
                {
                    logger.LogError("Error during CreateAsync {0}", exception);
                }
            }

            if(dbExists.HasValue && dbExists.Value)
            {
                try
                {
                    await dbContext.Database.MigrateAsync(cancellationToken);

                    using var migrationScope = serviceProvider.CreateScope();
                    var seeder = migrationScope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
                    await seeder.SeedDatabase();
                }
                catch (Exception exception)
                {
                    logger.LogError("Error during migrations {0}", exception);
                    throw;
                }
            }
        });
    }
}
