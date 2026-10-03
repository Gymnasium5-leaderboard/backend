using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Leaderboard.Api.Dtos;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Auth;
using Leaderboard.Domain.Results;
using Leaderboard.Tests.Constants;
using Leaderboard.Tests.FunctionalTests.Base;
using Leaderboard.Tests.FunctionalTests.Helpers;
using Leaderboard.Tests.Traits;
using Xunit;

namespace Leaderboard.Tests.FunctionalTests.Tests;

[FunctionalTest]
public class AuthServiceTests(FunctionalTestWebAppFactory factory) : SequentialFunctionalTest(factory)
{
    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithRefreshTokenCookie()
    {
        //Arrange
        var dto = new LoginDto("testowner1", TestConstants.TestPassword + "1");

        //Act
        var response = await HttpClient.PostAsJsonAsync("/api/auth/login", dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<AccessTokenDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data.AccessToken);
        Assert.NotNull(response.GetRefreshToken());
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        //Arrange
        var dto = new LoginDto("testowner1", TestConstants.WrongPassword);

        //Act
        var response = await HttpClient.PostAsJsonAsync("/api/auth/login", dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<AccessTokenDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(ErrorMessage.InvalidCredentials, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task Refresh_ValidToken_ReturnsOkWithNewToken()
    {
        //Act
        var response = await HttpClient.PostWithRefreshTokenAsync("/api/auth/refresh", TestConstants.RefreshToken);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<AccessTokenDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotNull(response.GetRefreshToken());
        Assert.NotEqual(TestConstants.RefreshToken, response.GetRefreshToken());
    }

    [Fact]
    public async Task Refresh_UsedToken_ReturnsUnauthorized()
    {
        //Arrange
        await HttpClient.PostWithRefreshTokenAsync("/api/auth/refresh", TestConstants.RefreshToken);

        //Act
        var response = await HttpClient.PostWithRefreshTokenAsync("/api/auth/refresh", TestConstants.RefreshToken);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<AccessTokenDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(ErrorMessage.InvalidRefreshToken, result.ErrorMessage);
    }

    [Fact]
    public async Task Refresh_ParallelRequestsWithSameToken_ReturnsOkOnceAndUnauthorizedForOthers()
    {
        //Act
        var responses = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ =>
            HttpClient.PostWithRefreshTokenAsync("/api/auth/refresh", TestConstants.RefreshToken)));

        //Assert
        Assert.Single(responses, x => x.StatusCode == HttpStatusCode.OK);
        Assert.Equal(4, responses.Count(x => x.StatusCode == HttpStatusCode.Unauthorized));
    }

    [Fact]
    public async Task Refresh_MissingCookie_ReturnsUnauthorized()
    {
        //Act
        var response = await HttpClient.PostAsync("/api/auth/refresh", null);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<AccessTokenDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(ErrorMessage.InvalidRefreshToken, result!.ErrorMessage);
    }

    [Fact]
    public async Task Logout_ValidToken_ReturnsNoContentAndRevokesToken()
    {
        //Act
        var response = await HttpClient.PostWithRefreshTokenAsync("/api/auth/logout", TestConstants.RefreshToken);
        var refreshResponse =
            await HttpClient.PostWithRefreshTokenAsync("/api/auth/refresh", TestConstants.RefreshToken);

        //Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_ValidPassword_ReturnsNoContentAndRevokesRefreshTokens()
    {
        //Arrange
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TokenHelper.GetToken());
        var dto = new ChangePasswordDto(TestConstants.TestPassword + "1", TestConstants.NewPassword);

        //Act
        var response = await HttpClient.PutAsJsonAsync("/api/owner/me/password", dto);
        var refreshResponse =
            await HttpClient.PostWithRefreshTokenAsync("/api/auth/refresh", TestConstants.RefreshToken);
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/auth/login",
            new LoginDto("testowner1", TestConstants.NewPassword));

        //Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_ReturnsUnauthorized()
    {
        //Arrange
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TokenHelper.GetToken());
        var dto = new ChangePasswordDto(TestConstants.WrongPassword, TestConstants.NewPassword);

        //Act
        var response = await HttpClient.PutAsJsonAsync("/api/owner/me/password", dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult>();

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(ErrorMessage.WrongCurrentPassword, result.ErrorMessage);
    }
}