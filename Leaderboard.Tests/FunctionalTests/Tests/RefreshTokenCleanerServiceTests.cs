using Leaderboard.DAL;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Tests.FunctionalTests.Base;
using Leaderboard.Tests.Traits;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Leaderboard.Tests.FunctionalTests.Tests;

[FunctionalTest]
public class RefreshTokenCleanerServiceTests(FunctionalTestWebAppFactory factory) : SequentialFunctionalTest(factory)
{
    [Fact]
    public async Task RemoveExpired_ExpiredActiveAndRevokedTokens_RemovesOnlyExpired()
    {
        //Arrange
        await using var scope = ServiceProvider.CreateAsyncScope();
        var cleaner = scope.ServiceProvider.GetRequiredService<IRefreshTokenCleanerService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        //Act
        var removed = await cleaner.RemoveExpiredAsync();
        var tokens = await dbContext.Set<RefreshToken>().ToListAsync();

        //Assert
        Assert.Equal(1, removed);
        Assert.Equal(2, tokens.Count);
        Assert.All(tokens, x => Assert.True(x.ExpiresAt > DateTime.UtcNow));
        Assert.Single(tokens, x => x.RevokedAt != null);
    }
}