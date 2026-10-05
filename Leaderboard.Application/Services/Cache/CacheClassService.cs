using Leaderboard.Domain.Dtos.Class;
using Leaderboard.Domain.Interfaces.Repository.Cache;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;
using Leaderboard.Domain.Settings;

namespace Leaderboard.Application.Services.Cache;

public class CacheClassService(IClassCacheRepository cacheRepository, IClassService inner) : IClassService
{
    public async Task<CollectionResult<ClassDto>> GetAllAsync(int? grade,
        CancellationToken cancellationToken = default)
    {
        // A grade out of range has no classes, and caching it would let anyone fill Redis with empty lists
        if (grade is < EntityConstraints.MinGrade or > EntityConstraints.MaxGrade)
            return await inner.GetAllAsync(grade, cancellationToken);

        var cached = await cacheRepository.GetByGradeAsync(grade, cancellationToken);
        if (cached != null) return CollectionResult<ClassDto>.Success(cached);

        var result = await inner.GetAllAsync(grade, cancellationToken);
        if (result.IsSuccess) await cacheRepository.SetByGradeAsync(grade, result.Data);

        return result;
    }

    public async Task<BaseResult<ClassDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var cached = await cacheRepository.GetAsync(id, cancellationToken);
        if (cached != null) return BaseResult<ClassDto>.Success(cached);

        var result = await inner.GetByIdAsync(id, cancellationToken);
        if (result.IsSuccess) await cacheRepository.SetAsync(result.Data);

        return result;
    }

    public async Task<BaseResult<ClassDto>> CreateAsync(CreateClassDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await inner.CreateAsync(dto, cancellationToken);
        // The new class is not cached yet, only the lists of its grade miss it
        if (result.IsSuccess) await cacheRepository.RemoveGradesAsync([result.Data.Grade]);

        return result;
    }

    public async Task<BaseResult<ClassDto>> UpdateAsync(long id, UpdateClassDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await inner.UpdateAsync(id, dto, cancellationToken);
        if (result.IsSuccess) await cacheRepository.RemoveAsync(id);

        return result;
    }
}