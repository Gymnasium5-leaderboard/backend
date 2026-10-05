using Leaderboard.Tests.FunctionalTests.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Leaderboard.Tests.FunctionalTests.Base;

public class SequentialFunctionalTest(FunctionalTestWebAppFactory factory) : BaseFunctionalTest(factory), IAsyncLifetime
{
    private readonly FunctionalTestWebAppFactory _factory = factory;

    public async Task InitializeAsync()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        ResetDb(scope);
        await _factory.FlushCacheAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private static void ResetDb(IServiceScope scope)
    {
        scope.PrepPopulation();
    }
}