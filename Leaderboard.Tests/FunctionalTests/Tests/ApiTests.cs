using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Security.Claims;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Score;
using Leaderboard.Domain.Results;
using Leaderboard.Tests.FunctionalTests.Base;
using Leaderboard.Tests.FunctionalTests.Helpers;
using Leaderboard.Tests.Traits;
using Microsoft.IdentityModel.JsonWebTokens;
using Xunit;

namespace Leaderboard.Tests.FunctionalTests.Tests;

[FunctionalTest]
public class ApiTests(FunctionalTestWebAppFactory factory) : BaseFunctionalTest(factory)
{
    private const string ProtectedUrl = "/api/score";

    [Fact]
    public async Task PostScore_MissingAuthToken_ReturnsUnauthorized()
    {
        //Arrange
        var dto = new ChangeScoreDto(1, 5, null);

        //Act
        var response = await HttpClient.PostAsJsonAsync(ProtectedUrl, dto);
        var body = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(MediaTypeNames.Text.Plain, response.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(body);
    }

    [Fact]
    public async Task PostScore_MissingAuthTokenFromOtherOrigin_ReturnsUnauthorizedWithCorsHeaders()
    {
        //Arrange
        var dto = new ChangeScoreDto(1, 5, null);
        using var request = new HttpRequestMessage(HttpMethod.Post, ProtectedUrl);
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Content = JsonContent.Create(dto);

        //Act
        var response = await HttpClient.SendAsync(request);

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task PostScore_InvalidClaims_ReturnsUnauthorized()
    {
        //Arrange
        var accessToken = TokenHelper.GetToken([new Claim(JwtRegisteredClaimNames.Sub, "not-an-owner-id")]);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var dto = new ChangeScoreDto(1, 5, null);

        //Act
        var response = await HttpClient.PostAsJsonAsync(ProtectedUrl, dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult>();

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(ErrorMessage.InvalidClaims, result.ErrorMessage);
    }

    [Fact]
    public async Task PostScore_WrongSigningKey_ReturnsUnauthorized()
    {
        //Arrange
        var accessToken = TokenHelper.GetToken([new Claim(JwtRegisteredClaimNames.Sub, "1")],
            "another-signing-key-that-is-at-least-32-bytes");
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var dto = new ChangeScoreDto(1, 5, null);

        //Act
        var response = await HttpClient.PostAsJsonAsync(ProtectedUrl, dto);

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostScore_ExpiredToken_ReturnsUnauthorized()
    {
        //Arrange
        var accessToken = TokenHelper.GetToken([new Claim(JwtRegisteredClaimNames.Sub, "1")],
            expiresAt: DateTime.UtcNow.AddMinutes(-5));
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var dto = new ChangeScoreDto(1, 5, null);

        //Act
        var response = await HttpClient.PostAsJsonAsync(ProtectedUrl, dto);

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSwaggerJson_Default_ReturnsOk()
    {
        //Arrange
        const string swaggerUrl = "/swagger/v1/swagger.json";

        //Act
        var response = await HttpClient.GetAsync(swaggerUrl);
        var body = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(MediaTypeNames.Application.Json, response.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(body);
    }
}