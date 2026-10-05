using Leaderboard.Domain.Dtos.AcademicYear;

namespace Leaderboard.Domain.Interfaces.Repository.Cache;

/// <summary>
///     Cache errors are logged, not thrown: a failed read is a miss.
/// </summary>
public interface IAcademicYearCacheRepository
{
    /// <summary>
    ///     The current academic year, or null on a cache miss.
    /// </summary>
    Task<AcademicYearDto?> GetCurrentAsync(CancellationToken cancellationToken = default);

    Task SetCurrentAsync(AcademicYearDto year);

    Task RemoveCurrentAsync();
}