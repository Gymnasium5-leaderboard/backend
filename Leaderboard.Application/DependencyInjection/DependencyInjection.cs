using FluentValidation;
using Leaderboard.Application.Helpers;
using Leaderboard.Application.Mappings;
using Leaderboard.Application.Services;
using Leaderboard.Application.Services.Cache;
using Leaderboard.Application.Validators;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Leaderboard.Application.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
        services.AddAutoMapper(typeof(OwnerMapping));
        services.InitServices();
    }

    private static void InitServices(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher<LeaderboardOwner>, PasswordHasher<LeaderboardOwner>>();
        services.AddSingleton<SingleFlight>();

        services.Scan(scan => scan.FromAssemblyOf<AuthService>()
            .AddClasses(c => c.InExactNamespaceOf<AuthService>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.Decorate<IClassService, CacheClassService>();
        services.Decorate<IAcademicYearService, CacheAcademicYearService>();
        services.Decorate<IStudentService, CacheStudentService>();
        services.Decorate<IScoreService, CacheScoreService>();
        services.Decorate<ILeaderboardService, CacheLeaderboardService>();

        services.AddValidatorsFromAssemblyContaining<CreateOwnerValidator>();
    }
}