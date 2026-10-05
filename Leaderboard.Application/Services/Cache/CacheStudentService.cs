using Leaderboard.Domain.Dtos.Student;
using Leaderboard.Domain.Interfaces.Repository.Cache;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;

namespace Leaderboard.Application.Services.Cache;

/// <summary>
///     Students are not cached. Any change of them removes the cached leaderboards.
/// </summary>
public class CacheStudentService(ILeaderboardCacheRepository leaderboardCacheRepository, IStudentService inner)
    : IStudentService
{
    public Task<CollectionResult<StudentDto>> GetAllAsync(long? classId,
        CancellationToken cancellationToken = default)
    {
        return inner.GetAllAsync(classId, cancellationToken);
    }

    public Task<BaseResult<StudentDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return inner.GetByIdAsync(id, cancellationToken);
    }

    public async Task<BaseResult<StudentDto>> CreateAsync(CreateStudentDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await inner.CreateAsync(dto, cancellationToken);
        if (result.IsSuccess) await leaderboardCacheRepository.RemoveAllAsync();

        return result;
    }

    public async Task<CollectionResult<StudentDto>> CreateManyAsync(IReadOnlyCollection<CreateStudentDto> dtos,
        CancellationToken cancellationToken = default)
    {
        var result = await inner.CreateManyAsync(dtos, cancellationToken);
        if (result.IsSuccess) await leaderboardCacheRepository.RemoveAllAsync();

        return result;
    }

    public async Task<BaseResult<StudentDto>> UpdateAsync(long id, UpdateStudentDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await inner.UpdateAsync(id, dto, cancellationToken);
        if (result.IsSuccess) await leaderboardCacheRepository.RemoveAllAsync();

        return result;
    }

    public async Task<BaseResult<StudentDto>> TransferAsync(long id, TransferStudentDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await inner.TransferAsync(id, dto, cancellationToken);
        if (result.IsSuccess) await leaderboardCacheRepository.RemoveAllAsync();

        return result;
    }

    public async Task<BaseResult> DeactivateAsync(long id, CancellationToken cancellationToken = default)
    {
        var result = await inner.DeactivateAsync(id, cancellationToken);
        if (result.IsSuccess) await leaderboardCacheRepository.RemoveAllAsync();

        return result;
    }
}