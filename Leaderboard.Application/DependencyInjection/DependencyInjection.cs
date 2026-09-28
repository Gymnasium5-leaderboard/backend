using FluentValidation;
using Leaderboard.Application.Mappings;
using Leaderboard.Application.Services;
using Leaderboard.Application.Validators;
using Leaderboard.Domain.Entities;
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
        
        services.Scan(scan => scan.FromAssemblyOf<AuthService>()
            .AddClasses(c => c.InExactNamespaceOf<AuthService>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddValidatorsFromAssemblyContaining<CreateOwnerValidator>();
    }
}