namespace Leaderboard.Domain.Interfaces.Service;

public interface IAcademicYearInitializer
{
    /// <summary>
    ///     Creates the first academic year by the current date if there are no years yet. Called at startup.
    /// </summary>
    Task EnsureCurrentAsync(CancellationToken cancellationToken = default);
}