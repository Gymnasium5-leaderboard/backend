using Leaderboard.Domain.Dtos.AcademicYear;
using Leaderboard.Domain.Results;

namespace Leaderboard.Domain.Interfaces.Service;

public interface IAcademicYearService
{
    Task<BaseResult<AcademicYearDto>> GetCurrentAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Closes the current year, graduates 11th classes, moves the other classes one grade up and opens a new year.
    /// </summary>
    Task<BaseResult<AcademicYearDto>> StartNewAsync(CancellationToken cancellationToken = default);
}