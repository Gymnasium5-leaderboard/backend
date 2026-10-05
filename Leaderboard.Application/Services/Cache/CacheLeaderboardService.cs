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
    public async Task<CollectionResult<ClassLeaderboardEntryDto>> GetSchoolClassesAsync(
        CancellationToken cancellationToken = default)
    {
        var cached = await cacheRepository.GetSchoolClassesAsync(cancellationToken);
        if (cached != null) return CollectionResult<ClassLeaderboardEntryDto>.Success(cached);

        return await singleFlight.RunAsync(async () =>
        {
            var result = await inner.GetSchoolClassesAsync(CancellationToken.None);
            if (result.IsSuccess) await cacheRepository.SetSchoolClassesAsync(result.Data);

            return result;
        }, cancellationToken);
    }

    public async Task<CollectionResult<ClassLeaderboardEntryDto>> GetGradeClassesAsync(int grade,
        CancellationToken cancellationToken = default)
    {
        var cached = await cacheRepository.GetGradeClassesAsync(grade, cancellationToken);
        if (cached != null) return CollectionResult<ClassLeaderboardEntryDto>.Success(cached);

        return await singleFlight.RunAsync(async () =>
        {
            var result = await inner.GetGradeClassesAsync(grade, CancellationToken.None);
            if (result.IsSuccess) await cacheRepository.SetGradeClassesAsync(grade, result.Data);

            return result;
        }, cancellationToken, grade);
    }

    public async Task<CollectionResult<StudentLeaderboardEntryDto>> GetClassStudentsAsync(long classId,
        CancellationToken cancellationToken = default)
    {
        var cached = await cacheRepository.GetClassStudentsAsync(classId, cancellationToken);
        if (cached != null) return CollectionResult<StudentLeaderboardEntryDto>.Success(cached);

        return await singleFlight.RunAsync(async () =>
        {
            var result = await inner.GetClassStudentsAsync(classId, CancellationToken.None);
            if (result.IsSuccess) await cacheRepository.SetClassStudentsAsync(classId, result.Data);

            return result;
        }, cancellationToken, classId);
    }

    public async Task<BaseResult<StudentRankDto>> GetStudentRankAsync(long studentId,
        CancellationToken cancellationToken = default)
    {
        var cached = await cacheRepository.GetStudentRankAsync(studentId, cancellationToken);
        if (cached != null) return BaseResult<StudentRankDto>.Success(cached);

        return await singleFlight.RunAsync(async () =>
        {
            var result = await inner.GetStudentRankAsync(studentId, CancellationToken.None);
            if (result.IsSuccess) await cacheRepository.SetStudentRankAsync(result.Data);

            return result;
        }, cancellationToken, studentId);
    }
}