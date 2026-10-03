using System.Security.Cryptography;
using System.Text;
using Leaderboard.Domain.Entities;
using Leaderboard.Tests.Constants;

namespace Leaderboard.Tests.TestData;

internal static class RefreshTokenMother
{
    public static IQueryable<RefreshToken> GetRefreshTokens()
    {
        return new RefreshToken[]
        {
            new()
            {
                Id = 1, OwnerId = 1, TokenHash = HashToken(TestConstants.RefreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            },
            new()
            {
                Id = 2, OwnerId = 1, TokenHash = HashToken(TestConstants.RevokedRefreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(30), RevokedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = 3, OwnerId = 1, TokenHash = HashToken(TestConstants.ExpiredRefreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(-1)
            }
        }.AsQueryable();
    }

    // The same hash as AuthService stores
    private static string HashToken(string token)
    {
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}