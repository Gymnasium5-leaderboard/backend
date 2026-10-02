using Leaderboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Application.Extensions;

public static class AcademicYearExtensions
{
    /// <summary>
    ///     Id of the current academic year, or null if there is none.
    /// </summary>
    public static Task<long?> GetCurrentIdAsync(this IQueryable<AcademicYear> years,
        CancellationToken cancellationToken = default)
    {
        return years
            .Where(x => x.FinishedAt == null)
            .Select(x => (long?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}