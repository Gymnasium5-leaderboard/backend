using AutoMapper;
using Leaderboard.Application.Mappings;

namespace Leaderboard.Tests.UnitTests.Fixtures;

internal static class MapperFixture
{
    public static IMapper GetMapperConfiguration()
    {
        var mockMapper = new MapperConfiguration(cfg => cfg.AddMaps(typeof(OwnerMapping)));
        return mockMapper.CreateMapper();
    }
}