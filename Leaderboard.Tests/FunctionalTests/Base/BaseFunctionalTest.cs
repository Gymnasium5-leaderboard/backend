using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Leaderboard.Tests.FunctionalTests.Base;

public class BaseFunctionalTest : IClassFixture<FunctionalTestWebAppFactory>
{
    protected readonly HttpClient HttpClient;
    protected readonly IServiceProvider ServiceProvider;

    protected BaseFunctionalTest(FunctionalTestWebAppFactory factory)
    {
        HttpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            // Refresh token cookies are sent by the tests themselves
            HandleCookies = false
        });
        ServiceProvider = factory.Services;
    }
}