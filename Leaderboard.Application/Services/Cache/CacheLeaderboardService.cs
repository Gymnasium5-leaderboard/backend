using Leaderboard.Application.Helpers;
using Leaderboard.Domain.Dtos.Leaderboard;
using Leaderboard.Domain.Interfaces.Repository.Cache;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;

namespace Leaderboard.Application.Services.Cache;

public class CacheLeaderboardService(
    ILeaderboardCacheRepository cacheRepository,
    SingleFlight singleFlight,
    ILeaderboardService inner) : ILeaderboardService
{
    public async Task<CollectionResult<ClassLeaderboardEntryDto>> GetSchoolClassesAsync(long? categoryId,
        CancellationToken cancellationToken = default)
    {
        var cached = await cacheRepository.GetSchoolClassesAsync(categoryId, cancellationToken);
        if (cached != null) return CollectionResult<ClassLeaderboardEntryDto>.Success(cached);

        return await singleFlight.RunAsync(async () =>
        {
            var result = await inner.GetSchoolClassesAsync(categoryId, CancellationToken.None);
            if (result.IsSuccess) await cacheRepository.SetSchoolClassesAsync(categoryId, result.Data);

            return result;
        }, cancellationToken, categoryId);
    }

    public async Task<CollectionResult<ClassLeaderboardEntryDto>> GetGradeClassesAsync(int grade, long? categoryId,
        CancellationToken cancellationToken = default)
    {
        var cached = await cacheRepository.GetGradeClassesAsync(grade, categoryId, cancellationToken);
        if (cached != null) return CollectionResult<ClassLeaderboardEntryDto>.Success(cached);

        return await singleFlight.RunAsync(async () =>
        {
            var result = await inner.GetGradeClassesAsync(grade, categoryId, CancellationToken.None);
            if (result.IsSuccess) await cacheRepository.SetGradeClassesAsync(grade, categoryId, result.Data);

            return result;
        }, cancellationToken, (grade, categoryId));
    }

    public async Task<CollectionResult<StudentLeaderboardEntryDto>> GetClassStudentsAsync(long classId,
        long? categoryId, CancellationToken cancellationToken = default)
    {
        var cached = await cacheRepository.GetClassStudentsAsync(classId, categoryId, cancellationToken);
        if (cached != null) return CollectionResult<StudentLeaderboardEntryDto>.Success(cached);

        return await singleFlight.RunAsync(async () =>
        {
            var result = await inner.GetClassStudentsAsync(classId, categoryId, CancellationToken.None);
            if (result.IsSuccess) await cacheRepository.SetClassStudentsAsync(classId, categoryId, result.Data);

            return result;
        }, cancellationToken, (classId, categoryId));
    }

    public async Task<BaseResult<StudentRankDto>> GetStudentRankAsync(long studentId, long? categoryId,
        CancellationToken cancellationToken = default)
    {
        var cached = await cacheRepository.GetStudentRankAsync(studentId, categoryId, cancellationToken);
        if (cached != null) return BaseResult<StudentRankDto>.Success(cached);

        return await singleFlight.RunAsync(async () =>
        {
            var result = await inner.GetStudentRankAsync(studentId, categoryId, CancellationToken.None);
            if (result.IsSuccess) await cacheRepository.SetStudentRankAsync(categoryId, result.Data);

            return result;
        }, cancellationToken, (studentId, categoryId));
    }
}