using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Leaderboard.Tests.FunctionalTests.Base.Exception;

public class ExceptionBaseFunctionalTest : IClassFixture<ExceptionFunctionalTestWebAppFactory>
{
    protected readonly HttpClient HttpClient;
    protected readonly IServiceProvider ServiceProvider;

    protected ExceptionBaseFunctionalTest(ExceptionFunctionalTestWebAppFactory factory)
    {
        HttpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            // Refresh token cookies are sent by the tests themselves
            HandleCookies = false
        });
        ServiceProvider = factory.Services;
    }
}