using Leaderboard.Domain.Entities;
using Leaderboard.Tests.Constants;
using Microsoft.AspNetCore.Identity;

namespace Leaderboard.Tests.TestData;

internal static class OwnerMother
{
    // Hashing is slow, so each password is hashed once
    private static readonly string[] PasswordHashes =
    [
        new PasswordHasher<LeaderboardOwner>().HashPassword(null!, TestConstants.TestPassword + "1"),
        new PasswordHasher<LeaderboardOwner>().HashPassword(null!, TestConstants.TestPassword + "2")
    ];

    public static IQueryable<LeaderboardOwner> GetOwners()
    {
        return new LeaderboardOwner[]
        {
            new()
            {
                Id = 1, // the seeded owner, see LeaderboardOwnerConfiguration
                Login = "testowner1",
                PasswordHash = PasswordHashes[0], // TestPassword1
                FirstName = "Owner1",
                LastName = "Test",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                Login = "testowner2",
                PasswordHash = PasswordHashes[1], // TestPassword2
                FirstName = "Owner2",
                LastName = "Test",
                CreatedAt = DateTime.UtcNow
            }
        }.AsQueryable();
    }
}