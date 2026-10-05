using Leaderboard.Domain.Dtos.AcademicYear;
using Leaderboard.Domain.Interfaces.Repository.Cache;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;

namespace Leaderboard.Application.Services.Cache;

public class CacheAcademicYearService(
    IAcademicYearCacheRepository cacheRepository,
    IClassCacheRepository classCacheRepository,
    IAcademicYearService inner) : IAcademicYearService
{
    public async Task<BaseResult<AcademicYearDto>> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var cached = await cacheRepository.GetCurrentAsync(cancellationToken);
        if (cached != null) return BaseResult<AcademicYearDto>.Success(cached);

        var result = await inner.GetCurrentAsync(cancellationToken);
        if (result.IsSuccess) await cacheRepository.SetCurrentAsync(result.Data);

        return result;
    }

    public async Task<BaseResult<AcademicYearDto>> StartNewAsync(CancellationToken cancellationToken = default)
    {
        var result = await inner.StartNewAsync(cancellationToken);
        if (!result.IsSuccess) return result;

        // Every class has moved up a grade or graduated
        await classCacheRepository.RemoveAllAsync();
        await cacheRepository.RemoveCurrentAsync();

        return result;
    }
}