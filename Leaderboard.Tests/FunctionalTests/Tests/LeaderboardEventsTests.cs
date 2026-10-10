using System.Net;
using System.Net.Http.Headers;
using Leaderboard.Domain.Dtos.Score;
using Leaderboard.Tests.FunctionalTests.Base;
using Leaderboard.Tests.FunctionalTests.Helpers;
using Leaderboard.Tests.Traits;
using Xunit;

namespace Leaderboard.Tests.FunctionalTests.Tests;

// Every change of the leaderboards sends the same event, so a score change stands for all of them
[FunctionalTest]
public class LeaderboardEventsTests : SequentialFunctionalTest
{
    private static readonly TimeSpan NoEventTimeout = TimeSpan.FromSeconds(1);

    public LeaderboardEventsTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TokenHelper.GetToken());
    }

    [Fact]
    public async Task ChangeScore_Subscribed_SendsChangedEvent()
    {
        //Arrange
        var dto = new ChangeScoreDto(1, 1, null);
        await using var events = await LeaderboardEventStream.ConnectAsync(HttpClient);

        //Act
        await HttpClient.ChangeScoreAsync(dto);

        //Assert
        Assert.Equal(LeaderboardEventStream.ChangedEventType, await events.ReadEventTypeAsync());
    }

    [Fact]
    public async Task ChangeScore_Failed_SendsNoEvent()
    {
        //Arrange
        var dto = new ChangeScoreDto(5, 1, null);
        await using var events = await LeaderboardEventStream.ConnectAsync(HttpClient);

        //Act
        var response = await HttpClient.ChangeScoreAsync(dto);

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Null(await events.ReadEventTypeAsync(NoEventTimeout));
    }

    [Fact]
    public async Task ChangeScore_OneOfTwoSubscribersDisconnected_SendsChangedEventToOther()
    {
        //Arrange
        var dto = new ChangeScoreDto(1, 1, null);
        await using var events = await LeaderboardEventStream.ConnectAsync(HttpClient);
        var disconnected = await LeaderboardEventStream.ConnectAsync(HttpClient);
        await disconnected.DisposeAsync();

        //Act
        var response = await HttpClient.ChangeScoreAsync(dto);

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(LeaderboardEventStream.ChangedEventType, await events.ReadEventTypeAsync());
    }
}