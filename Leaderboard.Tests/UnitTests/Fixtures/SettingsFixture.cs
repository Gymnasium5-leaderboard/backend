using Leaderboard.Domain.Settings;
using Microsoft.Extensions.Options;

namespace Leaderboard.Tests.UnitTests.Fixtures;

internal static class SettingsFixture
{
    public const int MaxScoreDelta = 100;

    public static IOptions<BusinessRules> GetBusinessRules(bool allowNegativeScore = false)
    {
        return Options.Create(new BusinessRules
        {
            MaxScoreDelta = MaxScoreDelta,
            AllowNegativeScore = allowNegativeScore
        });
    }

    public static IOptions<JwtSettings> GetJwtSettings()
    {
        return Options.Create(new JwtSettings
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SigningKey = "unit-tests-signing-key-at-least-32-bytes",
            AccessTokenLifetimeMinutes = 15,
            RefreshTokenLifetimeDays = 30
        });
    }
}