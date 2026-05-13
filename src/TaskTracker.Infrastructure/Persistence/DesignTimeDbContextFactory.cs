using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskTracker.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by `dotnet ef` to create the DbContext without
/// requiring the API host. Connection string is irrelevant for migration
/// scaffolding; a placeholder is used.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TaskTrackerDbContext>
{
    public TaskTrackerDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TaskTrackerDbContext>()
            .UseNpgsql("Host=localhost;Database=tasktracker_design;Username=postgres;Password=postgres",
                b => b.MigrationsAssembly(typeof(TaskTrackerDbContext).Assembly.FullName))
            .Options;

        return new TaskTrackerDbContext(options);
    }
}
