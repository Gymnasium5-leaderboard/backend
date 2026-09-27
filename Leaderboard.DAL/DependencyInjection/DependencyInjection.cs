using Leaderboard.DAL.Repositories;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Leaderboard.DAL.DependencyInjection;

public static class DependencyInjection
{
    public static void AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgresSQL");
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

        services.InitRepositories();
    }

    /// <summary>
    ///     Applies pending migrations.
    /// </summary>
    public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    private static void InitRepositories(this IServiceCollection services)
    {
        services.AddBaseRepositories(typeof(AcademicYear), typeof(LeaderboardOwner), typeof(RefreshToken),
            typeof(SchoolClass), typeof(ScoreTransaction), typeof(Student));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddBaseRepositories(this IServiceCollection services, params Type[] entityTypes)
    {
        foreach (var entityType in entityTypes)
        {
            var interfaceType = typeof(IBaseRepository<>).MakeGenericType(entityType);
            var implementationType = typeof(BaseRepository<>).MakeGenericType(entityType);

            services.AddScoped(interfaceType, implementationType);
        }
    }
}