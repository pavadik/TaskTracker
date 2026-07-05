using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using TaskTracker.Infrastructure.Persistence;

namespace TaskTracker.Tests.Integration;

public class TaskTrackerWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("tasktracker_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RabbitMQ:Host"] = string.Empty
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the app's DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TaskTrackerDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // Add DbContext using the test container
            services.AddDbContext<TaskTrackerDbContext>(options =>
                options.UseNpgsql(_postgresContainer.GetConnectionString()));

            // Configure test Redis
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = _redisContainer.GetConnectionString();
                options.InstanceName = "TaskTrackerTest:";
            });

            services.RemoveMassTransitHostedService();
            services.Configure<HealthCheckServiceOptions>(options =>
            {
                var massTransitChecks = options.Registrations
                    .Where(registration => registration.Name.StartsWith("masstransit", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var registration in massTransitChecks)
                {
                    options.Registrations.Remove(registration);
                }
            });

            // Create the schema
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TaskTrackerDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Test");
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _redisContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
    }
}
