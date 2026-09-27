using System.Reflection;
using Leaderboard.DAL.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace Leaderboard.DAL;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ILogger logger) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(logger.Information, LogLevel.Information);
        optionsBuilder.AddInterceptors(new DateInterceptor());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
